using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using Proestimator.Helpers;
using Proestimator.Resources;
using Proestimator.ViewModel.SendEstimate;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Logic;
using Proestimator.ViewModel;
using Proestimator.ViewModel.SendEstimate.EMSExport;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.Models;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;

namespace Proestimator.Controllers
{
    public class SMSController : Controller
    {
        [HttpGet]
        [Route("SMS/{uniqueID}")]
        public ActionResult Index(string uniqueID)
        {
            ViewBag.UniqueID = uniqueID;
            return View(new SMSLandingPageVM(uniqueID));
        }

        [HttpGet]
        [Route("SMS/download-pdf/{code}/{reportID}")]
        public FileResult DownloadPDF(string code, int reportID)
        {
            ReportEmail email = ReportEmail.GetForSMSCode(code);
            if (email != null)
            {
                Estimate estimate = new Estimate(email.EstimateID);

                ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(reportID);
                if (estimate != null && report != null && report.EstimateID == estimate.EstimateID)
                {
                    string diskPath = report.GetDiskPath();
                    if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                    {
                        MiscTracking.Insert(estimate.CreatedByLoginID, estimate.EstimateID, "SMSDownloadPDF", reportID.ToString());

                        FileInfo fileInfo = new FileInfo(diskPath);

                        byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                        string fileName = fileInfo.Name;
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    }
                }
            }

            return null;
        }

        [HttpGet]
        [Route("SMS/download-image/{code}/{reportID}")]
        public FileResult DownloadImage(string code, int reportID)
        {
            ReportEmail email = ReportEmail.GetForSMSCode(code);
            if (email != null)
            {
                Estimate estimate = new Estimate(email.EstimateID);

                ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(reportID);
                if (estimate != null && report != null && report.EstimateID == estimate.EstimateID)
                {
                    string diskPath = report.GetDiskPath();
                    if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                    {
                        try
                        {
                            FileInfo fileInfo = new FileInfo(diskPath);

                            int height = 0;
                            List<System.Drawing.Image> imgList = new List<System.Drawing.Image>();

                            using (var rasterizer = new Ghostscript.NET.Rasterizer.GhostscriptRasterizer())
                            {
                                rasterizer.Open(diskPath);

                                for (int i = 1; i <= rasterizer.PageCount; i++)
                                {
                                    System.Drawing.Image img = rasterizer.GetPage(96, 96, i);
                                    height += img.Height;
                                    imgList.Add(img);
                                }

                                rasterizer.Close();
                            }

                            MemoryStream stream = new MemoryStream();
                            System.Drawing.Bitmap img2 = new System.Drawing.Bitmap(imgList[0].Width, height);
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img2);
                            height = 0;
                            foreach (System.Drawing.Image image in imgList)
                            {
                                g.DrawImage(image, new System.Drawing.Point(0, height));
                                height += image.Height;
                            }
                            g.Dispose();
                            img2.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);

                            MiscTracking.Insert(estimate.CreatedByLoginID, estimate.EstimateID, "SMSDownloadPDFImage", reportID.ToString());

                            return File(stream.ToArray(), "image/jpeg", fileInfo.Name + ".JPG");
                        }
                        catch(Exception ex)
                        {
                            ErrorLogger.LogError(ex, estimate.CreatedByLoginID, "SMSDownloadImage");
                        }
                    }
                }
            }

            return null;
        }

    }
}