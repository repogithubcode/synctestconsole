using ProEstimator.Business.Logic.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proestimator.Controllers
{
    public class HelpController : SiteController
    {
        private IChatBotService _chatBotService;

        public HelpController(IChatBotService chatBotService)
        {
            _chatBotService = chatBotService;
        }

        [HttpGet]
        [Route("{userID}/help")]
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetChatResponse(string question)
        {
            string result = _chatBotService.GetAnswerAsync(question);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}