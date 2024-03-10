using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ProEstimatorData
{
    public static class QuickbookEncryptor
    {

        private static Aes _encryptor;

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return "";
            }

            Initialize();

            var clearBytes = Encoding.Unicode.GetBytes(plainText);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, _encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                }
                plainText = Convert.ToBase64String(ms.ToArray());
            }

            return plainText;
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return "";
            }

            Initialize();

            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, _encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
            return cipherText;
        }

        private static void Initialize()
        {
            if (_encryptor == null)
            {
                _encryptor = Aes.Create();
                var pdb = new Rfc2898DeriveBytes("m8*l4qp2(%k)", new byte[]
                    {
                0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                    });

                _encryptor.Key = pdb.GetBytes(32);
                _encryptor.IV = pdb.GetBytes(16);
            }
        }
    }
}
