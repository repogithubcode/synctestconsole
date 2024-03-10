using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ProEstimatorData
{
    public static class InputHelper
    {

        public static string GetString(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : input;
        }

        /// <summary>
        /// Get a Decimal from a user input string, which might not be a valid decimal.  
        /// </summary>
        public static decimal GetDecimal(string input, decimal defaultValue = 0)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            try
            {
                decimal d = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
                return d;
            }
            catch { }

            return defaultValue;

            //input = Regex.Replace(input, "[^0-9.-]", "");
            //decimal result = 0;
            //decimal.TryParse(input, out result);
            //return result;
        }

        public static double GetDouble(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }

            input = Regex.Replace(input, "[^0-9.-]", "");
            double result = 0;
            double.TryParse(input, out result);
            return result;
        }

        public static int GetInteger(string input)
        {
            return GetInteger(input, 0);
        }

        public static int GetInteger(string input, int defaultValue)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            double inputDouble = GetDouble(input);
            return (int)inputDouble;
        }

        public static Single GetSingle(string input)
        {
            return GetSingle(input, 0);
        }

        public static Single GetSingle(string input, Single defaultValue)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            double inputDouble = GetDouble(input);
            return (Single)inputDouble;
        }

        public static long GetLong(string input)
        {
            return GetLong(input, 0);
        }

        public static long GetLong(string input, long defaultValue)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            input = Regex.Replace(input, "[^0-9-]", "");
            long result = defaultValue;
            long.TryParse(input, out result);
            return result;
        }

        public static bool GetBoolean(string input)
        {
            return GetBoolean(input, false);
        }

        public static bool GetBoolean(string input, bool defaultValue)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            if (input.ToLower() == "true" || input == "1")
            {
                return true;
            }

            return false;
        }

        public static DateTime? GetNullableDateTime(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            try
            {
                DateTime result = DateTime.Parse(input);
                return result;
            }
            catch 
            {
                return null;
            }
        }

        public static DateTime GetDateTime(string input)
        {
            return GetDateTime(input, DateTime.Now);
        }

        public static DateTime GetDateTime(string input, DateTime defaultValue)
        {
            try
            {
                DateTime result = DateTime.Parse(input);
                return result;
            } catch { }

            return defaultValue;
        }

        /// <summary>
        /// Returns null if the input is not a valid DateTime.
        /// </summary>
        public static DateTime? GetDateTimeNullable(string input)
        {
            try
            {
                DateTime result = DateTime.Parse(input);
                return result;
            }
            catch { }

            return null;
        }

        public static string GetNumbersOnly(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            return new String(input.Where(Char.IsDigit).ToArray());
        }

        public static string RemoveInvalidPathCharacters(string input)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c.ToString(), "");
            }

            return input;
        }

        public static bool IsValidEmail(string email)
        {
            Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.CultureInvariant | RegexOptions.Singleline);
            return regex.IsMatch(email);
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            phoneNumber = PhoneNumberStrip(phoneNumber);
            return phoneNumber.Length == 10;
        }

        public static string FormatPhone(string phoneNumber)
        {
            phoneNumber = PhoneNumberStrip(phoneNumber);

            if (phoneNumber.Length == 10)
            {
                return "(" + phoneNumber.Substring(0, 3) + ") " + phoneNumber.Substring(3, 3) + "-" + phoneNumber.Substring(6, 4);
            }

            return phoneNumber;
        }

        private static string PhoneNumberStrip(string phoneNumber)
        {
            phoneNumber = GetNumbersOnly(phoneNumber);
            if (phoneNumber.Length > 10)
            {
                phoneNumber = phoneNumber.Substring(phoneNumber.Length - 10, 10);
            }

            return phoneNumber;
        }

        /// <summary>
        /// Add a span around all matches of the search words in the input for hilighting.
        /// </summary>
        public static string HilightWords(string input, List<string> searchWords)
        {
            input = input.ToLower();

            foreach (string word in searchWords)
            {
                input = HilightWord(input, word);
            }

            return input;
        }

        /// <summary>
        /// Wrap a span around the word found in the input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string HilightWord(string input, string word)
        {
            int index = input.IndexOf(word);
            if (index > -1)
            {
                string pre = input.Substring(0, index);
                string hilight = input.Substring(index, word.Length);
                string post = input.Substring(index + word.Length, input.Length - (index + word.Length));

                input = pre + "<span class='search-match'>" + hilight + "</span>" + post;
            }

            return input;
        }

        public static string GetHash(string text, int length)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                string fullHash = BitConverter.ToString(hash).Replace("-", String.Empty);

                if (fullHash.Length > length)
                {
                    fullHash = fullHash.Substring(0, length);
                }

                return fullHash;
            }
        }

        /// <summary>
        /// Get a float from a user input string, which might not be a valid decimal.  
        /// </summary>
        public static float GetFloat(string input, float defaultValue = 0)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            try
            {
                float d = float.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
                return d;
            }
            catch { }

            return defaultValue;
        }

        public static List<string> SearchOneLetterDifferent(string search, List<string> strings)
        {
            var results = new List<string>();
            foreach (var str in strings)
            {
                if (str.Length != search.Length)
                    continue; // skip strings with different length
                int differences = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] != search[i])
                    {
                        if (++differences > 1)
                            break; // too many differences, move on to the next string
                                   // check if the only difference is one letter
                        if (i == str.Length - 1 || str.Substring(i + 1) == search.Substring(i + 1))
                            results.Add(str);
                    }
                }
            }
            return results;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}