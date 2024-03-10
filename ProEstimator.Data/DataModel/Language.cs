using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class Language : ProEstEntity 
    {

        public int ID { get; private set; }
        public string LanguageName { get; private set; }
        public string Culture { get; private set; }

        public Language() { }

        public static List<Language> GetAll()
        {
            DBAccess dbAccess = new DBAccess();
            DBAccessTableResult result = dbAccess.ExecuteWithTable("GetAllLanguages");

            List<Language> list = new List<Language>();

            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    Language language = new Language();
                    language.LoadData(row);
                    list.Add(language);
                }
            }

            return list;
        }

        public SaveResult Save()
        {
            return new SaveResult("Languages cannot currently be saved via code.");
        }

        private void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["L_ID"].ToString());
            LanguageName = row["Language"].ToString();
            Culture = row["Culture"].ToString();
        }

    }
}
