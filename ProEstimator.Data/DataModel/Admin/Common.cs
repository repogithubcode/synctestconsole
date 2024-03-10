using System;

namespace ProEstimatorData.DataModel.Admin
{
    public class Common
    {
        public static dynamic GetParameterValue(object val)
        {
            if (val != null)
                return val;
            else
                return DBNull.Value;
        }
    }
}