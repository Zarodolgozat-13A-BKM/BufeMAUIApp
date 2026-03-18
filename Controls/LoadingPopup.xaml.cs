using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Devices;

namespace BufeApp.Controls
{
    public partial class LoadingPopup : Popup
    {
        public LoadingPopup()
        {
            InitializeComponent();
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            Size = new Size(mainDisplayInfo.Width / mainDisplayInfo.Density, mainDisplayInfo.Height / mainDisplayInfo.Density);
        }
    }
}
