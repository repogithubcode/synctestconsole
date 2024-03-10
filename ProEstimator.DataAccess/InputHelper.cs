using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ProEstimator.DataAccess
{
    public static class InputHelper
    {

        /// <summary>
        /// Get a Decimal from a user input string, which might not be a valid decimal.  
        /// </summary>
        public static decimal GetDecimal(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }

            input = Regex.Replace(input, "[^0-9.-]", "");
            decimal result = 0;
            decimal.TryParse(input, out result);
            return result;
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

        public static string GetString(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : input;
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
    }
}