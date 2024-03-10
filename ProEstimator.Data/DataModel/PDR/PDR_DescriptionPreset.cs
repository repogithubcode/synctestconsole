using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_DescriptionPreset : ProEstEntity
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public string Text { get; set; }

        public static PDR_DescriptionPreset GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_DescriptionPreset_Get", new SqlParameter("ID", id));
            if (result.Success)
            {
                PDR_DescriptionPreset descriptionPreset = new PDR_DescriptionPreset();
                descriptionPreset.LoadData(result.DataTable.Rows[0]);
                return descriptionPreset;
            }

            return null;
        }

        public static List<PDR_DescriptionPreset> GetAllByLoginID(int loginID)
        {
            List<PDR_DescriptionPreset> result = new List<PDR_DescriptionPreset>();

            DBAccess db = new DBAccess();
            DBAccessTableResult resultTable = db.ExecuteWithTable("PDR_DescriptionPreset_Get", new SqlParameter("LoginID", loginID));
            if (resultTable.Success)
            {
                foreach(DataRow row in resultTable.DataTable.Rows)
                {
                    PDR_DescriptionPreset descriptionPreset = new PDR_DescriptionPreset();
                    descriptionPreset.LoadData(row);
                    result.Add(descriptionPreset);
                }
            }

            return result;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("Text", Text));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("PDR_DescriptionPreset_AddOrUpdate", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDRDescriptionPreset", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PDR_DescriptionPreset_Delete", new SqlParameter("ID", ID));
        }

        public static void DeleteForLoginID(int loginID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PDR_DescriptionPreset_Delete", new SqlParameter("LoginID", loginID));
        }

        public void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Text = row["Text"].ToString();

            RowAsLoaded = row;
        }

    }
}
