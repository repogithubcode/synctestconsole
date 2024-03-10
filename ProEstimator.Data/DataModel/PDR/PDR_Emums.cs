using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel.PDR
{
    public enum PDR_Size
    {
        None = 0,
        _1in = 1,
        _2in = 2,
        _3in = 3,
        _4in = 4,
        _5in = 5,
        Dime = 6,
        Half = 7,
        Nickel = 8,
        Oversized = 9,
        Quarter = 10    
    }

    public enum PDR_Depth
    {
        None = 1,	
        NA = 2,
        Shallow = 3,
        Medium = 4,
        Deep  = 5
    }

    public enum PDR_RateProfileType
    {
        Insurance = 1,
        Dealership = 2,
        BodyShopReseller = 3,
        Retail = 4
    }

    public enum PDR_GridType
    {
        US,
        Euro,
        CANADA
    }
}
