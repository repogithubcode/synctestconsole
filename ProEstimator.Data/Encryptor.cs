using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ProEstimatorData
{
    public static class Encryptor
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
                var pdb = new Rfc2898DeriveBytes("l7&j3po1*$j(", new byte[]
                    {
                0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                    });

                _encryptor.Key = pdb.GetBytes(32);
                _encryptor.IV = pdb.GetBytes(16);
            }
        }

        // Got this here: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
        // Ezra - 8/10/21 - This is pretty slow (30 ms about).  We want to encrypt passwords through here so using something faster above, this is for history

        //// This constant is used to determine the keysize of the encryption algorithm in bits.
        //// We divide this by 8 within the code below to get the equivalent number of bytes.
        //private const int Keysize = 256;

        //// This constant determines the number of iterations for the password bytes generation function.
        //private const int DerivationIterations = 1000;

        //public static string Encrypt(string plainText)
        //{
        //    return Encrypt(plainText, "L$94920jfLJF(A#");
        //}

        //public static string Encrypt(string plainText, string 930)
        //{
        //    // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        //    // so that the same Salt and IV values can be used when decrypting.  
        //    var saltStringBytes = Generate256BitsOfRandomEntropy();
        //    var ivStringBytes = Generate256BitsOfRandomEntropy();
        //    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        //    using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        //    {
        //        var keyBytes = password.GetBytes(Keysize / 8);
        //        using (var symmetricKey = new RijndaelManaged())
        //        {
        //            symmetricKey.BlockSize = 256;
        //            symmetricKey.Mode = CipherMode.CBC;
        //            symmetricKey.Padding = PaddingMode.PKCS7;
        //            using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //                    {
        //                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        //                        cryptoStream.FlushFinalBlock();
        //                        // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
        //                        var cipherTextBytes = saltStringBytes;
        //                        cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
        //                        cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
        //                        memoryStream.Close();
        //                        cryptoStream.Close();
        //                        return Convert.ToBase64String(cipherTextBytes);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public static string Decrypt(string encryptedText)
        //{
        //    return Decrypt(encryptedText, "L$94920jfLJF(A#");
        //}

        //public static string Decrypt(string encryptedText, string passPhrase)
        //{
        //    // Get the complete stream of bytes that represent:
        //    // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        //    var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(encryptedText);
        //    // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        //    var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        //    // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        //    var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        //    // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        //    var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        //    using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        //    {
        //        var keyBytes = password.GetBytes(Keysize / 8);
        //        using (var symmetricKey = new RijndaelManaged())
        //        {
        //            symmetricKey.BlockSize = 256;
        //            symmetricKey.Mode = CipherMode.CBC;
        //            symmetricKey.Padding = PaddingMode.PKCS7;
        //            using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
        //            {
        //                using (var memoryStream = new MemoryStream(cipherTextBytes))
        //                {
        //                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        //                    {
        //                        var plainTextBytes = new byte[cipherTextBytes.Length];
        //                        var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        //                        memoryStream.Close();
        //                        cryptoStream.Close();
        //                        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private static byte[] Generate256BitsOfRandomEntropy()
        //{
        //    var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        //    using (var rngCsp = new RNGCryptoServiceProvider())
        //    {
        //        // Fill the array with cryptographically secure random bytes.
        //        rngCsp.GetBytes(randomBytes);
        //    }
        //    return randomBytes;
        //}
    }
}
