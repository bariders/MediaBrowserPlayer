using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace SmartPlayer.Classes
{
    public class TileNotifications
    {
        public async Task<bool> DeleteOldImages()
        {
            var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();

            foreach (StorageFile file in files)
            {
                if (file.Name.StartsWith("TileImage-"))
                {
                    MetroEventSource.Log.Info("Delete Tile Image  : " + file.Name);
                    await file.DeleteAsync();
                }
            }

            return true;
        }

        public async Task<bool> UpdateTileNotifications()
        {
            MetroEventSource.Log.Info("Setting Tile Notifications Started");

            await DeleteOldImages();

            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            AppSettings settings = new AppSettings();
            ApiClient client = new ApiClient();

            ServerListItem server = settings.GetServer();
            if(server == null)
            {
                return false;
            }

            List<MediaItem> items = await client.GetResentItems();

            foreach(MediaItem item in items)
            {
                string itemImage = "http://" + server + "/mediabrowser/Items/" + item.Id + "/Images/Thumb?Width=310&Height=150";
                string name = item.Name;
                string imageName = "TileImage-" + item.Id + ".jpg";

                // get image data and save it
                byte[] image = await client.GetImage(item.Id, "Thumb", 310, 150, "jpg");
                if(image != null)
                {
                    var storageFileTask = await ApplicationData.Current.LocalFolder.CreateFileAsync(imageName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(storageFileTask, image);

                    XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Image);

                    //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150ImageAndText01);
                    //XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
                    //tileTextAttributes[0].InnerText = name;

                    XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
                    ((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appdata:///local/" + imageName);
                    ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "item thumb");

                    TileNotification tileNotification = new TileNotification(tileXml);

                    tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddDays(90);

                    TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
                }
            }

            MetroEventSource.Log.Info("Setting Tile Notifications Started");

            return true;
        }

    }
}
