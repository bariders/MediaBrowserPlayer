using MediaBrowserPlayer.Classes;
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

namespace MediaBrowserPlayer
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
