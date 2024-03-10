using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic
{
    public static class SiteGlobalsManager
    {

        public static string HomePageMessage { get; private set; }

        public static void LoadData()
        {
            SiteGlobals globals = SiteGlobals.Get();
            HomePageMessage = globals.HomePageMessage;
        }
    }
}
