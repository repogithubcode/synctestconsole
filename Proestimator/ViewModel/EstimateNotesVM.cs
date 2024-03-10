using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using System.Globalization;

namespace Proestimator.ViewModel
{
    public class EstimateNotesVM
    {
        public int ID { get; set; }

        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public string NotesText { get; set; }
        public string NotesText40Chars { get; set; }
        public DateTime? TimeStamp { get; set; }

        public String TimeStampStr { get; set; }
        public string SaveMessage { get; set; }

        public Boolean IsDeleted { get; set; }
        public string DeleteRestoreImgName { get; set; }

        public EstimateNotesVM()
        {

        }

        public EstimateNotesVM(EstimateNotes estimateNote)
        {
            ID = estimateNote.ID;
            LoginID = estimateNote.LoginID;
            EstimateID = estimateNote.EstimateID;
            NotesText = estimateNote.NotesText;
            NotesText40Chars = StripHTML(NotesText);

            if(NotesText40Chars.Length > 60)
            {
                NotesText40Chars = NotesText40Chars.Substring(0, 60);
            }
            
            TimeStamp = estimateNote.TimeStamp;
            IsDeleted = estimateNote.IsDeleted;

            if (IsDeleted)
            {
                DeleteRestoreImgName = "restore.gif";
            }
            else
            {
                DeleteRestoreImgName = "delete.gif";
            }

            DateTime dateTime = estimateNote.TimeStamp.GetValueOrDefault();
            TimeStampStr = dateTime.ToString("MM/dd/yyyy hh:mm:ss tt");
        }

        private string StripHTML(string htmlString)
        {

            string pattern = @"<(.|\n)*?>";

            return System.Text.RegularExpressions.Regex.Replace(htmlString, pattern, " ");
        }
    }
}