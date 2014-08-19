using MediaBrowserPlayer.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Newtonsoft.Json.Linq;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace MediaBrowserPlayer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static WebSocketManager socketManager = new WebSocketManager();
        private static List<Notification> notifications = new List<Notification>();
        private static bool notificationRunning = false;
        private static DispatcherTimer notificationTimer = new DispatcherTimer();
        private static AppSettings settings = new AppSettings();
        private static DiscoverServer discoverer = new DiscoverServer();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            this.Resuming += OnResume;

            //notifications.Add(new Notification() { Title = "Test Title", Message = "This is a test" });

            notificationTimer.Tick += NotificationTimer_Tick;
            notificationTimer.Interval = TimeSpan.FromSeconds(3);
            notificationTimer.Start();
        }

        public static void AddNotification(Notification notification)
        {
            notifications.Add(notification);
        }

        private async void NotificationTimer_Tick(object sender, object e)
        {
            lock (notifications)
            {
                if (notificationRunning == true)
                {
                    return;
                }
                else
                {
                    notificationRunning = true;
                }
            }

            while (notifications.Count > 0)
            {
                Notification notification = notifications[0];
                notifications.RemoveAt(0);

                MessageDialog msg = new MessageDialog(notification.Message, notification.Title);
                await msg.ShowAsync();
            }

            lock (notifications)
            {
                notificationRunning = false;
            }
        }


        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Server Settings", "Server Settings", (handler) => ShowCustomSettingFlyoutMain()));

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Streaming Settings", "Streaming Settings", (handler) => ShowCustomSettingFlyoutStreaming()));
        }

        public void ShowCustomSettingFlyoutMain()
        {
            SettingsFlyoutMain settingFlyout = new SettingsFlyoutMain();
            settingFlyout.Show();
        }

        public void ShowCustomSettingFlyoutStreaming()
        {
            SettingsFlyoutStreaming settingFlyout = new SettingsFlyoutStreaming();
            settingFlyout.Show();
        }

        public async static void ReInitializeWebSocket()
        {
            socketManager.CloseWebSocket();
            await socketManager.SetupWebSocket();
            ApiClient apiClient = new ApiClient();
            try
            {
                await apiClient.SetCapabilities();
            }
            catch (Exception exp)
            {
                App.AddNotification(new Notification() { Title = "Set Capabilities Error", Message = exp.Message });
            }
        }

        private async void OnResume(object sender, object e)
        {
            socketManager.CloseWebSocket();
            await socketManager.SetupWebSocket();
            ApiClient apiClient = new ApiClient();
            try
            {
                await apiClient.SetCapabilities();
            }
            catch (Exception exp)
            {
                App.AddNotification(new Notification() { Title = "Set Capabilities Error", Message = exp.Message });
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // if there is no server/port then try to discover it
            string server = settings.GetAppSettingString("server_host");
            string port = settings.GetAppSettingString("server_port");
            if (server == "" || port == "")
            {
                try
                {
                    AutoResetEvent autoEvent = new AutoResetEvent(false);
                    discoverer.DiscoverNow(autoEvent);
                    autoEvent.WaitOne(TimeSpan.FromSeconds(5));

                    if (discoverer.discoverResponce != null)
                    {

                        JObject server_info = JObject.Parse(discoverer.discoverResponce);
                        string discovered_server_host = (string)server_info["Address"];
                        if (discovered_server_host != null)
                        {
                            Uri serverUri = new Uri(discovered_server_host);
                            string server_host = serverUri.Host;
                            string server_port = serverUri.Port.ToString();

                            settings.SaveAppSettingString("server_host", server_host);
                            settings.SaveAppSettingString("server_port", server_port);
                        }

                    }
                }
                catch (Exception exep)
                {
                    App.AddNotification(new Notification() { Title = "Error Processing Auto Discover User Data", Message = exep.Message });
                }
            }

            // if user name is blank then try to get the first visible none password protected user
            string user_name = settings.GetUserName();
            if (user_name == "")
            {
                try
                {
                    ApiClient client = new ApiClient();
                    string firstUser = await client.GetFirstUsableUser();

                    if(firstUser != null)
                    {
                        settings.SaveAppSettingString("user_name", firstUser);
                    }
                }
                catch (Exception exep)
                {
                    App.AddNotification(new Notification() { Title = "Error Processing Auto Discover User Data", Message = exep.Message });
                }
            }


            // set up the main page
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

            // set up WebSocket
            await socketManager.SetupWebSocket();

            ApiClient apiClient = new ApiClient();

            try
            {
                await apiClient.Authenticate();
            }
            catch(Exception exp)
            {
                App.AddNotification(new Notification() { Title = "Authentication Error", Message = exp.Message});
            }

            try
            {
                await apiClient.SetCapabilities();
            }
            catch (Exception exp)
            {
                App.AddNotification(new Notification() { Title = "Set Capabilities Error", Message = exp.Message });
            }

        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
