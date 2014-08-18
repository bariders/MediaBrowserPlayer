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
        private AppSettings appSettings = new AppSettings();
        private bool navigationStarted = false;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            this.Loaded += MainPage_Loaded;

            mainWebPage.NavigationStarting += OnNavigate;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMainPage();
        }

        public void LoadMainPage(bool overRide = false)
        {
            if (navigationStarted == false || overRide)
            {
                navigationStarted = true;
                try
                {
                    string server = appSettings.GetServer();
                    Uri mb = new Uri("http://" + server + "/mediabrowser");
                    mainWebPage.Navigate(mb);
                }
                catch (Exception exeption)
                {
                    App.AddNotification(new Notification() { Title = "Error Loading Main Page", Message = "Server Not Correct:\n" + exeption.Message });
                }
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
                App.AddNotification(new Notification() { Title = "Navigation Error", Message = "Remote sites not allowed" });
            }

        }

        private void AppBarButton_Back(object sender, RoutedEventArgs e)
        {
            mainWebPage.GoBack();
        }

        private void AppBarButton_Refresh(object sender, RoutedEventArgs e)
        {
            mainWebPage.Refresh();
        }

        private void AppBarButton_Home(object sender, RoutedEventArgs e)
        {
            LoadMainPage(true);
        }

    }
}
