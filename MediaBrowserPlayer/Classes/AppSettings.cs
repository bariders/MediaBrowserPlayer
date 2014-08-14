using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowserPlayer.Classes
{
    class AppSettings
    {
        

        public AppSettings()
        {

        }

        public string GetServerPort()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string tempData = (string)localSettings.Values["server_host"];
            return tempData;
        }

        public string GetServer()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            string value = null;
            string tempData = (string)localSettings.Values["server_host"];

            if (!string.IsNullOrEmpty(tempData))
            {
                value = tempData;
            }
            else
            {
                return null;
            }

            value += ":";

            tempData = (string)localSettings.Values["server_port"];

            if (!string.IsNullOrEmpty(tempData))
            {
                value += tempData;
            }
            else
            {
                return null;
            }

            return value;
        }

    }
}
