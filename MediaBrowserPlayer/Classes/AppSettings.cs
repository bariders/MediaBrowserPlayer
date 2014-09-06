﻿/*
Smart Player for Media Browser
Copyright (C) 2014  Blue Bit Solutions

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

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

        public string GetUserId()
        {
            string value = GetAppSettingString("user_id");
            return value.Trim();
        }

        public string GetAccessToken()
        {
            string value = GetAppSettingString("user_access_token");
            return value.Trim();
        }

        public void SaveAccessToken(string value)
        {
            SaveAppSettingString("user_access_token", value);
        }

        public void SaveUserId(string value)
        {
            SaveAppSettingString("user_id", value);
        }

        public void SaveAppSettingBool(string name, bool value)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[name] = value;
        }

        public bool GetAppSettingBool(string name)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values[name] != null)
            {
                bool tempData = (bool)localSettings.Values[name];
                return tempData;
            }
            else
            {
                return false;
            }
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

        public string GetServerHost()
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
