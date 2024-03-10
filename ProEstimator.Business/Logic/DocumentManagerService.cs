using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace ProEstimator.Business.Logic
{
    public class UploadDocumentResult
    {
        public bool Success { get; set; }
        public int DocumentID { get; set; }
        public string ErrorMessage { get; set; }
        public string NewDocumentPath { get; set; }

        public UploadDocumentResult()
        {
            Success = false;
            DocumentID = 0;
            ErrorMessage = "";
            NewDocumentPath = "";
        }
    }
}
