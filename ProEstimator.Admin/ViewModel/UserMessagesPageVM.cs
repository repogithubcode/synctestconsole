using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel
{
    public class UserMessagesPageVM
    {

        public string TodaysDate { get; set; }

        public UserMessagesPageVM()
        {
            TodaysDate = DateTime.Now.ToShortDateString();
        }

    }

    public class UserMessageVM
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CreatedStamp { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public UserMessageVM(UserMessage message)
        {
            ID = message.ID;
            Title = message.Title;
            Message = message.Message;
            StartDate = message.StartDate.ToShortDateString();
            EndDate = message.EndDate.ToShortDateString();
            CreatedStamp = message.CreatedStamp.ToShortDateString();
            IsPermanent = message.IsPermanent;
            IsActive = message.IsActive;
            IsDeleted = message.IsDeleted;
        }
    }

}
