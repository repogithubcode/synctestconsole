using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Model.Admin;

namespace ProEstimator.Admin.ViewModel
{
    public class SmsHistoryVM 
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string DateSent { get; set; }
        public string SalesRep { get; set; }
        public string SalesRepId { get; set; }
        public string LoginId { get; set; }

        public SmsHistoryVM(VmSmsHistory vmSmsHistory)
        {
            this.Id = vmSmsHistory.Id;
            this.Message = vmSmsHistory.Message;
            this.SalesRep = vmSmsHistory.SalesRep;
            this.PhoneNumber = vmSmsHistory.PhoneNumber;
            this.DateSent = vmSmsHistory.DateSent;
        }

        public VmSmsHistory ToVmSmsHistory(SmsHistoryVM smsHistoryVM)
        {
            VmSmsHistory vmSmsHistory = new VmSmsHistory();

            vmSmsHistory.Id = smsHistoryVM.Id;
            vmSmsHistory.Message = smsHistoryVM.Message;
            vmSmsHistory.SalesRep = smsHistoryVM.SalesRep;
            vmSmsHistory.PhoneNumber = smsHistoryVM.PhoneNumber;
            vmSmsHistory.DateSent = smsHistoryVM.DateSent;

            return vmSmsHistory;
        }

    }
}
