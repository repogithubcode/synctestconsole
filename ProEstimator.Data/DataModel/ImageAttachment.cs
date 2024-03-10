using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ImageAttachment : ProEstEntity
    {
        public int ID { get; set; }
        public int ReportID { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DeleteStamp { get; set; }

        public EstimationImage EstimationImage { get; set; }

        public bool ImagesOnlyChecked { get; set; }

        public ImageAttachment()
        {
            DateCreated = DateTime.Now;
            EstimationImage = new EstimationImage();
        }

        public ImageAttachment(int reportID)
        {
            ReportID = reportID;
        }

        public ImageAttachment(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ReportID = InputHelper.GetInteger(row["ReportID"].ToString());
            DateCreated = InputHelper.GetDateTime(row["DateCreated"].ToString());
            DeleteStamp = InputHelper.GetNullableDateTime(row["DeleteStamp"].ToString());
            EstimationImage = EstimationImage.GetEstimationImage(InputHelper.GetInteger(row["EstimationImageID"].ToString()));
            ImagesOnlyChecked = InputHelper.GetBoolean(row["ImagesOnlyChecked"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("ReportID", ReportID));
            parameters.Add(new SqlParameter("EstimationImageID", this.EstimationImage.ID));
            parameters.Add(new SqlParameter("DateCreated", DateCreated));
            parameters.Add(new SqlParameter("DeleteStamp", DeleteStamp));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ImageAttachment_AddOrUpdate", parameters);
            if (result.Success)
            {
                ID = result.Value;

                // These are not currently saved after the first time, no need to log
                //ChangeLogManager.LogChange(activeLoginID, "ImageAttachment", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public SaveResult DeleteForReport()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ReportID", ReportID));
            parameters.Add(new SqlParameter("DeleteStamp", DateTime.Now));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ImageAttachment_DeleteForReport", parameters);

            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        }

        public static ImageAttachment Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ImageAttachment_Get", new SqlParameter("ID", id));

            if (result.Success)
            {
                return new ImageAttachment(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<ImageAttachment> GetForReport(int reportID)
        {
            List<ImageAttachment> imageAttachments = new List<ImageAttachment>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ImageAttachment_GetByReport", new SqlParameter("ReportID", reportID));

            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    imageAttachments.Add(new ImageAttachment(row));
                }
            }

            return imageAttachments;
        }

    }
}
