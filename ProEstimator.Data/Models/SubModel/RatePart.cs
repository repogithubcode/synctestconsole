using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Profiles;

namespace ProEstimatorData.Models.SubModel
{
    public class RatePart
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public bool Cap { get; set; }
        public double Amount { get; set; }
        public bool Taxable { get; set; }
        public double Discount { get; set; }

        public RatePart() { }

        public RatePart(Rate rate)
        {
            Name = rate.RateType.RateName.Replace(" Parts", "");

            if (Name == "OEM")
            {
                Name = "Retail";
            }
            else if (Name == "Remanufactured")
            {
                Name = "Reman";
            }

            ID = rate.ID;
            Cap = rate.CapType != CapType.NoCap;
            Amount = rate.Cap;
            Taxable = Convert.ToBoolean(rate.Taxable);
            Discount = rate.DiscountMarkup;
        }

        public void FillRate(Rate rate)
        {
            rate.Cap = Amount;
            rate.CapType = Cap ? CapType.Dollars : CapType.NoCap;
            rate.DiscountMarkup = Discount;
            rate.Taxable = Taxable;
        }

        public void FillRate(Rate rate, Boolean rateCapSelectionCancelled)
        {
            rate.Cap = Amount;

            if (rateCapSelectionCancelled && rate.CapType == CapType.NoCap && this.Cap == true)
            {
                rate.CapType = CapType.NoCap;
            }
            else
            {
                rate.CapType = Cap ? CapType.Dollars : CapType.NoCap;
            }
            
            rate.DiscountMarkup = Discount;
            rate.Taxable = Taxable;
        }

    }
}