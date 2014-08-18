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
                "Server Settings", "Server Setting", (handler) => ShowCustomSettingFlyout()));
        }

        public void ShowCustomSettingFlyout()
        {
            SettingsFlyoutMain CustomSettingFlyout = new SettingsFlyoutMain();

            CustomSettingFlyout.Show();
        }

        public async static void ReInitializeWebSocket()
        {
            socketManager.CloseWebSocket();
            await socketManager.SetupWebSocket();
            ApiClient apiClient = new ApiClient();
            await apiClient.SetCapabilities();
        }

        private async void OnResume(object sender, object e)
        {
            socketManager.CloseWebSocket();
            await socketManager.SetupWebSocket();
            ApiClient apiClient = new ApiClient();
            await apiClient.SetCapabilities();
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

            ApiClient apiClient = new ApiClient();

            try
            {
                await apiClient.Authenticate();
            }
            catch(Exception exp)
            {
                App.AddNotification(new Notification() { Title = "Authentication Error", Message = exp.Message});
            }

            // set up WebSocket
            await socketManager.SetupWebSocket();
            await apiClient.SetCapabilities();

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
