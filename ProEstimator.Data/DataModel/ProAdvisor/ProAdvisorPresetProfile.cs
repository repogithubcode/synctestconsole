using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.ProAdvisor
{
    public class ProAdvisorPresetProfile : ProEstEntity, IIDSetter
    {

        public int ID { get; private set; }
        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public int LoginID { get; set; }
        public string Name { get; set; }
        public bool DefaultFlag { get; set; }
        public DateTime CreationStamp { get; set; }
        public bool Deleted { get; set; }
	
        public ProAdvisorPresetProfile()
        {
            CreationStamp = DateTime.Now;
        }

        public List<ChangeLogRowSummary> GetHistory()
        {
            List<ChangeLogRowSummary> history = ChangeLogRowSummary.GetByItem("AddOnPresetProfile", ID);
            history.AddRange(ChangeLogRowSummary.GetByItem("AddOnPreset", ID));

            return history.OrderBy(o => o.TimeStamp).ToList();
        }
    }
}