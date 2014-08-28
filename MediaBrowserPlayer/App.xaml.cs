/*
Media Browser Player
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
using System.Diagnostics.Tracing;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Search;

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
            PruneLogsFiles();

            string logFileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            EventListener informationListener = new StorageFileEventListener("AppLog-" + logFileName);
            informationListener.EnableEvents(MetroEventSource.Log, EventLevel.Informational);

            this.InitializeComponent();
            this.Suspending += OnSuspending;

            this.Resuming += OnResume;

            //notifications.Add(new Notification() { Title = "Test Title", Message = "This is a test" });

            notificationTimer.Tick += NotificationTimer_Tick;
            notificationTimer.Interval = TimeSpan.FromSeconds(3);
            notificationTimer.Start();

            MetroEventSource.Log.Info("App Started");
        }

        private void PruneLogsFiles()
        {
            var files = ApplicationData.Current.LocalFolder.GetFilesAsync();
            
            while (files.Status == AsyncStatus.Started)
                Task.Delay(10).Wait();

            IReadOnlyList<StorageFile> fileList = files.GetResults();

            IEnumerable<StorageFile> sortedFiles = fileList.OrderBy((x) => x.DateCreated);
            int numToDelete = Math.Max((fileList.Count - 5), 0);

            // delete the oldest
            foreach (StorageFile file in sortedFiles)
            {
                if (numToDelete > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Delete Log  : " + file.Name);
                    file.DeleteAsync();
                }
                else
                {
                    break;
                }
                numToDelete--;
            }
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

                MetroEventSource.Log.Info(notification.Title + " - " + notification.Message);
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
                "General Settings", "General Settings", (handler) => ShowCustomSettingFlyoutGeneral()));

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Server Settings", "Server Settings", (handler) => ShowCustomSettingFlyoutMain()));

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Streaming Settings", "Streaming Settings", (handler) => ShowCustomSettingFlyoutStreaming()));
        }

        public void ShowCustomSettingFlyoutGeneral()
        {
            SettingsFlyoutGeneral settingFlyout = new SettingsFlyoutGeneral();
            settingFlyout.Show();
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
                MetroEventSource.Log.Info("Set Capabilities Error - " + exp.Message);
                //App.AddNotification(new Notification() { Title = "Set Capabilities Error", Message = exp.Message });
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
                //App.AddNotification(new Notification() { Title = "Set Capabilities Error", Message = exp.Message });
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
                App.AddNotification(new Notification() { Title = "Notice", Message = "You need to set the server details in the settings" });
            }
            else
            {
                // set up WebSocket
                await socketManager.SetupWebSocket();
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
