using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin
{
    public class VmRatio : IModelMap<VmRatio>
    {
        public int? X { get; set; }
        public int? Y { get; set; }

        public decimal Payload
        {
            get { 
                    if(this.X != null && this.Y != null &&
                        this.Y != 0)
                    {
                        return Math.Round(Decimal.Divide((decimal)this.X, (decimal)this.Y), 2) * 100;
                        //return (decimal) (this.X % this.Y);
                    }
                    return 0; 
            }
        }

        public VmRatio ToModel(System.Data.DataRow row)
        {
            var model = new VmRatio();
            model.X = row["pe"].SafeInt();
            model.Y = row["we"].SafeInt();

            return model;
        }
    }
}
