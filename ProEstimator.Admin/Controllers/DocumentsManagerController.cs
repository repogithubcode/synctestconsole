using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.ManageDocument;
using ProEstimator.Admin.ViewModel.UserFeedbackBug;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class DocumentsManagerController : AdminController
    {
        [Route("DocumentsUpload/List")]
        public ActionResult DocumentsUpload()
        {
            DocumentVM documentVM = new DocumentVM();
            documentVM.Category = "Select";

            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/ChangeLog";
                return Redirect("/LogOut");
            }
            else
            {
                return View(documentVM);
            }
        }

        [Route("DocumentsDownload/List")]
        public ActionResult DocumentsDownload()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/ChangeLog";
                return Redirect("/LogOut");
            }
            else
            {
                return View();
            }
        }

        public JsonResult GetAllDocuments()
        {
            List<Document> allDocuments = Document.GetAllDocuments();

            List<DocumentVM> listDocumentVMs = new List<DocumentVM>();

            foreach (Document document in allDocuments)
            {
                listDocumentVMs.Add(new DocumentVM(document));
            }

            return Json(listDocumentVMs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDocumentInfo(int documentID)
        {
            Document document = Document.Get(documentID);

            DocumentVM documentVM = new DocumentVM(document);

            return Json(documentVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveUploadDocument()
        {
            UploadDocumentResult uploadDocumentResult = new UploadDocumentResult();
            SaveResult saveDocumentResult = null;

            #region Add/Edit Document Details

            string fileName = InputHelper.GetString(System.Web.HttpContext.Current.Request.Form["fileName"].ToString());

            if (!string.IsNullOrEmpty(fileName))
            {
                int documentID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["documentID"].ToString());
                string category = InputHelper.GetString(System.Web.HttpContext.Current.Request.Form["category"].ToString());
                string name = InputHelper.GetString(System.Web.HttpContext.Current.Request.Form["name"].ToString());

                Document document = new Document(documentID, category, fileName, name);

                saveDocumentResult = document.Save(ActiveLogin.ID);

                uploadDocumentResult.DocumentID = documentID;
                uploadDocumentResult.Success = true;

                if (!string.IsNullOrEmpty(saveDocumentResult.ErrorMessage))
                {
                    ErrorLogger.LogError(saveDocumentResult.ErrorMessage, "DocumentUpload");
                    uploadDocumentResult.Success = false;
                    return Json(saveDocumentResult, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (saveDocumentResult.Success)
                    {
                        if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
                        {
                            var httpRequest = System.Web.HttpContext.Current.Request;

                            foreach (string eachFile in httpRequest.Files)
                            {
                                var file = System.Web.HttpContext.Current.Request.Files[eachFile];

                                uploadDocumentResult = UploadDocument(category, document.DocumentID, name, fileName, file);

                                if (uploadDocumentResult != null && (!string.IsNullOrEmpty(uploadDocumentResult.ErrorMessage)))
                                {
                                    uploadDocumentResult.Success = false;
                                    ErrorLogger.LogError(uploadDocumentResult.ErrorMessage, "DocumentUpload");
                                    return Json(uploadDocumentResult, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return Json(uploadDocumentResult, JsonRequestBehavior.AllowGet);
        }

        public UploadDocumentResult UploadDocument(string category, int documentID, string name, string fileName, System.Web.HttpPostedFile file)
        {
            UploadDocumentResult result = null;

            try
            {
                if (file != null)
                {
                    string contentType = file.ContentType.ToLower();

                    string fileNameWoExtension = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName).Replace(".", "");

                    string documentsRootFolder = Path.Combine(GetDocumentsRootDiskPath());

                    if (!Directory.Exists(documentsRootFolder))
                    {
                        Directory.CreateDirectory(documentsRootFolder);
                    }

                    // Save the file to disk
                    string fileFullName = fileNameWoExtension + "_" + documentID + "." + extension;
                    string filePath = Path.Combine(documentsRootFolder, fileFullName);
                    if(Path.GetFullPath(filePath).StartsWith(documentsRootFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        file.SaveAs(filePath);
                        result = new UploadDocumentResult(documentID);
                    }
                }
                else
                {
                    result = new UploadDocumentResult("No file attached.");
                }
            }
            catch (Exception ex)
            {
                result = new UploadDocumentResult("Error uploading image: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Files for an estimate are stored on disk in a folder like C:/UserContent/LoginID/EstimateID.  This function returns that as a path.
        /// </summary>
        public string GetDocumentsRootDiskPath()
        {
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Documents Manager", "Documents Root");
        }

        [Route("DownloadDocument/{documentID}")]
        public ActionResult DownloadDocument(int documentID)
        {
            string fileName = string.Empty;
            Document document = Document.Get(documentID);

            string fileNameWoExtension = Path.GetFileNameWithoutExtension(document.FileName);
            string extension = Path.GetExtension(document.FileName).Replace(".", "");

            string documentsRootFolder = Path.Combine(GetDocumentsRootDiskPath());
            // get file
            string fileFullName = fileNameWoExtension + "_" + documentID + "." + extension;
            string filePath = Path.Combine(documentsRootFolder, fileFullName);

            if (System.IO.File.Exists(filePath))
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                if (extension == "pdf")
                {
                    return File(fileStream, "application/pdf", document.FileName);
                }
                else if (extension == "txt")
                {
                    return File(fileStream, "text/plain", document.FileName);
                }
                else if (extension == "png")
                {
                    return File(fileStream, "image/png", document.FileName);
                }
                else if ((extension == "jpg") || (extension == "jpeg"))
                {
                    return File(fileStream, "image/jpeg", document.FileName);
                }
                else if (extension == "doc")
                {
                    return File(fileStream, "application/msword", document.FileName);
                }
                else if (extension == "docx")
                {
                    return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", document.FileName);
                }
                else if (extension == "xls")
                {
                    return File(fileStream, "application/vnd.ms-excel", document.FileName);
                }
                else if (extension == "xlsx")
                {
                    return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", document.FileName);
                }
                else if (extension == "ppt")
                {
                    return File(fileStream, "application/vnd.ms-powerpoint", document.FileName);
                }
                else if (extension == "pptx")
                {
                    return File(fileStream, "application/vnd.openxmlformats-officedocument.presentationml.presentation", document.FileName);
                }
                else
                {
                    return Content("Error: File not found.");
                }
            }

            return Content("Error: File not found.");
        }

        public JsonResult DeleteDocument(int documentID)
        {
            Document document = Document.Get(documentID);
            if (document != null)
            {
                document.Delete();
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
    }

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

        public UploadDocumentResult(int id)
        {
            Success = true;
            DocumentID = id;
            ErrorMessage = "";
            NewDocumentPath = "";
        }

        public UploadDocumentResult(string errorMessage)
        {
            Success = false;
            DocumentID = 0;
            ErrorMessage = "";
            NewDocumentPath = "";
        }
    }
}

