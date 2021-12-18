using UnityEngine;
using UnityEngine.Advertisements;

// TODO: work out how this is supposed to be used.
namespace FlowFree
{
    public class Banner_Ad : MonoBehaviour
    {
        [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;
        //private BannerPlaceholder _placeholder; TODO: Placeholder?
        [SerializeField] string _androidAdUnitId = "Banner_Android";
        [SerializeField] string _iOSAdUnitId = "Banner_iOS";
        string _adUnitId;

        void Awake()
        {
            // Get the Ad Unit ID for the current platform:
            _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
                ? _iOSAdUnitId
                : _androidAdUnitId;
        }
        void Start()
        {
            // Set the banner position:
            Advertisement.Banner.SetPosition(_bannerPosition);
        }

        // Implement a method to call when the Load Banner button is clicked:
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

        // Implement code to execute when the loadCallback event triggers:
        void OnBannerLoaded()
        {
            Debug.Log("Banner loaded");
        }

        // Implement code to execute when the load errorCallback event triggers:
        void OnBannerError(string message)
        {
            Debug.Log($"Banner Error: {message}");
            // Optionally execute additional code, such as attempting to load another ad.
        }

        // Implement a method to call when the Show Banner button is clicked:
        public void ShowBannerAd()
        {
            // Show the loaded Banner Ad Unit:
            Advertisement.Banner.Show(_adUnitId);
        }
        
    }
}