using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace MediaBrowserPlayer.Classes
{
    class ApiClient
    {
        private AppSettings settings = new AppSettings();

        private string accessToken = null;
        private string userId = null;

        public ApiClient()
        {

        }

        public async Task<string> GetAuthorizationHeader(bool includeUser = true)
        {
            //MediaBrowser UserId="e8837bc1-ad67-520e-8cd2-f629e3155721", Client="Android", Device="Samsung Galaxy SIII", DeviceId="xxx", Version="1.0.0.0"

            string value = "MediaBrowser ";

            if (includeUser)
            {
                value += "UserId=\"" + await GetUserID() + "\", ";
            }

            value += "Client=\"Windows RT\", ";

            string deviceName = settings.GetDeviceName();
            if (string.IsNullOrEmpty(deviceName))
            {
                value += "Device=\"MBP\", ";
            }
            else
            {
                value += "Device=\"MBP-" + settings.GetDeviceName() + "\", ";
            }

            value += "DeviceId=\"" + settings.GetDeviceId() + "\", ";

            value += "Version=\"0.0.1\"";

            return value;
        }

        public async Task<bool> SetCapabilities()
        {
            bool worked = false;
            SessionInfo sessionInfo;
            sessionInfo = await GetSessionInfo();

            if(sessionInfo == null)
            {
                return false;
            }

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Sessions/Capabilities?Id=" + sessionInfo.Id + "&PlayableMediaTypes=Video&SupportedCommands=Play");

            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string authToken = await Authenticate();
            httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", authToken);

            JObject jsonData = new JObject();

            jsonData.Add("Id", sessionInfo.Id);
            jsonData.Add("PlayableMediaTypes", "Video");

            HttpContent myContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");

            var responce = await httpClient.PostAsync(url, myContent);
            responce.EnsureSuccessStatusCode();

            return worked;
        }

        public async Task<SessionInfo> GetSessionInfo()
        {
            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Sessions?DeviceId=" + settings.GetDeviceId() + "&format=json");

            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string authToken = await Authenticate();
            httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", authToken);

            HttpResponseMessage itemResponce = await httpClient.GetAsync(url);
            itemResponce.EnsureSuccessStatusCode();

            string responceText = await itemResponce.Content.ReadAsStringAsync();

            JArray sessionInfoList = JArray.Parse(responceText);

            SessionInfo info = null;

            if (sessionInfoList.Count > 0)
            {
                JObject sessionInfo = (JObject)sessionInfoList[0];

                // build the responce object
                info = new SessionInfo();

                info.Id = (string)sessionInfo["Id"];

                JObject transcodingInfo = (JObject)sessionInfo["TranscodingInfo"];
                if (transcodingInfo != null)
                {
                    info.AudioCodec = (transcodingInfo["AudioCodec"] != null) ? (string)transcodingInfo["AudioCodec"] : "";
                    info.VideoCodec = (transcodingInfo["VideoCodec"] != null) ? (string)transcodingInfo["VideoCodec"] : "";
                    info.Container = (transcodingInfo["Container"] != null) ? (string)transcodingInfo["Container"] : "";
                    info.Bitrate = (transcodingInfo["Bitrate"] != null) ? (int)transcodingInfo["Bitrate"] : 0;
                    info.Framerate = (transcodingInfo["Framerate"] != null) ? (double)transcodingInfo["Framerate"] : 0;
                    info.CompletionPercentage = (transcodingInfo["CompletionPercentage"] != null) ? (double)transcodingInfo["CompletionPercentage"] : 0;
                    info.Width = (transcodingInfo["Width"] != null) ? (int)transcodingInfo["Width"] : 0;
                    info.Height = (transcodingInfo["Height"] != null) ? (int)transcodingInfo["Height"] : 0;
                    info.AudioChannels = (transcodingInfo["AudioChannels"] != null) ? (int)transcodingInfo["AudioChannels"] : 0;
                }
            }

            return info;
        }

        public async void PlaybackCheckinProgress(string itemId, long position)
        {
            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string authToken = await Authenticate();
            httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", authToken);

            JObject jsonData = new JObject();

            JArray queueable = new JArray();
            //queueable.Add("Video");
            jsonData.Add("QueueableMediaTypes", queueable);
            jsonData.Add("CanSeek", true);
            jsonData.Add("ItemId", itemId);
            jsonData.Add("MediaSourceId", "");
            jsonData.Add("IsPaused", false);
            jsonData.Add("IsMuted", false);
            jsonData.Add("PositionTicks", (position * 1000 * 10000));
            jsonData.Add("PlayMethod", "Transcode");

            string playbackData = jsonData.ToString();

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Sessions/Playing/Progress");
            HttpContent myContent = new StringContent(playbackData, Encoding.UTF8, "application/json");
            var responce = await httpClient.PostAsync(url, myContent);
            //responce.EnsureSuccessStatusCode();
        }

        public async void PlaybackCheckinStopped(string itemId, long position)
        {
            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string authToken = await Authenticate();
            httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", authToken);

            JObject jsonData = new JObject();

            JArray queueable = new JArray();
            //queueable.Add("Video");
            jsonData.Add("QueueableMediaTypes", queueable);
            jsonData.Add("CanSeek", true);
            jsonData.Add("ItemId", itemId);
            jsonData.Add("MediaSourceId", "");
            jsonData.Add("IsPaused", false);
            jsonData.Add("IsMuted", false);
            jsonData.Add("PositionTicks", (position * 1000 * 10000));
            jsonData.Add("PlayMethod", "Transcode");

            string playbackData = jsonData.ToString();

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Sessions/Playing/Stopped");
            HttpContent myContent = new StringContent(playbackData, Encoding.UTF8, "application/json");
            var responce = await httpClient.PostAsync(url, myContent);
            //responce.EnsureSuccessStatusCode();
        }

        public async void PlaybackCheckinStarted(string itemId, long position)
        {
            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string authToken = await Authenticate();
            httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", authToken);

            JObject jsonData = new JObject();

            JArray queueable = new JArray();
            //queueable.Add("Video");
            jsonData.Add("QueueableMediaTypes", queueable);
            jsonData.Add("CanSeek", true);
            jsonData.Add("ItemId", itemId);
            jsonData.Add("MediaSourceId", "");
            jsonData.Add("IsPaused", false);
            jsonData.Add("IsMuted", false);
            jsonData.Add("PositionTicks", (position * 1000 * 10000));
            jsonData.Add("PlayMethod", "Transcode");

            string playbackData = jsonData.ToString();

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Sessions/Playing");
            HttpContent myContent = new StringContent(playbackData, Encoding.UTF8, "application/json");
            var responce = await httpClient.PostAsync(url, myContent);
            //responce.EnsureSuccessStatusCode();
        }

        public async Task<string> Authenticate()
        {
            if (accessToken != null)
            {
                return accessToken;
            }

            //await GetUserID();

            string userName = settings.GetUserName();
            string password = settings.GetPassword();

            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            IBuffer hash = provider.HashData(buffer);
            string hashString = CryptographicBuffer.EncodeToHexString(hash);

            JObject authJsonData = new JObject();
            authJsonData.Add("password", hashString);
            authJsonData.Add("username", userName);
            string authData = authJsonData.ToString();

            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Users/AuthenticateByName?format=json");

            HttpContent myContent = new StringContent(authData, Encoding.UTF8, "application/json");

            var responce = await httpClient.PostAsync(url, myContent);

            if(responce.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errormessage = "Error In Authentication:\n" + responce.StatusCode.ToString() + " - " + responce.ReasonPhrase;

                string reason = "";
                foreach(var header in responce.Headers)
                {
                    string headerName = header.Key;
                    if(headerName == "X-Application-Error-Code")
                    {
                        foreach (string value in header.Value)
                        {
                            reason += value + "\n";
                        }
                    }
                }
                if (string.IsNullOrEmpty(reason) == false)
                {
                    errormessage += errormessage + "\n" + reason;
                }

                throw new Exception(errormessage);
            }

            string data = await responce.Content.ReadAsStringAsync();
            JObject authObject = JObject.Parse(data);

            string accessTokenString = (string)authObject["AccessToken"];
            accessToken = accessTokenString;

            return accessToken;
        }

        public async Task<MediaItem> GetItemInfo(string itemId)
        {
            MediaItem item = new MediaItem();

            string userId = await GetUserID();

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Users/" + userId + "/Items/" + itemId + " ?format=json");

            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            string authToken = await Authenticate();
            httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", authToken);

            HttpResponseMessage itemResponce = await httpClient.GetAsync(url);
            itemResponce.EnsureSuccessStatusCode();

            string itemResponceText = await itemResponce.Content.ReadAsStringAsync();

            JObject itemInfo = JObject.Parse(itemResponceText);

            if (itemInfo["RunTimeTicks"] != null)
            {
                long runTimeSeconds = (long)itemInfo["RunTimeTicks"];
                runTimeSeconds = (runTimeSeconds / 1000) / 10000;
                item.duration = runTimeSeconds;
            }
            else
            {
                item.duration = 0;
                App.AddNotification(new Notification() { Title = "Media Item Error", Message = "The media item has no duration" });
            }

            return item;
        }

        public async Task<string> GetUserID()
        {
            if (userId != null)
            {
                return userId;
            }

            string server = settings.GetServer();
            if (server == null)
            {
                throw new Exception("Server not set");
            }

            Uri url = new Uri("http://" + server + "/mediabrowser/Users?format=json");

            HttpClient httpClient = new HttpClient();

            string authorization = await GetAuthorizationHeader(false);
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBodyAsText = await response.Content.ReadAsStringAsync();

            JArray json = JArray.Parse(responseBodyAsText);

            string username = settings.GetUserName();

            foreach (JObject obj in json)
            {
                string name = (string)obj["Name"];
                if (name != null && username.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    userId = (string)obj["Id"];
                    break;
                }
            }

            if (userId == null)
            {
                throw new Exception("User name (" + username + ") not found");
            }

            return userId;
        }


    }
}
