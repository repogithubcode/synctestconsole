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
    public class UserMessagesController : AdminController
    {

        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/UserMessages";
                return Redirect("/LogOut");
            }
            else
            {
                return View(new UserMessagesPageVM());
            }
        }
        
        public JsonResult SaveUserMessage(int id, string title, string message, string startDate, string endDate, string createdDate, bool isPermanent, bool isActive, bool isDeleted)
        {
            UserMessage userMessage = new UserMessage();

            if (id > 0)
            {
                userMessage = UserMessage.Get(id);
            }

            userMessage.Title = title;
            userMessage.Message = message;
            userMessage.StartDate = InputHelper.GetDateTime(startDate);
            userMessage.EndDate = InputHelper.GetDateTime(endDate);
            userMessage.CreatedStamp = InputHelper.GetDateTime(createdDate);
            userMessage.IsPermanent = isPermanent;
            userMessage.IsActive = isActive;
            userMessage.IsDeleted = isDeleted;

            SaveResult saveResult = userMessage.Save(ActiveLogin.ID);
            if (saveResult.Success)
            {
                return Json(new SaveUserMessageResponse(userMessage), JsonRequestBehavior.AllowGet); 
            }
            else
            {
                return Json(new SaveUserMessageResponse(saveResult.ErrorMessage), JsonRequestBehavior.AllowGet); 
            }
        }

        public JsonResult GetUserMessages()
        {
            List<UserMessage> messages = UserMessage.GetAll().Where(o => o.IsDeleted == false).OrderByDescending(o => o.CreatedStamp).ToList();

            List<UserMessageVM> messageVMs = new List<UserMessageVM>();
            foreach(UserMessage message in messages)
            {
                messageVMs.Add(new UserMessageVM(message));
            }

            return Json(messageVMs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteMessage(int id)
        {
            UserMessage message = UserMessage.Get(id);
            if (message != null)
            {
                message.IsDeleted = true;
                message.Save(ActiveLogin.ID);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserMessage(int id)
        {
            SaveUserMessageResponse response;

            UserMessage message = UserMessage.Get(id);
            if (message != null)
            {
                response = new SaveUserMessageResponse(message);
            }
            else
            {
                response = new SaveUserMessageResponse("No message found for ID " + id);
            }

            return Json(response, JsonRequestBehavior.AllowGet); 
        }

        public class SaveUserMessageResponse : FunctionResult
        {
            public UserMessageVM UserMessage { get; set; }
            
            public SaveUserMessageResponse(string errorMessage)
                : base(errorMessage)
            {

            }

            public SaveUserMessageResponse(UserMessage savedUserMessage)
            {
                UserMessage = new UserMessageVM(savedUserMessage);
            }
        }

    }
}