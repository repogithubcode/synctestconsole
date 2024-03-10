using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Business.Model.Admin;

using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.Contracts;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.Controllers
{
    public class SuccessBoxController : AdminController
    {

        [Route("SuccessBox/List")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/SuccessBox/List/";
                return Redirect("/LogOut");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult DoManualSync(int loginID)
        {
            if (AdminIsValidated())
            {
                FunctionResult result = SuccessBox.CheckSuccessBoxByLogin(loginID);

                if (result.Success)
                {
                    FunctionResult functionResult = SuccessBox.Save(loginID);

                    if(functionResult.Success)
                    {
                        return Json(new { Success = true, Message = "User is added successfully in Success Box." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Success = false, Message = result.ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Success = false, Message = result.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Success = false, Message = "User is not validated." }, JsonRequestBehavior.AllowGet);
            }
        }

    }

}