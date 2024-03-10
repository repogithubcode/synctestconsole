using ProEstimatorData;
using System.Configuration;

namespace ProEstimator.Business.Logic
{
    public class ConfigurationManagerUtilities
    {
        public static string GetAppSetting(string appSettingName, string defaultValue = "")
        {
            if (ConfigurationManager.AppSettings[appSettingName] != null)
                return ConfigurationManager.AppSettings.Get(appSettingName);
                
            ErrorLogger.LogError($"Key not found: '{appSettingName}'", "ConfigurationManagerUtilities GetAppSetting");
            return defaultValue;
        }
    }
}
