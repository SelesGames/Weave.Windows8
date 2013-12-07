using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels.StartHub
{
    public class StartAdvertisingViewModel : StartItemBase
    {
        private const String WeaveImage = "/Assets/Ads/160x600-WeaveWP.png";
        public const String WeaveWpUrl = "http://windowsphone.com/s?appid=25f7c2fa-ca89-49a7-9937-c7347be73fec";

        private bool _showFallbackAd = false;
        public bool ShowFallbackAd
        {
            get { return _showFallbackAd; }
            set { SetProperty(ref _showFallbackAd, value); }
        }

        private String _fallbackAdPath = WeaveImage;
        public String FallbackAdPath
        {
            get { return _fallbackAdPath; }
            private set { SetProperty(ref _fallbackAdPath, value); }
        }

        public void ExecuteFallbackAd()
        {
            if (String.Equals(FallbackAdPath, WeaveImage))
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri(WeaveWpUrl));
            }
        }
    }
}
