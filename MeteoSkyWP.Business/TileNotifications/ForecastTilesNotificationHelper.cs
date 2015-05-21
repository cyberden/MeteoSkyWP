using MeteoSkyWP.DataModel;
using NotificationsExtensions.TileContent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;

namespace MeteoSkyWP.Business.TileNotifications
{
    public static class ForecastTilesNotificationHelper
    {
        public static void NotifyTile(string tileId, ForecastElement forecastElement, string city)
        {
            var currentElement = forecastElement;

            //string tileImage = "/Assets/wide310x150Tile.png";
            //if (generateOk)
              //  tileImage = "tileId" + ".png";

            // Create a notification for the Wide310x150 tile using one of the available templates for the size.
            ITileWide310x150PeekImage02 tileContent = TileContentFactory.CreateTileWide310x150PeekImage02();
            tileContent.Image.Src = forecastElement.TileWeatherIconPath;
            tileContent.TextHeading.Text = currentElement.Weather;
            tileContent.TextBody1.Text = string.Format("Heure : {0}", currentElement.Hour);
            tileContent.TextBody2.Text = string.Format("Temp : {0}", currentElement.Temperature);
            tileContent.TextBody3.Text = string.Format("Pluie : {0}", currentElement.Rain);

            // Create a notification for the Square150x150 tile using one of the available templates for the size.
            ITileSquare150x150PeekImageAndText01 square150x150Content = TileContentFactory.CreateTileSquare150x150PeekImageAndText01();
            square150x150Content.Image.Src = forecastElement.TileWeatherIconPath;
            square150x150Content.Branding = TileBranding.Logo;
            square150x150Content.TextHeading.Text = currentElement.Weather;
            square150x150Content.TextBody1.Text = string.Format("Heure : {0}", currentElement.Hour);
            square150x150Content.TextBody2.Text = string.Format("Temp : {0}", currentElement.Temperature);
            square150x150Content.TextBody3.Text = string.Format("Pluie : {0}", currentElement.Rain);

            // Attach the Square150x150 template to the Wide310x150 template.
            tileContent.Square150x150Content = square150x150Content;

            // Send the notification to the application? tile.
            TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId).Update(tileContent.CreateNotification());
        }

        private static async Task<bool> GenerateImage(string tileId)
        {
            DataReader dr = null;
            IRandomAccessStream outputStream = null;
            
            try
            {

                var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                var file = await folder.GetFileAsync("customTile.xml");
                string szCustomTileXML = await Windows.Storage.FileIO.ReadTextAsync(file);

                Border border = (Border)XamlReader.Load(szCustomTileXML);
                RenderTargetBitmap rtb = new RenderTargetBitmap();

                await rtb.RenderAsync(border, 150, 150);
                var pixelBuffers = await rtb.GetPixelsAsync();

                dr = DataReader.FromBuffer(pixelBuffers);
                byte[] data = new byte[pixelBuffers.Length];
                dr.ReadBytes(data);

            
                var outputFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFileAsync("tileId" + ".png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                outputStream = await outputFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                outputStream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);

                uint myUint = (uint)96;


                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, myUint, myUint, data);
                await encoder.FlushAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (dr != null)
                    dr.Dispose();

                if (outputStream != null)
                    outputStream.Dispose();
            }

            return true;
        }
    }
}
