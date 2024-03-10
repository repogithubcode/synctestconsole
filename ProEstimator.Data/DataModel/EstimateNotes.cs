using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class EstimateNotes : ProEstEntity 
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public string NotesText { get; set; }
        public DateTime? TimeStamp { get; set; }
        public Boolean IsDeleted { get; set; }

        public EstimateNotes() { }

        public static EstimateNotes GetByID(int id)
        {
            EstimateNotes estimateNote = null;

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetEstimateNotesByID", new System.Data.SqlClient.SqlParameter("ID", SqlDbType.Int) { Value = id });
            if (result.Success)
            {
                estimateNote = new EstimateNotes();
                estimateNote.LoadData(result.DataTable.Rows[0]);
            }

            return estimateNote;
        }

        /// <summary>
        /// Get all of the EstimateNotes for 
        /// </summary>
        /// <param name="estimateID"></param>
        /// <returns></returns>
        public static List<EstimateNotes> GetForEstimate(int estimateID, string showDeletedNotes)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("IsDeleted", InputHelper.GetBoolean(showDeletedNotes)));

            List<EstimateNotes> estimateNotes = new List<EstimateNotes>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetEstimateNotes", parameters);
            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    EstimateNotes estimateNote = new EstimateNotes();
                    estimateNote.LoadData(row);
                    estimateNotes.Add(estimateNote);
                }
            }

            return estimateNotes;
        }

        private void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            NotesText = InputHelper.GetString(row["NotesText"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("NotesText", GetString(NotesText)));
            parameters.Add(new SqlParameter("DeleteStamp", TimeStamp));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateEstimateNotes", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "EstimateNotes", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public void Delete(int activeLoginID = 0)
        {
            TimeStamp = DateTime.Now;
            Save(activeLoginID);
        }

        public void Restore(int activeLoginID = 0)
        {
            TimeStamp = null;
            Save(activeLoginID);
        }
    }
}
