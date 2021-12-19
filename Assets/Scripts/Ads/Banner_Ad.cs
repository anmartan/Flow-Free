using UnityEngine;
using UnityEngine.Advertisements;

// TODO: work out how this is supposed to be used.
namespace FlowFree
{
    public class Banner_Ad : MonoBehaviour
    {
        [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;
        [SerializeField] string _androidAdUnitId = "Banner_Android";
        string _adUnitId;

        void Awake()
        {
            // Get the Ad Unit ID for the current platform:
            _adUnitId = _androidAdUnitId;
        }
        void Start()
        {
            // Set the banner position:
            Advertisement.Banner.SetPosition(_bannerPosition);
            ShowBannerAd();
        }

        public void LoadBanner()
        {
            // Set up options to notify the SDK of load events:
            BannerLoadOptions options = new BannerLoadOptions
            {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerError,
            };
            // Load the Ad Unit with banner content:
            Advertisement.Banner.Load(_adUnitId, options);
        }

        void OnBannerLoaded()
        {
        }

        void OnBannerError(string message)
        {
            Debug.LogError($"Banner Error: {message}");
        }

        public void ShowBannerAd()
        {
            Advertisement.Banner.Show(_adUnitId);
        }
        
    }
}