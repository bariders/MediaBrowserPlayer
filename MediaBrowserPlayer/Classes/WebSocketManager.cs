using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web;

namespace MediaBrowserPlayer.Classes
{

    class WebSocketManager
    {
        private AppSettings settings = new AppSettings();
        private MessageWebSocket webSocket = null;

        public WebSocketManager()
        {

        }

        public void CloseWebSocket()
        {
            if(webSocket != null)
            {
                try
                {
                    webSocket.Close(0, "App Closing");
                }
                catch (Exception)
                { }
                webSocket = null;
            }
        }

        public async void SetupWebSocket()
        {
            try
            {
                string server = settings.GetServer();
                if (server == null)
                {
                    throw new Exception("Server not set");
                }

                Uri serverUri = new Uri("ws://" + server + "/mediabrowser");
                webSocket = new MessageWebSocket();
                webSocket.Control.MessageType = SocketMessageType.Utf8;

                webSocket.MessageReceived += MessageReceived;

                webSocket.Closed += Closed;

                await webSocket.ConnectAsync(serverUri);

                DataWriter messageWriter = new DataWriter(webSocket.OutputStream);

                string deviceName = settings.GetDeviceName();
                string value = "MBP";
                if (string.IsNullOrEmpty(deviceName) == false)
                {
                    value = "MBP-" + settings.GetDeviceName();
                }

                string identityMessage = "{\"MessageType\":\"Identity\", \"Data\":\"Windows RT|" + settings.GetDeviceId() + "|0.0.1|" + value + "\"}";
                messageWriter.WriteString(identityMessage);
                await messageWriter.StoreAsync();
            }
            catch(Exception e)
            {
                string errorString = "Error Creating WebSocket : " + e.Message;
                MessageDialog msg = new MessageDialog(errorString, "WebSocket Error");
                msg.ShowAsync();
            }

        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string read = reader.ReadString(reader.UnconsumedBufferLength);

                    // parse the message 
                    JObject messageObject = null;
                    try
                    {
                        messageObject = JObject.Parse(read);
                    }
                    catch(Exception e)
                    {
                        messageObject = null;
                    }

                    // if we have an object and it is of type play
                    if (messageObject != null && "Play" == (string)messageObject["MessageType"])
                    {
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            Frame rootFrame = Window.Current.Content as Frame;

                            var p = rootFrame.Content as MainPage;
                            if (p != null)
                            {
                                p.LogMessage(read);
                            }

                            rootFrame.Navigate(typeof(PlayerPage), messageObject);
                        });
                    }


                }
            }
            catch (Exception ex) // For debugging
            {
                WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                // Add your specific error-handling code here.
            }
        }

        private void Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            // You can add code to log or display the code and reason
            // for the closure (stored in args.Code and args.Reason)

            // This is invoked on another thread so use Interlocked 
            // to avoid races with the Start/Close/Reset methods.
            if (webSocket != null)
            {
                MessageWebSocket ws = Interlocked.Exchange(ref webSocket, null);
                if (ws != null)
                {
                    ws.Dispose();
                }
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Frame rootFrame = Window.Current.Content as Frame;
                    var p = rootFrame.Content as MainPage;
                    p.LogMessage("SocketClosed");
                });
            }
        }

    }
}
