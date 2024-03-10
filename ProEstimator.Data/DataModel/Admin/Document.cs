using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Admin
{
    public class Document : ProEstEntity
    {
        public int DocumentID { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string UploadDate { get; set; }
        public string IsDeleted { get; set; }
        public string Category { get; set; }

        public Document()
        {

        }

        public Document(int documentID, string category, string fileName, string name)
        {
            DocumentID = documentID;
            Category = category;
            FileName = fileName;
            Name = name;
        }

        public Document(DataRow row)
        {
            DocumentID = InputHelper.GetInteger(row["DocumentID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            Category = InputHelper.GetString(row["Category"].ToString());
            FileName = InputHelper.GetString(row["FileName"].ToString());
            UploadDate = InputHelper.GetString(row["UploadDate"].ToString());
            IsDeleted = InputHelper.GetString(row["IsDeleted"].ToString());

            RowAsLoaded = row;
        }

        public static List<string> GetAllCategories()
        {
            List<string> categories = new List<string>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[DocumentsManager_GetAllCategories]");

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    categories.Add(Convert.ToString(row["Category"]));
                }
            }

            return categories.OrderBy(category => category).ToList();
        }

        public static List<Document> GetAllDocuments()
        {
            List<Document> documents = new List<Document>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[DocumentsManager_GetAllDocuments]");

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    documents.Add(new Document(row));
                }
            }

            return documents;
        }

        public static Document Get(int id)
        {
            List<string> categories = new List<string>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[DocumentsManager_Get]", new SqlParameter("DocumentID", id));

            if (tableResult.Success)
            {
                return new Document(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (DocumentID > 0) // update document
            {
                parameters.Add(new SqlParameter("DocumentID", DocumentID));
            }

            parameters.Add(new SqlParameter("Name", Name));
            parameters.Add(new SqlParameter("FileName", FileName));
            parameters.Add(new SqlParameter("Category", Category));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("[dbo].[DocumentsManager_Update]", parameters);

            if (intResult.Success)
            {
                DocumentID = intResult.Value;

                // These are not currently saved after the first time, no need to log
                // ChangeLogManager.LogChange(activeLoginID, "Document", DocumentID, parameters, RowAsLoaded);

                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult);
            }
        }

        public SaveResult Delete()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("DocumentID", DocumentID));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("[dbo].[DocumentsManager_Delete]", parameters);

            if (intResult.Success)
            {
                DocumentID = intResult.Value;
                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult);
            }
        }
    }
}