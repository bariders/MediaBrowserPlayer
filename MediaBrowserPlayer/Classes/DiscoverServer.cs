using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;


namespace MediaBrowserPlayer.Classes
{
    class DiscoverServer
    {
        public delegate void DataReceived(string discoverData);
        public DataReceived dataReceived;

        private DatagramSocket socket = null;
        public string discoverResponce = null;
        private AutoResetEvent autoEvent = null;

        public DiscoverServer()
        {
            socket = new DatagramSocket();

            socket.MessageReceived += SocketOnMessageReceived;
        }

        public async void DiscoverNow(AutoResetEvent ae)
        {
            autoEvent = ae;
            SendMessage("who is MediaBrowserServer_v2?", 7359);
        }

        private async Task SendMessage(string message, int port)
        {
            socket.MessageReceived += SocketOnMessageReceived;

            using (var stream = await socket.GetOutputStreamAsync(new HostName("255.255.255.255"), port.ToString()))
            {
                using (var writer = new DataWriter(stream))
                {
                    var data = Encoding.UTF8.GetBytes(message);

                    writer.WriteBytes(data);
                    await writer.StoreAsync();
                }

                stream.Dispose();
            }
        }

        private async void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                var result = args.GetDataStream();
                var resultStream = result.AsStreamForRead(1024);

                using (var reader = new StreamReader(resultStream))
                {
                    var text = await reader.ReadToEndAsync();
                    discoverResponce = text;
                    if (autoEvent != null)
                    {
                        autoEvent.Set();
                    }
                    dataReceived(text);
                }
            }
            catch (Exception e)
            { }
        }
    }
}
