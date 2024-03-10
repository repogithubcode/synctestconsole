using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;
using System;

namespace ProEstimator.Admin.ViewModel.EmailReport
{
    public class EmailReportVM
    {
        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public string LoginName { get; private set; }
        public string ToAddresses { get; private set; }
        public string CCAddresses { get; private set; }
        public string SMSNumbers { get; private set; }
        public string ReplyTo { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
        public string CreateStamp { get; private set; }
        public string SendStamp { get; private set; }
        public string HasError { get; private set; }
        public string ErrorMessage { get; private set; }
        public string Recipient { get; private set; }

        public EmailReportVM() { }

        public EmailReportVM(Email email)
        {
            ID = InputHelper.GetInteger(email.ID.ToString());
            LoginID = InputHelper.GetInteger(email.LoginID.ToString());
            ToAddresses = InputHelper.GetString(email.ListToString(email.ToAddresses));
            CCAddresses = InputHelper.GetString(email.ListToString(email.CCAddresses));
            SMSNumbers = InputHelper.GetString(email.ListToString(email.SMSNumbers));

            ReplyTo = InputHelper.GetString(email.ReplyTo.ToString());
            Subject = InputHelper.GetString(email.Subject.ToString());
            Body = InputHelper.GetString(email.Body.ToString());

            CreateStamp = InputHelper.GetString(email.CreateStamp.ToString());
            SendStamp = InputHelper.GetString(email.SendStamp.ToString());

            HasError = InputHelper.GetString(email.HasError.ToString()); 
            ErrorMessage = InputHelper.GetString(email.ErrorMessage.ToString());
            Recipient = InputHelper.GetString(email.Recipient.ToString());
        }
    }
}