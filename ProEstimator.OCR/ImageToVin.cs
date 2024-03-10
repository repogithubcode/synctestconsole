using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
//using tessnet2;

namespace ProEstimator.OCR
{
    public class GetVinResults
    {
        public string VIN { get; private set; }
        public Bitmap HilightBitmap { get; private set; }

        public GetVinResults(string vin)
        {
            VIN = vin;
            HilightBitmap = null;
        }

        public GetVinResults(string vin, Bitmap hilightBitmap)
        {
            VIN = vin;
            HilightBitmap = hilightBitmap;
        }
    }

    public static class ImageToVin
    {
        //private static Tesseract _ocr;

        public static GetVinResults GetVinFromImage(Bitmap bitmap, bool drawDetectedRegions, StringBuilder messageBuilder)
        {
            InitializeOCR(messageBuilder);

            bitmap = ScaleImage(bitmap, 2000, 2000);
            string vin = GetVinFromBitmap(bitmap, drawDetectedRegions, messageBuilder);

            return new GetVinResults(vin, bitmap);
        }

        private static void InitializeOCR(StringBuilder messageBuilder)
        {
            //    if (_ocr == null)
            //    {
            //        try
            //        {
            //            _ocr = new Tesseract();
            //            _ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-");
            //            //_ocr.Init(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"), "eng", false);
            //            //_ocr.Init(@"C:\tessdata", "eng", false);  !this works on production
            //            _ocr.Init(@"C:\inetpub\wwwroot\ProEstimator\bin\tessdata", "eng", false);

            //        }
            //        catch(Exception ex)
            //        {
            //            messageBuilder.AppendLine("  InitializeOCR Error: " + ex.Message);
            //        }
            //    }
        }

        private static string GetVinFromBitmap(Bitmap bitmap, bool drawOnBitmap, StringBuilder messageBuilder)
        {
            messageBuilder.AppendLine("GetVinFromBitmap start");

            Graphics graphics = null;

            Pen pen = new Pen(new SolidBrush(Color.Red));
            pen.Width = 3;

            Font font = new Font("Arial", 30);
            Brush textBrushRed = new SolidBrush(Color.Red);
            Brush textBrushWhite = new SolidBrush(Color.White);

            if (drawOnBitmap)
            {
                graphics = Graphics.FromImage(bitmap);
            }

            //string validVin = "";

            //List<Word> result = _ocr.DoOCR(bitmap, Rectangle.Empty);
            //messageBuilder.AppendLine("Got " + result.Count.ToString() + " results.");
            //foreach (Word word in result)
            //{
            //    if (drawOnBitmap)
            //    {
            //        graphics.DrawRectangle(pen, new Rectangle(word.Left, word.Top, word.Right - word.Left, word.Bottom - word.Top));
            //        string cleanedString = CleanString(word.Text);
            //        graphics.DrawString(cleanedString, font, textBrushWhite, new PointF(word.Left + 1, word.Top + 1));
            //        graphics.DrawString(cleanedString, font, textBrushRed, new PointF(word.Left, word.Top));
            //    }

            //    string vin = GetVin(word.Text);
            //    if (!string.IsNullOrEmpty(vin))
            //    {
            //        validVin = vin;
            //    }
            //}

            messageBuilder.AppendLine("GetVinFromBitmap returning");

            //return validVin;
            return "";
        }

        private static string CleanString(string input)
        {
            input = input.ToUpper();
            input = input.Replace("1-1", "H");
            input = input.Replace("-", "");

            // Vins do not include I, Q, or O
            input = input.Replace("I", "1").Replace("O", "0").Replace("Q", "0");
            return input;
        }

        private static string GetVin(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = CleanString(input);

                if (input.Length >= 17)
                {
                    // The last 6 characters should be numbers.  Replace number looking letters with their numbers
                    if (input.Length > 6)
                    {
                        string beginning = input.Substring(0, input.Length - 6);
                        string end = input.Substring(input.Length - 6, 6);
                        end = end.Replace("A", "4").Replace("B", "8").Replace("C", "0").Replace("D", "0").Replace("G", "6").Replace("H", "4").Replace("J", "1").Replace("T", "1").Replace("Z", "2");
                        input = beginning + end;
                    }

                    string vin = SearchForVin(input);
                    if (string.IsNullOrEmpty(vin))
                    {
                        // In tests, an H sometimes got read as two 1s, try replacing them and look again
                        vin = SearchForVin(input.Replace("11", "H"));
                    }
                    return vin;
                }
            }

            return "";
        }

        /// <summary>
        /// Take a string.  If it is at least 17 characters long, check each 17 character chunk for a valid VIN.  Return the first one found.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string SearchForVin(string input)
        {
            // An input of all 1s is recognized as a vin but isn't, make sure it's not counted
            string onesRemoved = input.Replace("1", "");
            int onesCount = input.Length - onesRemoved.Length;
            if (onesCount > 10)
            {
                return "";
            }

            // If the number is at least 17 characters, search it for a valid vin.  Go through the string and check each 17 character chunk in case OCR picked up extra characters
            if (input.Length >= 17)
            {
                for (int startIndex = 0; startIndex <= input.Length - 17; startIndex++)
                {
                    string piece = input.Substring(startIndex, 17);
                    if (IsVin(piece))
                    {
                        return piece;
                    }
                }
            }

            return "";
        }

        private static bool IsVin(string vin)
        {
            if (vin.Length != 17) return false;
            return getCheckDigit(vin) == vin[8];
        }

        // VIN check taken from wikipedia
        // https://en.wikipedia.org/wiki/Vehicle_identification_number

        private static int transliterate(char c)
        {
            return "0123456789.ABCDEFGH..JKLMN.P.R..STUVWXYZ".IndexOf(c) % 10;
        }

        private static char getCheckDigit(String vin)
        {
            String map = "0123456789X";
            String weights = "8765432X098765432";
            int sum = 0;
            for (int i = 0; i < 17; ++i)
            {
                sum += transliterate(vin[i]) * map.IndexOf(weights[i]);
            }
            return map[sum % 11];
        }

        private static Bitmap ScaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
    }


}
