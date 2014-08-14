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

            localSettings.Values["server_host"] = setting_server.Text;
            localSettings.Values["server_port"] = setting_port.Text;
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["server_host"] != null)
            {
                setting_server.Text = ((string)localSettings.Values["server_host"]).Trim();
            }
            else
            {
                setting_server.Text = "localhost";
            }

            if (localSettings.Values["server_port"] != null)
            {
                setting_port.Text = ((string)localSettings.Values["server_port"]).Trim();
            }
            else
            {
                setting_port.Text = "8096";
            }   
        }
    }
}
