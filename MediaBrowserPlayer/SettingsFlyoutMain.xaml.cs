/*
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

using SmartPlayer.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SmartPlayer
{
    public sealed partial class SettingsFlyoutMain : SettingsFlyout
    {
        public SettingsFlyoutMain()
        {
            this.InitializeComponent();
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            string original_server_host = (string)localSettings.Values["server_host"];
            string original_server_post = (string)localSettings.Values["server_port"];
            string original_server_name = (string)localSettings.Values["device_name"];

            // if settings change then set them and reload interface
            if (original_server_host != setting_server.Text.Trim() || 
                original_server_post != setting_port.Text.Trim() ||
                original_server_name != setting_device_name.Text.Trim())
            {

                localSettings.Values["server_host"] = setting_server.Text.Trim();
                localSettings.Values["server_port"] = setting_port.Text.Trim();
                localSettings.Values["device_name"] = setting_device_name.Text.Trim();

                // on settings update reload main page and reconnect websocket

                Frame rootFrame = Window.Current.Content as Frame;
                var p = rootFrame.Content as MainPage;
                if (p != null)
                {
                    p.LoadMainPage(true);
                }

                // ReInitialize WebSocket
                App.ReInitializeWebSocket();
            }
        }

        private string GetSetting(string name)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey(name))
            {
                return ((string)localSettings.Values[name]).Trim();
            }
            else
            {
                return "";
            }
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            setting_server.Text = GetSetting("server_host");
            setting_port.Text = GetSetting("server_port");
            setting_device_name.Text = GetSetting("device_name");
        }

        public void DataReceived(string discoverData)
        {
            try
            {
                JObject server_info = JObject.Parse(discoverData);
                string discovered_server_host = (string)server_info["Address"];
                if (discovered_server_host != null)
                {
                    Uri serverUri = new Uri(discovered_server_host);
                    string server_host = serverUri.Host;
                    string server_port = serverUri.Port.ToString();

                    Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        setting_server.Text = server_host;
                        setting_port.Text = server_port;
                    });

                }
            }
            catch (Exception exep)
            {
                App.AddNotification(new Notification() { Title = "Error Receiving Discover Data", Message = exep.Message });
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DiscoverServer discoverer = new DiscoverServer();
                discoverer.dataReceived += DataReceived;
                discoverer.DiscoverNow(null);
            }
            catch (Exception exep)
            {
                App.AddNotification(new Notification() { Title = "Error Sending Discover Data", Message = exep.Message });
            }
        }
    }
}
