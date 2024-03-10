using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;

namespace ProEstimator.Business.Logic
{
    public static class PartSearchManager
    {

        /// <summary>
        /// Do a text part search for the vehicle attached to the passed vehicle.
        /// </summary>
        /// <param name="searchText">Can be up to 4 words.</param>
        public static List<PartSearchResult> SearchByVehicle(int vehicleID, string searchText, string partNumber)
        {
            List<PartSearchResult> parts = new List<PartSearchResult>();

            searchText = searchText.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchText) || !string.IsNullOrEmpty(partNumber))
            {
                List<string> searchWords = searchText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                List<SqlParameter> parameters = GetParameters(vehicleID, searchWords, partNumber);

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("VehiclePartSearch", parameters);

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    parts.Add(new PartSearchResult(row, searchWords, partNumber));
                }
            }

            return parts;
        }

        /// <summary>
        /// Do a text part search for the vehicle attached to the passed vehicle and return only the sections
        /// </summary>
        /// <param name="searchText">Can be up to 4 words.</param>
        public static List<PartSearchSection> GetSectionsSearchByVehicle(int vehicleID, string searchText, string partNumber)
        {
            List<PartSearchSection> parts = new List<PartSearchSection>();

            searchText = searchText.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchText) || !string.IsNullOrEmpty(partNumber))
            {
                List<string> searchWords = searchText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                List<SqlParameter> parameters = GetParameters(vehicleID, searchWords, partNumber);

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("VehiclePartSearch_Sections", parameters);

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    parts.Add(new PartSearchSection(row));
                }
            }

            return parts;
        }

        /// <summary>
        /// Do a text part search for the vehicle attached to the passed vehicle and return the parts for a section
        /// </summary>
        /// <param name="searchText">Can be up to 4 words.</param>
        public static List<PartSearchPart> GetPartsSearchByVehicleAndSection(int vehicleID, string searchText, string partNumber, int sectionKey)
        {
            List<PartSearchPart> parts = new List<PartSearchPart>();

            searchText = searchText.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchText) || !string.IsNullOrEmpty(partNumber))
            {
                List<string> searchWords = searchText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                List<SqlParameter> parameters = GetParameters(vehicleID, searchWords, partNumber);
                parameters.Add(new SqlParameter("SectionKey", sectionKey));

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("VehiclePartSearch_ForSection", parameters);

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    parts.Add(new PartSearchPart(row, searchWords));
                }
            }

            return parts;
        }

        /// <summary>
        /// Do a text part search for the vehicle attached to the passed vehicle and return the parts for a section
        /// </summary>
        /// <param name="searchText">Can be up to 4 words.</param>
        public static List<PartSearchDetail> GetPartsSearchDetails(int vehicleID, string searchText, string partNumber, int sectionKey, string partDescription)
        {
            List<PartSearchDetail> parts = new List<PartSearchDetail>();

            searchText = searchText.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchText) || !string.IsNullOrEmpty(partNumber))
            {
                List<string> searchWords = searchText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                List<SqlParameter> parameters = GetParameters(vehicleID, searchWords, partNumber);
                parameters.Add(new SqlParameter("SectionKey", sectionKey));
                parameters.Add(new SqlParameter("PartDescription", partDescription.Replace("&amp;", "&")));

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("VehiclePartSearch_PartDetails", parameters);

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    parts.Add(new PartSearchDetail(row, searchWords));
                }
            }

            return parts;
        }

        private static List<SqlParameter> GetParameters(int vehicleID, List<string> searchWords, string partNumber)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("PartNumber", partNumber));

            // The search text can be multiple words
            for (int i = 1; i <= 4; i++)
            {
                if (searchWords.Count >= i)
                {
                    parameters.Add(new SqlParameter("Search" + i.ToString(), searchWords[i - 1]));
                }
            }

            return parameters;
        }
    }

    public class PartSearchSection
    {
        public int SectionKey { get; private set; }
        public string SectionName { get; private set; }
        public int PartCount { get; private set; }

        public PartSearchSection(DataRow row)
        {
            SectionKey = InputHelper.GetInteger(row["SectionKey"].ToString());
            SectionName = InputHelper.GetString(row["SectionName"].ToString());
            PartCount = InputHelper.GetInteger(row["Parts"].ToString());
        }
    }

    public class PartSearchPart
    {
        public string UnhilightedDescription { get; private set; }
        public string PartDescription { get; private set;}
        public int DetailCount { get; private set; }

        public PartSearchPart(DataRow row, List<string> searchWords)
        {
            UnhilightedDescription = InputHelper.GetString(row["PartDescription"].ToString());
            if (row.Table.Columns.Contains("PartDescription"))
            {
                PartDescription = InputHelper.HilightWords(InputHelper.GetString(row["PartDescription"].ToString()), searchWords);
            }
            else
            {
                PartDescription = "";
            }
            DetailCount = InputHelper.GetInteger(row["DetailCount"].ToString());
        }
    }

    public class PartSearchDetail
    {
        public int Barcode { get; private set; }
        public string PartNumber { get; private set; }
        public string PartText { get; private set; }
        public double Price { get; private set; }
        public string DateRange { get; private set; }

        public PartSearchDetail(DataRow row, List<string> searchWords)
        {
            Barcode = InputHelper.GetInteger(row["Barcode"].ToString());
            PartNumber = InputHelper.HilightWords(InputHelper.GetString(row["PartNumber"].ToString()), searchWords);
            PartText = InputHelper.HilightWords(InputHelper.GetString(row["PartText"].ToString()), searchWords);
            Price = InputHelper.GetDouble(row["Price"].ToString());
            DateRange = InputHelper.GetString(row["DateRange"].ToString());
        }
    }

    public class PartSearchResult
    {
        public int SectionKey { get; private set; }
        public string SectionName { get; private set; }
        public string PartNumber { get; private set; }
        public string PartText { get; private set; }
        public string PartDescription { get; private set; }
        public decimal Price { get; private set; }

        public PartSearchResult(DataRow row, List<string> searchWords, string partNumber)
        {
            SectionKey = InputHelper.GetInteger(row["SectionKey"].ToString());
            SectionName = HilightSearch(InputHelper.GetString(row["SectionName"].ToString()), searchWords, partNumber);
            PartNumber = HilightSearch(InputHelper.GetString(row["Part_Number"].ToString()), searchWords, partNumber);
            PartText = HilightSearch(InputHelper.GetString(row["Part_Text"].ToString()), searchWords, partNumber);
            PartDescription = HilightSearch(InputHelper.GetString(row["Prtc_Description"].ToString()), searchWords, partNumber);
            Price = InputHelper.GetDecimal(row["Price"].ToString());
        }

        private string HilightSearch(string input, List<string> searchWords, string partNumber)
        {
            input = input.ToLower();

            foreach (string word in searchWords)
            {
                input = HilightWord(input, word);
            }

            if (!string.IsNullOrEmpty(partNumber))
            {
                input = HilightWord(input, partNumber);
            }
            
            return input;
        }

        private string HilightWord(string input, string word)
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
    }
}
