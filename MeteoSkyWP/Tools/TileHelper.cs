using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace MeteoSkyWP.Tools
{
    public static class TileHelper
    {
        public static async Task PinSecondaryTile(string tileId, string tileLabel, string url, Action updateTileAction)
        {
            App.OnNewTilePinned = updateTileAction;

            // Prepare package images for all four tile sizes in our tile to be pinned as well as for the square30x30 logo used in the Apps view.  
            Uri square150x150Logo = new Uri("ms-appx:///Assets/square150x150Tile.png");
            Uri wide310x150Logo = new Uri("ms-appx:///Assets/wide310x150Tile.png");
            //Uri square310x310Logo = new Uri("ms-appx:///Assets/square310x310Tile-sdk.png");
            //Uri square30x30Logo = new Uri("ms-appx:///Assets/square30x30Tile-sdk.png");

            // During creation of secondary tile, an application may set additional arguments on the tile that will be passed in during activation.
            // These arguments should be meaningful to the application. In this sample, we'll pass in the date and time the secondary tile was pinned.
            string tileActivationArguments = url.Replace("tendances", "previsions");

            // Create a Secondary tile with all the required arguments.
            // Note the last argument specifies what size the Secondary tile should show up as by default in the Pin to start fly out.
            // It can be set to TileSize.Square150x150, TileSize.Wide310x150, or TileSize.Default.  
            // If set to TileSize.Wide310x150, then the asset for the wide size must be supplied as well.
            // TileSize.Default will default to the wide size if a wide size is provided, and to the medium size otherwise. 
            SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                            tileLabel,
                                                            tileActivationArguments,
                                                            square150x150Logo,
                                                            TileSize.Square150x150);

            // Like the background color, the square30x30 logo is inherited from the parent application tile by default. 
            // Let's override it, just to see how that's done.
            secondaryTile.VisualElements.Wide310x150Logo = wide310x150Logo;

            // The display of the secondary tile name can be controlled for each tile size.
            // The default is false.
            secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;
            secondaryTile.VisualElements.ShowNameOnWide310x150Logo = true;

            // Specify a foreground text value.
            // The tile background color is inherited from the parent unless a separate value is specified.
            //secondaryTile.VisualElements.ForegroundText = ForegroundText.Dark;

            // Set this to false if roaming doesn't make sense for the secondary tile.
            // The default is true;
            secondaryTile.RoamingEnabled = false;

            // OK, the tile is created and we can now attempt to pin the tile.

            // Since pinning a secondary tile on Windows Phone will exit the app and take you to the start screen, any code after 
            // RequestCreateForSelectionAsync or RequestCreateAsync is not guaranteed to run.  For an example of how to use the OnSuspending event to do
            // work after RequestCreateForSelectionAsync or RequestCreateAsync returns, see Scenario9_PinTileAndUpdateOnSuspend in the SecondaryTiles.WindowsPhone project.

            await secondaryTile.RequestCreateAsync();
        }
    }
}
