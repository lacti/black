﻿using System.Collections.Generic;
using AudienceNetwork;
#if BLACK_FACEBOOK
using Facebook.Unity;
#endif
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformFacebookAdsInit : MonoBehaviour
{
    public static PlatformFacebookAdsInit instance;

    bool didClose;
    bool didCompleted;

    bool init = false;

    [SerializeField]
    PlatformFacebookAds platformFacebookAds;

    [SerializeField]
    PlatformInterface platformInterface;

    RewardedVideoAd rewardedVideoAd;

    public bool IsAdCurrentlyCalled { get; set; } = true;

    bool IsLoaded { get; set; }

    string PLACEMENT_ID => PlatformInterface.instance.config.GetFacebookAdsPlacementId();

    void Awake()
    {
        instance = this;
        if (init == false)
        {
            init = true;
#if BLACK_FACEBOOK
            if (Application.isEditor == false) FB.Init(OnInitComplete, OnHideUnity);
#endif
        }

        if (Application.isEditor == false)
            foreach (var deviceId in PlatformInterface.instance.config.FacebookAdsTestDeviceIdList)
                AdSettings.AddTestDevice(deviceId);
    }

    void OnHideUnity(bool isUnityShown)
    {
    }

    void OnInitComplete()
    {
        if (Application.isEditor == false)
        {
#if BLACK_FACEBOOK
            FB.LogAppEvent("init completed", null, new Dictionary<string, object>
            {
                {AppEventParameterName.Description, "PlatformFacebookInit FB Init completed"}
            });
#endif
        }
    }

    public void LoadAndShowRewardedVideo()
    {
        LoadRewardedVideo();
    }

    void LoadRewardedVideo()
    {
        rewardedVideoAd = new RewardedVideoAd(PLACEMENT_ID);

        rewardedVideoAd.Register(gameObject);

        rewardedVideoAd.RewardedVideoAdDidLoad = delegate
        {
            PlatformInterface.instance.logger.Log("RewardedVideo ad loaded.");
            IsLoaded = true;
            didClose = false;
            //showAfterLoaded
            if (IsAdCurrentlyCalled) Show();
        };

        rewardedVideoAd.RewardedVideoAdDidFailWithError = delegate(string error)
        {
            PlatformInterface.instance.logger.Log("RewardedVideo ad failed to load with error: " + error);
            platformFacebookAds.HandleShowResult(false, error);
        };

        rewardedVideoAd.RewardedVideoAdWillLogImpression = delegate
        {
            PlatformInterface.instance.logger.Log("RewardedVideo ad logged impression.");
        };

        rewardedVideoAd.RewardedVideoAdDidClick = delegate
        {
            PlatformInterface.instance.logger.Log("RewardedVideo ad clicked.");
        };

        rewardedVideoAd.RewardedVideoAdDidSucceed = delegate
        {
            PlatformInterface.instance.logger.Log("Rewarded video ad validated by server");
            // 검증 로직 없을 때는 불리지 않는 듯 하다.
        };

        rewardedVideoAd.RewardedVideoAdDidFail = delegate
        {
            PlatformInterface.instance.logger.Log("Rewarded video ad not validated, or no response from server");
            platformFacebookAds.HandleShowResult(false, "DidFail");
        };

        rewardedVideoAd.RewardedVideoAdComplete = delegate
        {
            PlatformInterface.instance.logger.Log("Rewarded video ad completed");
            didCompleted = true;
        };

        rewardedVideoAd.RewardedVideoAdDidClose = delegate
        {
            PlatformInterface.instance.logger.Log("Rewarded video ad did close.");
            didClose = true;
            platformFacebookAds.HandleShowResult(didCompleted, "");
            rewardedVideoAd?.Dispose();
        };

        if (Application.platform == RuntimePlatform.Android) /*
             * Only relevant to Android.
             * This callback will only be triggered if the Rewarded Video activity
             * has been destroyed without being properly closed. This can happen if
             * an app with launchMode:singleTask (such as a Unity game) goes to
             * background and is then relaunched by tapping the icon.
             */
            rewardedVideoAd.RewardedVideoAdActivityDestroyed = () =>
            {
                if (!didClose)
                {
                    PlatformInterface.instance.logger.Log(
                        "Rewarded video activity destroyed without being closed first.");
                    PlatformInterface.instance.logger.Log("Game should resume. User should not get a reward.");
                    platformFacebookAds.HandleShowResult(false, "ActivityDestroyed");
                }
            };

        // Initiate the request to load the ad.
        rewardedVideoAd.LoadAd();
    }

    void Show()
    {
        if (IsLoaded)
        {
            didCompleted = false;
            rewardedVideoAd.Show();
            IsLoaded = false;
        }
    }

    void OnDestroy()
    {
        // Dispose of rewardedVideo ad when the scene is destroyed
        rewardedVideoAd?.Dispose();
        PlatformInterface.instance.logger.Log("RewardedVideoAdTest was destroyed!");
    }
}