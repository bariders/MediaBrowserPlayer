using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            localSettings.Values["user_name"] = setting_user_name.Text.Trim();
            localSettings.Values["password"] = setting_password.Text.Trim();
            localSettings.Values["device_name"] = setting_device_name.Text.Trim();
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
            if(setting_server.Text == "")
            {
                setting_server.Text = "localhost";
            }
            setting_port.Text = GetSetting("server_port");
            if(setting_port.Text == "")
            {
                setting_port.Text = "8096";
            }

            setting_user_name.Text = GetSetting("user_name");
            setting_password.Text = GetSetting("password");
            setting_device_name.Text = GetSetting("device_name");
        }
    }
}
