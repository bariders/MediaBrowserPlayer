using MediaBrowserPlayer.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaBrowserPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AppSettings appSettings = new AppSettings();

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            mainWebPage.NavigationStarting += OnNavigate;

            string server = appSettings.GetServer();

            if (server != null)
            {
                Uri mb = new Uri("http://" + server + "/mediabrowser");
                mainWebPage.Navigate(mb);
            }
            else
            {
                MessageDialog msg = new MessageDialog("Server Not Set", "Error");
                msg.ShowAsync();
            }
        }

        private void OnNavigate(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Uri destination = args.Uri;

            // block if not to media portal
            string server = appSettings.GetServerPort();
            if (destination.Host != server)
            {
                args.Cancel = true;
                MessageDialog msg = new MessageDialog("Remote sites not allowed", "Warning");
                msg.ShowAsync();
            }

        }

        public void LogMessage(string data)
        {
            notificationBox.Text = notificationBox.Text + data + "\n";
        }

    }
}
