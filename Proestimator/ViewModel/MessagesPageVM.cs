using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class MessagesPageVM
    {

        public List<MessageVM> Messages { get; set; }

        public MessagesPageVM()
        {
            Messages = new List<MessageVM>();
        }

    }

    public class MessageVM
    {

        public int ID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool HasBeenSeen { get; set; }
        public string Date { get; set; }

        public MessageVM(UserMessage message, int loginID)
        {
            ID = message.ID;
            Title = message.Title;
            Message = message.Message.Replace(Environment.NewLine, "<br />").Replace("\n", "<br />").Replace("@LoginID", loginID.ToString());
            HasBeenSeen = message.SeenByUser;
            Date = message.CreatedStamp.ToShortDateString();
        }
    }
}