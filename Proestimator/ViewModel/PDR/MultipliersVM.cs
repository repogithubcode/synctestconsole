using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel.PDR
{
    public class MultipliersVM
    {
        public int LoginID { get; set; }

        public bool GoodData { get; set; }
        public int ProfileID { get; set; }

        public string ErrorMessage { get; set; }

        public List<MultiplierVM> Multipliers { get; private set; }

        public MultipliersVM()
        {
            GoodData = false;
            ProfileID = 0;
            ErrorMessage = "";

            Multipliers = new List<MultiplierVM>();
        }

        public void LoadMultipliers(List<PDR_Multiplier> multipliers)
        {
            multipliers.ForEach(o => Multipliers.Add(new MultiplierVM(o)));
        }

    }

    public class MultiplierVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int Index { get; set; }

        public MultiplierVM()
        {
            ID = 0;
            Name = "";
            Value = "";
            Index = 0;
        }

        public MultiplierVM(PDR_Multiplier multiplier)
        {
            ID = multiplier.ID;
            Name = multiplier.Name;
            Value = multiplier.Value.ToString();
            Index = 0;
        }
    }
}