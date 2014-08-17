using MediaBrowserPlayer.Classes;
using Newtonsoft.Json.Linq;
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
    public sealed partial class PlayerPage : Page
    {
        private DispatcherTimer _timer;
        private long _seekTo = 0;
        private string itemId;
        private bool _sliderpressed = false;
        private AppSettings settings = new AppSettings();
        private ApiClient client = new ApiClient();

        private DispatcherTimer _playbackCheckin;

        public PlayerPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            playbackProgress.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            playbackProgress.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);

            SetupTimer();

            mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;

            mediaPlayer.InteractiveActivationMode = Microsoft.PlayerFramework.InteractionType.None;

            mediaPlayer.BufferingProgressChanged += mediaPlayer_BufferingProgressChanged;

            // reset session info controls
            sessionInfo.Text = "";
            transcodingProgress.Value = 0;

            // process the play action
            JObject playRequestData = e.Parameter as JObject;

            MediaItem mediaItem = null;
            long startIndex = 0;

            try
            {
                JObject data = (JObject)playRequestData["Data"];
                if (data["StartPositionTicks"] != null)
                {
                    startIndex = (long)data["StartPositionTicks"];
                    startIndex = (startIndex / 1000) / 10000;
                }
                JArray itemIds = (JArray)data["ItemIds"];

                itemId = (string)itemIds[0];

                
                mediaItem = await client.GetItemInfo(itemId);
            }
            catch(Exception exception)
            {
                MessageDialog msg = new MessageDialog(exception.Message, "Error Retreiving Playback Info");
                msg.ShowAsync();
            }

            playbackProgress.Minimum = 0;
            playbackProgress.Maximum = mediaItem.duration;

            PlaybackAction(startIndex);
        }

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            long seekTime = (long)playbackProgress.Value;
            PlaybackAction(seekTime);

            _sliderpressed = false;
        }

        private void _timer_Tick(object sender, object e)
        {
            if (!_sliderpressed)
            {
                progress.Text = mediaPlayer.Position.Add(new TimeSpan(0, 0, (int)_seekTo)).ToString(@"hh\:mm\:ss");
                playbackProgress.Value = mediaPlayer.Position.TotalSeconds + _seekTo;
            }
        }

        private async void _playbackCheckin_Tick(object sender, object e)
        {
            long possition = (long)playbackProgress.Value;

            client.PlaybackCheckinProgress(itemId, possition);

            SessionInfo info = await client.GetSessionInfo();

            if (info != null)
            {
                transcodingProgress.Value = info.CompletionPercentage;

                String transInfo = "";
                transInfo += "Audio Codec: " + info.AudioCodec + "\n";
                transInfo += "AudioChannels: " + info.AudioChannels + "\n";
                transInfo += "Video Codec: " + info.VideoCodec + "\n";
                transInfo += "Width x Height: " + info.Width + "x" + info.Height + "\n";
                transInfo += "Container: " + info.Container + "\n";
                transInfo += "Bitrate: " + info.Bitrate + "\n";
                transInfo += "Framerate: " + info.Framerate + "\n";
                transInfo += "Complete: " + info.CompletionPercentage + "\n";

                sessionInfo.Text = transInfo;
            }
            
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);

            _playbackCheckin = new DispatcherTimer();
            _playbackCheckin.Interval = TimeSpan.FromSeconds(10);

            StartTimer();
        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _playbackCheckin.Tick += _playbackCheckin_Tick;
            _playbackCheckin.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;

            _playbackCheckin.Stop();
            _playbackCheckin.Tick -= _playbackCheckin_Tick;
        }

        private void mediaPlayer_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            int percent = (int)(mediaPlayer.BufferingProgress * 100);
            stats.Text = percent.ToString() + "%";
        }

        private async void mediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string error = e.ErrorMessage;
            if (e.OriginalSource != null)
            {
                error += "\n" + e.OriginalSource.ToString();
            }

            MessageDialog msgDialog = new MessageDialog(error, "Error Playing Media");
            await msgDialog.ShowAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Close();
            StopTimer();

            long possition = (long)playbackProgress.Value;
            client.PlaybackCheckinStopped(itemId, possition);

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                mediaPlayer.Pause();
            }
            else if (mediaPlayer.CurrentState == MediaElementState.Paused)
            {
                mediaPlayer.Play();
            }
        }

        private void PlaybackAction(long startAtSeconds)
        {
            _seekTo = startAtSeconds;

            stats.Text = playbackProgress.Value.ToString();

            long startTicks = startAtSeconds * 1000 * 10000;

            string server = settings.GetServer();

            string mediaFile = "http://" + server + "/mediabrowser/Videos/" + itemId + "/stream.ts" +
                "?audioChannels=2&" +
                "AudioStreamIndex=1&" +
                "deviceId=" + settings.GetDeviceId() + "&" +
                "Static=false&" +
                "mediaSourceId=" + itemId + "&" +
                "VideoCodec=h264&" +
                "AudioCodec=aac&" +
                "maxWidth=1920&" +
                "videoBitrate=4000000&" +
                "audioBitrate=128000&" +
                "EnableAutoStreamCopy=true&" +
                "StartTimeTicks=" + startTicks;

            mediaPlayer.Source = new Uri(mediaFile, UriKind.Absolute);

            client.PlaybackCheckinStarted(itemId, startAtSeconds);
        }
    }
}
