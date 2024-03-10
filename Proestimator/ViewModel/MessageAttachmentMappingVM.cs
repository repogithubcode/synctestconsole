using ProEstimatorData.DataModel;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proestimator.ViewModel
{
    public class MessageAttachmentMappingVM
    {
        public int ReportID { get; set; }
        public string ReportType { get; set; }
        public string FileName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ImageWebPath { get; set; }
        public int EstimationImageID { get; set; }
        public int ImagePageIndexNo { get; set; }
        public bool ImagesOnlyChecked { get; set; }
        public int NumPages { get; set; }
        public string FileExtension { get; set; }
        public double FileSize { get; set; }

        public MessageAttachmentMappingVM()
        {

        }

        public MessageAttachmentMappingVM(MessageAttachmentMapping messageAttachmentMapping)
        {
            ReportID = messageAttachmentMapping.ReportID;
            ReportType = messageAttachmentMapping.ReportType;
            FileName = messageAttachmentMapping.FileName;
            Subject = messageAttachmentMapping.Subject;
            Body = messageAttachmentMapping.Body;
            ImagesOnlyChecked = messageAttachmentMapping.ImagesOnlyChecked;
            NumPages = NumberOfPages();

            Report report = Report.Get(ReportID);
            string diskPath = report.GetDiskPath();

            FileInfo fileInfo = new FileInfo(diskPath);
            if (fileInfo.Exists)
            {
                FileExtension = fileInfo.Extension;
                FileSize = fileInfo.Length;
            }

            using (var r = new System.IO.StreamReader(diskPath))
            {
                string pdfText = r.ReadToEnd();
                System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(@"/Type\s*/Page[^s]");
                System.Text.RegularExpressions.MatchCollection matches = rx.Matches(pdfText);
                NumPages = matches.Count == 0 ? 1 : matches.Count;
            }
        }

        private int NumberOfPages()
        {
            Report report = Report.Get(ReportID);
            string diskPath = report.GetDiskPath();
            using (var r = new System.IO.StreamReader(diskPath))
            {
                string pdfText = r.ReadToEnd();
                System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(@"/Type\s*/Page[^s]");
                System.Text.RegularExpressions.MatchCollection matches = rx.Matches(pdfText);
                return matches.Count == 0 ? 1 : matches.Count;
            }
        }
    }
}