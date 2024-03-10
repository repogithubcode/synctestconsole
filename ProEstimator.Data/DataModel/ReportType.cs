using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel
{
    public class ReportType
    {

        public string Tag { get; set; }
        public string Text { get; set; }
        public bool MultiLanguage { get; set; }

        public ReportType(string tag, string text, bool multiLanguage)
        {
            Tag = tag;
            Text = text;
            MultiLanguage = multiLanguage;
        }

        public static List<ReportType> GetAll()
        {
            if (_reportTypes == null)
            {
                lock(_reportTypeLock)
                {
                    _reportTypes = new List<ReportType>();

                    _reportTypes.Add(new ReportType("Estimate", "Estimate", false));
                    _reportTypes.Add(new ReportType("AuthorizationLetter", "Authorization Letter", true));
                    _reportTypes.Add(new ReportType("CustomerLetter", "Customer Letter", false));
                    _reportTypes.Add(new ReportType("DealerLetter", "Dealer Letter", false));
                    _reportTypes.Add(new ReportType("DirectionOfPaymentLetter", "Direction Of Payment Letter", true));
                    _reportTypes.Add(new ReportType("EMS", "EMS", false));
                    _reportTypes.Add(new ReportType("EstimateApproval", "Estimate Approval", true));
                    _reportTypes.Add(new ReportType("FinalReport", "Final Report", false));
                    _reportTypes.Add(new ReportType("FollowUpLetter", "Follow Up Letter", false));
                    _reportTypes.Add(new ReportType("PartsOrder", "Parts Order", false));
                    _reportTypes.Add(new ReportType("ThankYouLetter", "Thank You Letter", true));
                    _reportTypes.Add(new ReportType("WorkOrderReport", "Work Order Report", false));
                    _reportTypes.Add(new ReportType("PDRWorkOrderReport", "PDR Work Order Report", false));
                    _reportTypes.Add(new ReportType("RepairNotesReport", "Repair Notes Report", false));
                }
            }

            return _reportTypes.ToList();
        }

        public static List<ReportType> GetAll2()
        {
            if (_reportTypes2 == null)
            {
                lock (_reportTypeLock)
                {
                    _reportTypes2 = new List<ReportType>();

                    _reportTypes2.Add(new ReportType("ContractInvoices", "Contract Invoices", false));
                }
            }

            return _reportTypes2.ToList();
        }

        private static List<ReportType> _reportTypes;
        private static List<ReportType> _reportTypes2;
        private static object _reportTypeLock = new object();
    }
}
