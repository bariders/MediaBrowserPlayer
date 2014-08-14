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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaBrowserPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            mainWebPage.NavigationStarting += OnNavigate;

            Uri mb = new Uri("http://localhost:8096/mediabrowser");
            mainWebPage.Navigate(mb);
        }

        private void OnNavigate(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Uri destination = args.Uri;

            // block if not to media portal
            if(destination.Host != "localhost")
            {
                args.Cancel = true;
            }

        }

        public void LogMessage(string data)
        {
            notificationBox.Text = notificationBox.Text + data + "\n";
        }
    }
}
