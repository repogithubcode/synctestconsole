using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class ImageTag : ProEstEntity
    {
        public int ID { get; private set; }
        public string Tag { get; set; }

        public ImageTag()
        {
            ID = 0;
            Tag = "";
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", ID));
            parameters.Add(new SqlParameter("tag", GetString(Tag)));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateEstimateImageLookup", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "ImageTag", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("DeleteeEstimateImageLookup", new SqlParameter("id", ID));
        }

        /// <summary>
        /// Get an Image Tag record by database ID
        /// </summary>
        public static ImageTag GetImageTag(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetEstimateImageLookup", new SqlParameter("id", id));

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                ImageTag tag = new ImageTag();
                tag.LoadData(result.DataTable.Rows[0]);
                return tag;
            }

            return null;
        }

        /// <summary>
        /// Get all Image Tags
        /// </summary>
        public static List<ImageTag> GetAll()
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetEstimationLookupList");

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                List<ImageTag> returnList = new List<ImageTag>();

                foreach (DataRow row in result.DataTable.Rows)
                {
                    ImageTag tag = new ImageTag();
                    tag.LoadData(row);
                    returnList.Add(tag);
                }

                return returnList;
            }

            return null;
        }

        private void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            Tag = row["Tag"].ToString();

            RowAsLoaded = row;
        }
    }
}
