using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace ProEstimatorData.DataModel
{
    public class ProEstEntity
    {
        //public virtual SaveResult Save() { return new SaveResult(); }

        public virtual SaveResult Save(int activeLoginID = 0, int loginID = 0) { return new SaveResult(); }

        public DataRow RowAsLoaded { get; set; }

        /// <summary>
        /// If a ProEstEntity is going to be returned as json, we must clear the RowAsLoaded or it fails with a circular reference.
        /// </summary>
        public void ClearRowAsLoaded()
        {
            RowAsLoaded = null;
        }

        protected string GetString(object input)
        {
            if (input == null)
            {
                return "";
            }

            return input.ToString();
        }
    }
}