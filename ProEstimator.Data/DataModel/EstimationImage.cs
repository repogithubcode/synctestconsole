using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class EstimationImage : ProEstEntity
    {

        public int ID { get; set; }
	    public int AdminInfoID{ get; set; }
        public string FileName { get; set; }
	    public string FileType { get; set; }
	    public int ImageTag { get; set; }
        public string ImageTagCustom { get; set; }
        public int PDFPageCount { get; set; }
        public int SupplementVersion { get; set; }
        public bool Include { get; set; }
        public bool Deleted { get; set; }
        public int SelectedPageIndex { get; set; }
        public int OrderNumber { get; set; }

        public EstimationImage()
        {
            ID = 0;
	        AdminInfoID = 0;
            FileName = "";
	        FileType = "";
	        ImageTag = 0;
            ImageTagCustom = "";
            PDFPageCount = 0;
            SupplementVersion = 0;
            SelectedPageIndex = 0;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", ID));
            parameters.Add(new SqlParameter("AdminInfoID", AdminInfoID));
            parameters.Add(new SqlParameter("FileName", GetString(FileName)));
            parameters.Add(new SqlParameter("FileType", GetString(FileType)));
            parameters.Add(new SqlParameter("ImageTag", GetString(ImageTag)));
            parameters.Add(new SqlParameter("ImageTagCustom", GetString(ImageTagCustom)));
            parameters.Add(new SqlParameter("PDFPageCount", PDFPageCount));
            parameters.Add(new SqlParameter("SupplementVersion", SupplementVersion));
            parameters.Add(new SqlParameter("Include", Include));
            parameters.Add(new SqlParameter("Deleted", Deleted));
            parameters.Add(new SqlParameter("OrderNo", OrderNumber));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateEstimateImage", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Image", ID, ChangeLogManager.GetLoginIDFromEstimate(AdminInfoID), parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public void Delete()
        {
            Deleted = true;
            Save();
            //DBAccess db = new DBAccess();
            //db.ExecuteNonQuery("sp_DeleteImages", new SqlParameter("ID", ID));
        }

        /// <summary>
        /// Get an Estimation Image record by database ID
        /// </summary>
        public static EstimationImage GetEstimationImage(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetEstimationImageByID", new SqlParameter("id", id));

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                EstimationImage estimationImage = new EstimationImage();
                estimationImage.LoadData(result.DataTable.Rows[0]);
                return estimationImage;
            }

            return null;
        }

        /// <summary>
        /// Get all Estimation Images for an estimate
        /// </summary>
        public static List<EstimationImage> GetForEstimate(int estimateID, bool includeDeleted = false)
        {
            List<EstimationImage> returnList = new List<EstimationImage>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("adminInfoID", estimateID));
            parameters.Add(new SqlParameter("includeDeleted", includeDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetEstimationImagesByEstimate", parameters);

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    EstimationImage estimationImage = new EstimationImage();
                    estimationImage.LoadData(row);
                    returnList.Add(estimationImage);
                }
            }

            return returnList;
        }

        private void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            FileName = row["FileName"].ToString();
            FileType = InputHelper.GetString(row["FileType"].ToString()).ToLower();
            ImageTag = InputHelper.GetInteger(row["ImageTag"].ToString());
            ImageTagCustom = row["ImageTagCustom"].ToString();
            PDFPageCount = InputHelper.GetInteger(row["PDFPageCount"].ToString());
            SupplementVersion = InputHelper.GetInteger(row["SupplementVersion"].ToString());
            Include = InputHelper.GetBoolean(row["Include"].ToString());
            Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            OrderNumber = InputHelper.GetInteger(row["OrderNo"].ToString());

            RowAsLoaded = row;
        }

        public string GetWebPath(int loginID, bool thumb)
        {
            string imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["BaseURL"], "UserContent", loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + "." + (thumb && FileType == "pdf" ? "jpg" : FileType)).Replace(@"\", "/");
            return imageFolder;
        }

        public string GetWebPath(int loginID, bool thumb, int pageIndex = 1)
        {
            string imageFolder = string.Empty;

            if (pageIndex > 1)
            {
                imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["BaseURL"], "UserContent", loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb_" + pageIndex : "") + "." + (thumb && FileType == "pdf" ? "jpg" : FileType)).Replace(@"\", "/");
            }
            else
            {
                imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["BaseURL"], "UserContent", loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + "." + (thumb && FileType == "pdf" ? "jpg" : FileType)).Replace(@"\", "/");
            }

            return imageFolder;
        }

        public string GetDiskPath(int loginID, bool thumb)
        {
            string imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + "." + FileType);
            return imageFolder; 
        }

        public string GetEditDiskPath(int loginID, bool thumb, bool edit)
        {
            //string imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + (!thumb && edit ? "_edit" : "") + "." + FileType);
            string imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + (edit ? "_edit" : "") + "." + FileType);
            return imageFolder;
        }

        public ImageExtraInfo GetImageExtraInfo(int loginID, bool boolEdit)
        {
            string filePath;
            ImageExtraInfo imageExtra = new ImageExtraInfo();
            if (FileType.ToLower() == "pdf")
            {
                filePath = GetPDFImageEditDiskPath(loginID, false, SelectedPageIndex, boolEdit);
            }
            else
            {
                filePath = GetEditDiskPath(loginID, false, boolEdit);
            }

            if (System.IO.File.Exists(filePath))
            {
                var length = new System.IO.FileInfo(filePath).Length;
                Decimal size = Decimal.Divide(length, 1024);  //Decimal size = Decimal.Divide(Bytes, 1048576);
                imageExtra.DiskSize = String.Format("{0:##.##} KB", size);

                using (System.IO.Stream stream = System.IO.File.OpenRead(filePath))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream, false, false);
                    imageExtra.Width = image.Width;
                    imageExtra.Height = image.Height;
                }
            }
            
            return imageExtra;
        }
       
        public string GetDiskPathForImageFileName(int loginID, string fileName, string fileType)
        {
            string imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", fileName + "." + "JPG");
            return imageFolder;
        }

        public string GetPDFImageDiskPath(int loginID, bool thumb, int pageIndex)
        {
            string imageFolder = string.Empty;

            if (pageIndex > 1)
            {
                imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb_" + pageIndex : "") + "." + "JPG");
            }
            else
            {
                imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + "." + "JPG");
            }

            return imageFolder;
        }

        public string GetPDFImageEditDiskPath(int loginID, bool thumb, int pageIndex, bool bEdit)
        {
            string imageFolder = string.Empty;

            if (pageIndex > 1)
            {
                //imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb_" : "_") + pageIndex + (!thumb && bEdit ? "_edit" : "") + ".JPG");
                imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb_" : "_") + pageIndex + (bEdit ? "_edit" : "") + ".JPG");
            }
            else
            {
                //imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + (!thumb && bEdit ? "_edit" : "") + ".JPG");
                imageFolder = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "") + (bEdit ? "_edit" : "") + ".JPG");
            }

            return imageFolder;
        }

        public string GetEditedDiskPath(int loginID, bool thumb)
        {
            return System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "_") + "edit." + FileType);
        }

        public string GetPDFImageEditedDiskPath(int loginID, bool thumb, int pageIndex)
        {
            if (pageIndex > 1)
            {
                return System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + "_" + pageIndex + (thumb ? "_thumb" : "_") + "edit.jpg");
            }
            else
            {
                return System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings["UserContentPath"], loginID.ToString(), AdminInfoID.ToString(), "Images", ID + (thumb ? "_thumb" : "_") + "edit.jpg");
            }
        }

        public string GetPDFImageFileName(bool thumb, int pageIndex)
        {
            string fileName = null;

            if (pageIndex > 1)
            {
                fileName = ID + (thumb ? "_thumb_" + pageIndex : "") + "." + "JPG";
            }
            else
            {
                fileName = ID + (thumb ? "_thumb" : "") + "." + "JPG";
            }

            return fileName;
        }

    }

    public class ImageExtraInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        //public float HResolution { get; set; }
        //public float VResolution { get; set; }
        public string DiskSize { get; set; }
    }

}
