using System.Web;

namespace ProEstimatorData.Helpers
{
    public static class HttpContextCurrentItemsHelper
    {
        public static int GetItemValue(string itemKey, int defaultValue)
        {
            try
            {
                if (HttpContext.Current?.Items?[itemKey] != null)
                {
                    return int.Parse(HttpContext.Current.Items[itemKey].ToString());
                }
                else if (HttpContext.Current?.Session?[itemKey] != null)
                {
                    return int.Parse(HttpContext.Current.Session[itemKey].ToString());
                }
            }
            catch { }

            return defaultValue;
        }
    }
}