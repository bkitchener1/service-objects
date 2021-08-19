using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceObjects
{
    /// <summary>
    /// WebConfig stores config values from a .env file or environment variable.B
    /// If no value has been set in the app.config uses the default setting provided here
    /// </summary>
    public class WebConfig
    {
        public static int ElementTimeoutMs = GetConfigInt("ElementTimeoutMs", 10000);
        public static bool AutoLaunchBrowser = GetConfigBool("AutoLaunchBrowser", true);
        public static bool AutoQuitBrowser = GetConfigBool("AutoQuitBrowser", true);
        public static string DefaultUrl = GetConfigString("DefaultUrl", "http://rposwebdev2/");
        public static string BrowserName = GetConfigString("BrowserName", "chrome");
        public static bool RemoteSession = GetConfigBool("RemoteSession", false);
        public static string RemoteServer = GetConfigString("RemoteServer", "http://127.0.0.1:4444");
        public static string RemoteCapabilities = GetConfigString("RemoteCapabilities", "");
        public static bool WiniumSession = GetConfigBool("WiniumSession", false);
        public static string WiniumApp = GetConfigString("WiniumApp", "");

        static WebConfig()
        {
            DotNetEnv.Env.Load();
        }

        private static string GetConfigString(string key, string defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (value != null)
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        private static bool GetConfigBool(string key, bool defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (value != null)
            {
                var result = Boolean.Parse(value.ToLower());
                return result;
            }
            else
            {
                return defaultValue;
            }
        }


        private static int GetConfigInt(string key, int defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (value != null)
            {
                return Int32.Parse(value);
            }
            else
            {
                return defaultValue;
            }  
        }
    }
}
