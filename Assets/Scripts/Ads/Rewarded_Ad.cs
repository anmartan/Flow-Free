using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

namespace FlowFree
{

	public class Rewarded_Ad : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
	{
		[SerializeField] string _androidAdUnitId = "Rewarded_Android";
		[SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
		string _adUnitId;

		void Awake()
		{
			// Get the Ad Unit ID for the current platform:
			_adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
				? _iOSAdUnitId
				: _androidAdUnitId;
		}
		
		// Load content to the Ad Unit:
		public void LoadAd()
		{
			// IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
			Debug.Log("Loading Ad: " + _adUnitId);
			Advertisement.Load(_adUnitId, this);
		}
		
		// Implement a method to execute when the user clicks the button.
		public void ShowAd()
		{
			Advertisement.Show(_adUnitId, this);
		}

		// Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
		public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
		{
			if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
			{
				Debug.Log("Unity Ads Rewarded Ad Completed");
				// Grant a reward.

				// Load another ad:
				Advertisement.Load(_adUnitId, this);
			}
		}

		// Implement Load and Show Listener error callbacks:
		public void OnUnityAdsAdLoaded(string placementId)
		{
			
		}

		public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
		{
			Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
			// Use the error details to determine whether to try to load another ad.
		}

		public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
		{
			Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
			// Use the error details to determine whether to try to load another ad.
		}

		public void OnUnityAdsShowStart(string adUnitId)
		{
		}

		public void OnUnityAdsShowClick(string adUnitId)
		{
		}

		void OnDestroy()
		{
		}
	}
}