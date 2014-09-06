using SmartPlayer.Classes;
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

namespace SmartPlayer
{
    public sealed partial class SettingsFlyoutGeneral : SettingsFlyout
    {
        private AppSettings settings = new AppSettings();

        public SettingsFlyoutGeneral()
        {
            this.InitializeComponent();
        }

        private void playerStartFullscreen_Checked(object sender, RoutedEventArgs e)
        {
            settings.SaveAppSettingBool("player_start_fullscreen", true);
        }

        private void playerStartFullscreen_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.SaveAppSettingBool("player_start_fullscreen", false);
        }

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            bool playFullscreen = settings.GetAppSettingBool("player_start_fullscreen");

            playerStartFullscreen.IsChecked = playFullscreen;
        }
    }
}
