using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MediaBrowserPlayer.Classes
{
    public class TileNotifications
    {

        public async Task<bool> UpdateTileNotifications()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            AppSettings settings = new AppSettings();
            ApiClient client = new ApiClient();

            string server = settings.GetServer();
            if(server == null)
            {
                return false;
            }

            List<MediaItem> items = await client.GetResentItems();
            int count = 1;
            foreach(MediaItem item in items)
            {
                string itemImage = "http://" + server + "/mediabrowser/Items/" + item.Id + "/Images/Thumb?Width=310&Height=150";
                string name = item.Name + " 08";

                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150ImageAndText01);

                XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
                tileTextAttributes[0].InnerText = name + " A " + count++;

                XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
                ((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appdata:///local/" + item.Id + ".jpg");
                //((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appx:///assets/Logo.scale-100.png");

                //((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appx:///assets/" + item.Id + ".jpg");

                //((XmlElement)tileImageAttributes[0]).SetAttribute("src", "http://192.168.0.15:8096/mediabrowser/Items/ac456952a0644d28e5c5498f3f65f978/Images/Thumb?Width=310&Height=150");
                //((XmlElement)tileImageAttributes[0]).SetAttribute("src", itemImage);
                ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "red graphic");

                TileNotification tileNotification = new TileNotification(tileXml);

                tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddDays(1);

                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }

            return true;
        }


    }
}
