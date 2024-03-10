using System;
using System.Collections.Generic;

using ProEstimatorData.DataModel.Profiles;

namespace ProEstimatorData.Models.SubModel
{
    public class RateLabor
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Rate { get; set; }
        public int CapType { get; set; }
        public double CapAmount { get; set; }
        public bool Taxable { get; set; }
        public double Discount { get; set; }
        public int IncludeLabor { get; set; }
        public int RateType { get; set; }

        public List<SimpleListItem> DropDownList { get; set; }

        public RateLabor() { }

        public RateLabor(Rate rate, List<SimpleListItem> dropDownList)
        {
            ID = rate.ID;
            Name = rate.RateType.RateName.Replace(" Labor", "").Replace(" Supplies", "");
            Rate = rate.RateAmount;
            CapType = (int)rate.CapType;
            CapAmount = rate.Cap;
            Taxable = Convert.ToBoolean(rate.Taxable);
            Discount = rate.DiscountMarkup;
            RateType = rate.RateType.ID;
            IncludeLabor = rate.IncludeIn == null ? 0 : rate.IncludeIn.ID;
            
            DropDownList = dropDownList;
        }

        public void FillRate(Rate rate)
        {
            rate.Cap = CapAmount;
            rate.CapType = (CapType)CapType;
            rate.DiscountMarkup = Discount;
            rate.IncludeIn = IncludeLabor == 0 ? null : ProEstimatorData.DataModel.Profiles.RateType.GetByID(IncludeLabor);
            rate.RateAmount = Rate;
            rate.Taxable = Taxable;
        }

        public void FillRate(Rate rate, Boolean rateCapSelectionCancelled)
        {
            rate.Cap = CapAmount;
            if(rateCapSelectionCancelled && rate.CapType == ProEstimatorData.DataModel.Profiles.CapType.NoCap &&
                                                     (int) rate.CapType != this.CapType)
            {
                rate.CapType = ProEstimatorData.DataModel.Profiles.CapType.NoCap;
            }
            else
            {
                rate.CapType = (CapType)CapType;
            }
            
            rate.DiscountMarkup = Discount;
            rate.IncludeIn = IncludeLabor == 0 ? null : ProEstimatorData.DataModel.Profiles.RateType.GetByID(IncludeLabor);
            rate.RateAmount = Rate;
            rate.Taxable = Taxable;
        }
    }
}