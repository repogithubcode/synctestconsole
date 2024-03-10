using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Admin
{
    public class DocumentCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public List<Document> Documents { get; set; }

        public DocumentCategory()
        {

        }

        public DocumentCategory(DataRow row, DataRow[] documentDataRowArr)
        {
            CategoryID = InputHelper.GetInteger(row["CategoryID"].ToString());
            CategoryName = InputHelper.GetString(row["CategoryName"].ToString());
            Description = InputHelper.GetString(row["Description"].ToString());

            Documents = new List<Document>();

            foreach (DataRow documentDataRow in documentDataRowArr)
            {
                Documents.Add(new Document(documentDataRow));
            }
        }

        public static List<DocumentCategory> GetForFilter(int? categoryID, int? documentID = null)
        {
            List<DocumentCategory> categories = new List<DocumentCategory>();
            DataRow[] documentDataRowArr = null;
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@CategoryID", Common.GetParameterValue(categoryID)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@DocumentID", Common.GetParameterValue(documentID)) { SqlDbType = SqlDbType.Int });

            DBAccessDataSetResult datasetResult = db.ExecuteWithDataSet("[dbo].[GetCategoryDocuments]", parameters);

            DataRelation categoryDocumentRelation =
                datasetResult.DataSet.Relations.Add("CategoryDocumentRelation",
                datasetResult.DataSet.Tables[0].Columns["CategoryID"],
                datasetResult.DataSet.Tables[1].Columns["CategoryID"]);

            foreach (DataRow categoryRow in datasetResult.DataSet.Tables[0].Rows)
            {
                documentDataRowArr = categoryRow.GetChildRows(categoryDocumentRelation);

                categories.Add(new DocumentCategory(categoryRow, documentDataRowArr));
            }

            return categories.OrderBy(category => category.CategoryID).ToList();
        }

        public static SaveResult Save(int categoryID, string categoryName, string categoryDescription, out int ID)
        {
            ID = 0;

            List<SqlParameter> parameters = new List<SqlParameter>();
            if (categoryID > 0) // update category
            {
                parameters.Add(new SqlParameter("CategoryID", categoryID));
            }
            
            parameters.Add(new SqlParameter("CategoryName", categoryName));
            parameters.Add(new SqlParameter("Description", categoryDescription));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("SaveCategory", parameters);

            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        }
    }
}