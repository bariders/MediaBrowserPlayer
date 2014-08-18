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

        public string GetDeviceId()
        {
            string guid = GetAppSettingString("device_id");

            if(string.IsNullOrEmpty(guid))
            {
                Guid newGuid = Guid.NewGuid();
                guid = newGuid.ToString();
                SaveAppSettingString("device_id", guid);
            }

            return guid;
        }

        public string GetDeviceName()
        {
            string device_name = GetAppSettingString("device_name");
            return device_name.Trim();
        }

        public string GetUserName()
        {
            string value = GetAppSettingString("user_name");
            return value.Trim();
        }

        public string GetPassword()
        {
            string value = GetAppSettingString("password");
            return value.Trim();
        }

        public void SaveAppSettingInt(string name, int value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[name] = value;
        }

        public void SaveAppSettingString(string name, string value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[name] = value;
        }

        public int GetAppSettingInt(string name)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values[name] != null)
            {
                int tempData = (int)localSettings.Values[name];
                return tempData;
            }
            else
            {
                return -1;
            }
        }

        public string GetAppSettingString(string name)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string tempData = (string)localSettings.Values[name];
            if (tempData == null)
            {
                return "";
            }
            return tempData;
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
