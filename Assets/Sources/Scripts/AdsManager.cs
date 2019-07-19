using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour
{
    public static AdsManager instance { get; set; }

    bool isRemoveAds = false;
    // 테스트 광고 ID
    public const string unityAdsGameId = "1111111";

    string skipVideoPlacementId = "video";
    public string bannerPlacement = "banner";
    private int interstitialWatchGameCount = 3;
    private int MAX_COUNT = 3;

    void Awake()
    {
        instance = this; 
    }

    void Start()
    {
        interstitialWatchGameCount = MAX_COUNT;
        UnityAdsInit();
        if (Advertisement.isSupported)
        {
            Advertisement.Initialize(unityAdsGameId);
        }
        else
        {
            MessagePopManager.instance.ShowPop("UnityAds 지원불가");
        }

        StartCoroutine(ShowBannerWhenReady());
    }

    void OnEnable()
    {
        GameManager.OnGameReady += OnGameReady;
    }

    void OnDisable()
    {
        GameManager.OnGameReady -= OnGameReady;
    }

    private void OnGameReady()
    {
        if (--interstitialWatchGameCount <= 0)
        {
            interstitialWatchGameCount = MAX_COUNT;
            ShowAd();
        }       
    }

    // Unity ADS ==================================================
    private void UnityAdsInit()
    {
        if (Advertisement.isSupported)
        {
            Advertisement.Initialize(unityAdsGameId);
        } else {
            MessagePopManager.instance.ShowPop("UnityAds 지원불가");
        }
    }

    public bool GetReadyAds()
    {
        return Advertisement.IsReady(skipVideoPlacementId);
    }

    public void SetRemoveAds()
    {
        isRemoveAds = true;
    }

    // 리워드 동영상 광고 시청 _ 추가 체력
    public void ShowAd()
    {
        //isRemoveAds = true;

        if (!isRemoveAds)
        {            
            if (GetReadyAds())
            {
                ShowOptions options = new ShowOptions { resultCallback = ShowAdHandleResult };
                Advertisement.Show(skipVideoPlacementId, options);                
            }
            else
            {
                MessagePopManager.instance.ShowPop("[!]Not Ready Ads");
            }
        }
    }

    IEnumerator ShowBannerWhenReady()
    {
        Debug.LogError("ShowBannerWhenReady: "+ Advertisement.IsReady(bannerPlacement));
        while (!Advertisement.IsReady(bannerPlacement))
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(bannerPlacement);
    }



    private void ShowAdHandleResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("광고 보기 완료");
                break;
            case ShowResult.Skipped:
                Debug.Log("광고 스킵");
                break;
            case ShowResult.Failed:
                Debug.Log("광고 보기 실패");
                break;
        }
    }
}