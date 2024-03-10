using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;
using ProEstimatorData.DataModel;

using ProEstimator.Admin.ViewModel;

namespace ProEstimator.Admin.Controllers
{
    public class ChangeLogController : AdminController
    {
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/ChangeLog";
                return Redirect("/LogOut");
            }
            else
            {
                ChangeLogPageVM pageVM = new ChangeLogPageVM();
                return View(pageVM);
            }
        }

        public JsonResult GetDatesList()
        {
            ChangeLogDateVM currentDateVM = null;
            List<SiteChangeLog> logItems = SiteChangeLog.GetAll();
            DateTime lastDate = DateTime.MinValue;

            List<ChangeLogDateVM> dateSets = new List<ChangeLogDateVM>();

            foreach (SiteChangeLog logItem in logItems)
            {
                if (lastDate.Date != logItem.Date.Date)
                {
                    lastDate = logItem.Date.Date;

                    currentDateVM = new ChangeLogDateVM();
                    currentDateVM.Date = logItem.Date.ToShortDateString();
                    dateSets.Add(currentDateVM);
                }

                currentDateVM.Items.Add(new ChangeLogItemVM(logItem));
            }

            return Json(dateSets, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetChangeLog(int id)
        {
            SiteChangeLog changeLog = SiteChangeLog.Get(id);
            ChangeLogItemVM vm = new ChangeLogItemVM(changeLog);
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveChangeLog(int id, string date, string note, bool isActive)
        {
            SiteChangeLog log = new SiteChangeLog();

            if (id > 0)
            {
                log = SiteChangeLog.Get(id);
                if (log == null)
                {
                    return Json("Invalid log ID", JsonRequestBehavior.AllowGet);
                }
            }

            log.Date = InputHelper.GetDateTime(date).Date;
            log.ShortNote = note;
            log.IsActive = isActive;

            FunctionResult saveResult = log.Save();
            if (saveResult.Success)
            {
                return Json(new ChangeLogSaveResult(log.ID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new ChangeLogSaveResult(saveResult.ErrorMessage), JsonRequestBehavior.AllowGet);
            }
        }

        public class ChangeLogSaveResult
        {
            public bool Success { get; set; }
            public int ChangeLogID { get; set; }
            public string ErrorMessage { get; set; }

            public ChangeLogSaveResult(int id)
            {
                Success = true;
                ChangeLogID = id;
                ErrorMessage = "";
            }

            public ChangeLogSaveResult(string errorMessage)
            {
                Success = false;
                ChangeLogID = 0;
                ErrorMessage = errorMessage;
            }
        }

        public JsonResult DeleteChangeLog(int id)
        {
            SiteChangeLog log = SiteChangeLog.Get(id);
            if (log != null)
            {
                log.Delete();
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActivateChecked(string data)
        {
            SetListActive(data, true);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeactivateChecked(string data)
        {
            SetListActive(data, false);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        private void SetListActive(string data, bool isActive)
        {
            string[] pieces = data.Split(",".ToCharArray());
            foreach (string piece in pieces)
            {
                int id = InputHelper.GetInteger(piece);
                SiteChangeLog changeLog = SiteChangeLog.Get(id);
                if (changeLog != null)
                {
                    changeLog.IsActive = isActive;
                    changeLog.Save();
                }
            }
        }

        public JsonResult ClearSeenLog()
        {
            SiteChangeLog.ClearAccountSeenTable();
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}