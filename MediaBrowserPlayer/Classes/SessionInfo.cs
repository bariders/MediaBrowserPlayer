using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowserPlayer.Classes
{
    class SessionInfo
    {
        public string Id = "";

        public string AudioCodec = "";
        public string VideoCodec = "";
        public string Container = "";
        public int Bitrate = 0;
        public double Framerate = 0;
        public double CompletionPercentage = 0;
        public int Width = 0;
        public int Height = 0;
        public int AudioChannels = 0;

    }
}
