using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ProEstimatorData;

using TuesPechkin;
using System.Drawing.Printing;

namespace ProEstimator.Business.Logic
{
    public static class HtmlToPdfSaver
    {

        private static IConverter _converter;

        public static FunctionResult SavePdf(string documentTitle, string diskPath, string html, ProEstimatorData.DataModel.Report report = null)
        {
            if (_converter == null)
            {
                _converter =
                    new ThreadSafeConverter(
                        new RemotingToolset<PdfToolset>(
                            new WinAnyCPUEmbeddedDeployment(
                                new TempFolderDeployment())));
            }

            var marginsAll = 25;
            PaperKind paperKind = PaperKind.Letter;

            try
            {
                var document = new HtmlToPdfDocument
                {
                    GlobalSettings =
                    {
                        ProduceOutline = true,
                        DocumentTitle = documentTitle,
                        PaperSize = paperKind, // Implicit conversion to PechkinPaperSize
                        Margins =
                        {
                            All = marginsAll,
                            Unit = Unit.Millimeters
                        }
                    },
                    Objects = {
                        new ObjectSettings { HtmlText = html }
                    }
                };

                if (report != null )
                {
                    document.Objects[0].FooterSettings.HtmlUrl = report.ReportFooterHtmlFilePath;
                }

                byte[] pdfBuf = _converter.Convert(document);

                // Make sure the folder exists
                string folderPath = Path.GetDirectoryName(diskPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream fs = new FileStream(diskPath, FileMode.Create))
                {
                    fs.Write(pdfBuf, 0, pdfBuf.Length);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "HtmlToPdfSaver SavePdf");
                return new FunctionResult(ex.Message);
            }

            return new FunctionResult();
        }

    }
}
