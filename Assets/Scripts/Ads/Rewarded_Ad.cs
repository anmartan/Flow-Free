using System;
using UnityEngine;
using UnityEngine.Advertisements;

// TODO: work out how this is supposed to be used.
namespace FlowFree
{

	public class Rewarded_Ad : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
	{
		[SerializeField] string _androidAdUnitId = "Rewarded_Android";
		string _adUnitId;

		void Awake()
		{
			_adUnitId = _androidAdUnitId;
		}

		private void Start()
		{
			LoadAd();
		}

		public void LoadAd()
		{
			Advertisement.Load(_adUnitId, this);
		}
		
		public void ShowAd()
		{
			Advertisement.Show(_adUnitId, this);
		}

		public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
		{
			Debug.Log(adUnitId);
			if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
			{
				// Grant a reward.
				GameManager.Instance().AddHint();
				// Load another ad:
				Advertisement.Load(_adUnitId, this);
			}
		}

		public void OnUnityAdsAdLoaded(string placementId)
		{
		}

		public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
		{
			Debug.LogError($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
			// Use the error details to determine whether to try to load another ad.
		}

		public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
		{
			Debug.LogError($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
			// Use the error details to determine whether to try to load another ad.
		}

		public void OnUnityAdsShowStart(string adUnitId)
		{
		}

		public void OnUnityAdsShowClick(string adUnitId)
		{
		}
	}
}