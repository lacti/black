﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ConditionalDebug;
using Dirichlet.Numerics;
using JetBrains.Annotations;
using MessagePack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Admin : MonoBehaviour
{
    public static Admin instance;

    [SerializeField]
    Subcanvas adminMenuSubcanvas;

    [SerializeField]
    Button changeButton;

    [SerializeField]
    EventSystem eventSystem;

    [SerializeField]
    SaveLoadManager saveLoadManager;

    [SerializeField]
    GameObject spawnButton;

    [SerializeField]
    Image toggleAdUnitIdMode;

    void OnEnable()
    {
        if (BlackContext.instance != null)
        {
            BlackContext.instance.CheatMode = true;
            BlackLogManager.Add(BlackLogEntry.Type.GameCheatEnabled, 0, 0);
        }
    }

    void Start()
    {
        UpdateAdminState();
    }

    public void DeleteSaveFileAndReloadScene()
    {
#if BLACK_ADMIN
        SaveLoadManager.DeleteSaveFileAndReloadScene();
#endif
    }

    public void GoldToZero()
    {
#if BLACK_ADMIN
        BlackContext.instance.SetGold(0);
#endif
    }

    public void GemToZero()
    {
#if BLACK_ADMIN
        BlackContext.instance.SetGemZero();
        BlackLogManager.Add(BlackLogEntry.Type.GemToZero, 0, 0);
#endif
    }

    // long is not supported on Unity Inspector's On Click() setting UI
    [UsedImplicitly]
    public void Gold(int amount)
    {
#if BLACK_ADMIN
        BlackContext.instance.AddGoldSafe((uint) amount);
#endif
    }

    public void ManyGold()
    {
#if BLACK_ADMIN
        BlackContext.instance.SetGold(UInt128.MaxValue);
        BlackLogManager.Add(BlackLogEntry.Type.GoldAddAdmin, 1, UInt128.MaxValue.ToClampedLong());
#endif
    }

    // long is not supported on Unity Inspector's On Click() setting UI
    [UsedImplicitly]
    public void Gem(int amount)
    {
#if BLACK_ADMIN
        BlackContext.instance.AddFreeGem((uint) amount);
        BlackLogManager.Add(BlackLogEntry.Type.GemAddAdmin, 2, amount);
#endif
    }

    [UsedImplicitly]
    public void TestVibrate()
    {
#if BLACK_ADMIN && (UNITY_ANDROID || UNITY_IOS)
        // VIBRATE 속성을 AndroidManifest.xml에 들어가게 하도록 하기 위해...
        // (노티피케이션에서 필요한 권한임)
        Handheld.Vibrate();
#endif
    }

    public void EnableLogging(bool b)
    {
#if BLACK_ADMIN
        Debug.unityLogger.logEnabled = b;
#endif
    }

    public void GoToBlackMakeContest()
    {
#if BLACK_ADMIN
        SceneManager.LoadScene("Contest");
#endif
    }

    [UsedImplicitly]
    public void GoToMain()
    {
#if BLACK_ADMIN
        Splash.LoadSplashScene();
#endif
    }
    
    [UsedImplicitly]
    public void GoToIso()
    {
#if BLACK_ADMIN
        SceneManager.LoadScene("Iso");
#endif
    }

    public void BackToYesterday()
    {
#if BLACK_ADMIN
        BlackContext.instance.LastDailyRewardRedeemedTicks = 0;
        BlackContext.instance.UpdateDailyRewardAllButtonStates();
        CloseAdminMenu();
#endif
    }

    public void Fastforward()
    {
#if BLACK_ADMIN
        Time.timeScale = 100;
#endif
    }

    public void NormalTimeScale()
    {
#if BLACK_ADMIN
        Time.timeScale = 1;
#endif
    }

    public void CorruptSaveFileAndLoadSplash()
    {
#if BLACK_ADMIN
        for (var i = 0; i < SaveLoadManager.maxSaveDataSlot; i++)
        {
            File.WriteAllBytes(SaveLoadManager.SaveFileName,
                Encoding.ASCII.GetBytes(ErrorReporter.RandomString(512)));
            SaveLoadManager.IncreaseSaveDataSlotAndWrite();
        }

        Splash.LoadSplashScene();
#endif
    }

    public void OpenMergeBlackShow()
    {
#if BLACK_ADMIN
        GameObject.Find("Canvas (Whac-A-Cat)").transform.Find("Merge Black Show").gameObject.SetActive(true);
        CloseAdminMenu();
#endif
    }

    public void ShowSavedGameSelectUI()
    {
        // 유저용 응급 기능이다. BLACK_ADMIN으로 감싸지 말것.
        GameObject.Find("Platform After Login").GetComponent<PlatformAfterLogin>().ShowSavedGameSelectUI();
    }

    public void SpawnTest()
    {
#if BLACK_ADMIN
        ExecuteEvents.Execute(spawnButton, new PointerEventData(eventSystem), ExecuteEvents.pointerClickHandler);
#endif
    }

    public void StartTest()
    {
#if BLACK_ADMIN
        CloseAdminMenu();

        // 자동 테스트 전체는 'blackTester.StartTest()'를 호출해서 시작된다.
        // 다만 테스트 과정을 개발하는 동안에는 각 부분에 대한 테스트를 빠르게 하기 위해서
        // 그 이하의 각 테스트 부분의 StartCoroutine을 수동으로 호출해가며 확인한다.

        var blackTesterLoaded = SceneManager.GetSceneByName("Black Tester").isLoaded;
        if (blackTesterLoaded == false) SceneManager.LoadScene("Black Tester", LoadSceneMode.Additive);

        //var blackTester = GameObject.Find("Black Tester").GetComponent<BlackTester>();
        //blackTester.StartTest();

        //blackTester.StartCoroutine(blackTester.DragOneBlackToAnotherCoro());
        //blackTester.StartCoroutine(blackTester.OpenShopPopupAndBuyNonpurchaseProductAtIndex(2));
        //blackTester.StartCoroutine(blackTester.StartSimpleLoopCoro());
#endif
    }

    public void LoadFromUserSaveCode(string domain)
    {
#if BLACK_ADMIN
        if (Application.isEditor)
        {
            ConfirmPopup.instance.OpenInputFieldPopup($"Firestore Save Document Code (Domain:{domain})",
                () => { ErrorReporter.instance.ProcessUserSaveCode(ConfirmPopup.instance.InputFieldText, domain); },
                ConfirmPopup.instance.Close, "ADMIN", Header.Normal, "", "");
            CloseAdminMenu();
        }
        else
        {
            // 실 기기에서 다른 유저 데이터를 받아버리면, 치트 플래그가 올라가지 않은 상태로
            // 플레이하게 되는 것으로써, 리더보드/업적 등 진행 상황이 개발자 실기기 연동 계정에
            // 반영이 되게 된다. 이에 대한 방지책을 마련하기 전까지는 이 기능은 개발 컴퓨터에서만
            // 작동되도록 한다.
            ShortMessage.instance.Show("Only supported in Editor", true);
        }
#endif
    }

    public void ChangeLanguage()
    {
#if BLACK_ADMIN
        //한국어 -> 일본어 -> 중국어 (간체) -> 중국어 (번체) -> 한국어 ... (반복)

        var text = changeButton.GetComponentInChildren<Text>();
        switch (Data.instance.CurrentLanguageCode)
        {
            case BlackLanguageCode.Ko:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ja);
                text.text = "현재: 한국어\n일본어로 바꾼다";
                break;
            case BlackLanguageCode.Ja:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ch);
                text.text = "현재: 일본어\n중국어(간체)로 바꾼다";
                break;
            case BlackLanguageCode.Ch:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Tw);
                text.text = "현재: 중국어(간체)\n중국어(번체)로 바꾼다";
                break;
            case BlackLanguageCode.Tw:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ko);
                text.text = "현재: 중국어(번체)\n한국어로 바꾼다";
                break;
        }
#endif
    }

    // 일단 메인 플레이가 되는 상황에서 유저가 저장 데이터를 개발팀에게 제출하고 싶은 경우 쓰는 메뉴
    public async void ReportSaveDataAsync()
    {
        // 유저용 응급 기능이다. BLACK_ADMIN으로 감싸지 말것.
        // 저장 한번 하고

        StageSaveData stageSaveData = null;

        ConDebug.Log($"=== {nameof(ReportSaveDataAsync)} ===");
        ConDebug.Log($"LastClearedStageId: {BlackContext.instance.LastClearedStageId}");
        var currentStageMetadata = await StageDetailPopup.LoadStageMetadataByZeroBasedIndexAsync(BlackContext.instance.LastClearedStageId);
        if (currentStageMetadata != null)
        {
            ConDebug.Log($"WIP Stage Metadata: {currentStageMetadata.name}");
            var stageSaveFileName = StageSaveManager.GetStageSaveFileName(currentStageMetadata.name);
            ConDebug.Log($"WIP Stage Save File Name: {stageSaveFileName}");
            byte[] stageSaveDataBytes;
            try
            {
                stageSaveDataBytes = File.ReadAllBytes(FileUtil.GetPath(stageSaveFileName));
                ConDebug.Log($"WIP Stage Save File Size: {stageSaveDataBytes.Length} bytes");
            }
            catch (Exception e)
            {
                stageSaveDataBytes = null;
                Debug.LogWarning(e);
            }
            
            if (stageSaveDataBytes != null)
            {
                var wipPngFileName = StageSaveManager.GetWipPngFileName(currentStageMetadata.name);
                ConDebug.Log($"WIP PNG Save File Name: {wipPngFileName}");
                byte[] wipPngBytes;
                try
                {
                    wipPngBytes = File.ReadAllBytes(FileUtil.GetPath(wipPngFileName));
                    ConDebug.Log($"WIP PNG File Size: {wipPngBytes.Length} bytes");
                }
                catch (Exception e)
                {
                    wipPngBytes = null;
                    Debug.LogWarning(e);
                }

                try
                {
                    ConDebug.Log("Deserializing WIP stage save data...");
                    stageSaveData =
                        MessagePackSerializer.Deserialize<StageSaveData>(stageSaveDataBytes, Data.DefaultOptions);
                    stageSaveData.png = wipPngBytes;
                    ConDebug.Log("DONE!");
                }
                catch
                {
                    stageSaveData = null;
                }
            }
        }
        else
        {
            ConDebug.Log($"Current Stage Metadata: ??? Empty ???");
        }
        
        SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, stageSaveData);
        // 제출 시작한다.
        await ErrorReporter.instance.UploadSaveFileIncidentAsync(new List<Exception>(), "NO CRITICAL ERROR",
            true);
    }

    public async void ReportPlayLogAsync()
    {
        // 유저용 응급 기능이다. BLACK_ADMIN으로 감싸지 말것.
        try
        {
            // 저장 한번 하고
            SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, null);
            // 제출 시작한다.
            var reasonPhrase = await BlackLogManager.DumpAndUploadPlayLog("\\플레이 로그 업로드 중...".Localized(), "", "", "");
            if (string.IsNullOrEmpty(reasonPhrase))
            {
                var errorDeviceId = ErrorReporter.instance.GetOrCreateErrorDeviceId();
                var msg = "\\플레이 로그 업로드가 성공적으로 완료됐습니다.\\n\\n업로드 코드: {0}".Localized(errorDeviceId);
                ConfirmPopup.instance.Open(msg);
            }
            else
            {
                throw new Exception(reasonPhrase);
            }
        }
        catch (Exception e)
        {
            var msg = "\\플레이 로그 업로드 중 오류가 발생했습니다.\\n\\n{0}".Localized(e.Message);
            Debug.LogException(e);
            ConfirmPopup.instance.Open(msg);
        }
        finally
        {
            ProgressMessage.instance.Close();
        }
    }

    [UsedImplicitly]
    public void ResetTimeServerListQueryStartIndex()
    {
        // 유저용 응급 기능이다. BLACK_ADMIN으로 감싸지 말것.
        NetworkTime.ResetTimeServerListQueryStartIndex();
    }

    public void SetNoticeDbPostfix()
    {
        // 유저가 쓰더라도 해가 없다. BLACK_ADMIN으로 감싸지 말것.
        SetNoticeDbPostfixToDev();
    }

    public static void SetNoticeDbPostfixToDev()
    {
        ConfigPopup.noticeDbPostfix = "Dev";
    }

    public void SetOnSavedGameOpenedAndWriteAlwaysInternalError()
    {
#if BLACK_ADMIN
        PlatformAndroid.OnSavedGameOpenedAndWriteAlwaysInternalError = true;
#endif
    }

    // 업적 진행 상황은 그대로 두고, 보상 수령 현황만 리셋해서
    // 다시 보상을 처음부터 모두 받을 수 있도록 한다.
    public void ClearAchievementRedeemHistory()
    {
#if BLACK_ADMIN
        BlackContext.instance.AchievementRedeemed = new AchievementRecord1(false);
#endif
    }

    void CloseAdminMenu()
    {
        adminMenuSubcanvas.Close();
    }

    public async void ShowDummyProgressMessage()
    {
        CloseAdminMenu();
        ShortMessage.instance.Show("5초 후 테스트용 Progress Message창이 열립니다.");
        await Task.Delay(TimeSpan.FromSeconds(5));
        ProgressMessage.instance.Open("테스트 중... 10초 후 닫힙니다.");
        await Task.Delay(TimeSpan.FromSeconds(10));
        ProgressMessage.instance.Close();
    }

    public void GetAllDailyRewardsAtOnceAdminToDay()
    {
#if BLACK_ADMIN
        ConfirmPopup.instance.OpenInputFieldPopup(
            $"Redeem Daily Rewards To Day ({BlackContext.instance.LastDailyRewardRedeemedIndex} ~ {Data.dataSet.DailyRewardData.Count})",
            () =>
            {
                if (int.TryParse(ConfirmPopup.instance.InputFieldText, out var toDay))
                    BlackContext.instance.GetAllDailyRewardsAtOnceAdminToDay(toDay);

                ConfirmPopup.instance.Close();
            }, ConfirmPopup.instance.Close, "Admin", Header.Normal, "", "");
        CloseAdminMenu();
#endif
    }

    public void RefillAllCoins()
    {
#if BLACK_ADMIN
        BlackContext.instance.CoinAmount = 5;
#endif
    }

    public void ToggleSlowMode()
    {
#if BLACK_ADMIN
        BlackContext.instance.SlowMode = !BlackContext.instance.SlowMode;
        ShortMessage.instance.Show($"SlowMode to {BlackContext.instance.SlowMode}");
#endif
    }

    public void CalculateGoldRate()
    {
#if BLACK_ADMIN
        ConfirmPopup.instance.OpenInputFieldPopup("Gold Rate", () =>
        {
            PrintGoldRate(ConfirmPopup.instance.InputFieldText);
            ConfirmPopup.instance.Close();
        }, ConfirmPopup.instance.Close, "Admin", Header.Normal, "", "");
        CloseAdminMenu();
#endif
    }

#if BLACK_ADMIN
    static void PrintGoldRate(string goldRateBigIntStr)
    {
        if (BigInteger.TryParse(goldRateBigIntStr, out var n))
        {
            //var n = BigInteger.Parse("256");
            ConDebug.Log(n.ToString("n0"));
            var nb = n.ToByteArray();
            var level = 0;
            foreach (var t in nb)
                for (var j = 0; j < 8; j++)
                {
                    level++;
                    if (((t >> j) & 1) == 1) ConDebug.Log($"Level {level} = {BigInteger.One << (level - 1):n0}/s");
                }
        }
        else
        {
            Debug.LogError("Invalid number format");
        }
    }
#endif

    public void ToggleNoAdsCode()
    {
#if BLACK_ADMIN
        BlackContext.instance.NoAdsCode = BlackContext.instance.NoAdsCode == 0 ? 19850506 : 0;
        ShortMessage.instance.Show($"No Ads Code set to {BlackContext.instance.NoAdsCode}");
#endif
    }

#if BLACK_ADMIN
    void Update()
    {
        if (Keyboard.current[Key.F5].wasReleasedThisFrame)
        {
            ScreenCapture.CaptureScreenshot("Test");
        }
    }
#endif

    public void TestAd()
    {
#if BLACK_ADMIN
        CloseAdminMenu();
        var adContext = new BlackAdContext(() =>
        {
            ScUInt128 goldAmount = 1;
            BlackContext.instance.AddGoldSafe(goldAmount);
            ConfirmPopup.instance.Open(@"\광고 시청 보상으로 {0}골드를 받았습니다.".Localized(goldAmount));
        });
        PlatformAdMobAds.instance.TryShowRewardedAd(adContext);
#endif
    }
    
    public void SetLastClearedStageId()
    {
#if BLACK_ADMIN
        ConfirmPopup.instance.OpenInputFieldPopup("Last Cleared Stage ID",
            () =>
            {
                if (int.TryParse(ConfirmPopup.instance.InputFieldText, out var stageId))
                {
                    BlackContext.instance.LastClearedStageId = stageId;
                }
                ConfirmPopup.instance.Close();
            },
            ConfirmPopup.instance.Close, "ADMIN", Header.Normal, BlackContext.instance.LastClearedStageId.ToString(), "");
        CloseAdminMenu();
#endif
    }
    
    public static bool IsAdUnitIdModeTest => PlayerPrefs.GetInt("AD_UNIT_ID_MODE", 0) != 0;

    public void ToggleAdUnitIdMode()
    {
        PlayerPrefs.SetInt("AD_UNIT_ID_MODE", IsAdUnitIdModeTest ? 0 : 1);
        PlayerPrefs.Save();

        UpdateAdminState();
    }

    void UpdateAdminState()
    {
        if (toggleAdUnitIdMode != null)
        {
            toggleAdUnitIdMode.color = IsAdUnitIdModeTest ? Color.yellow : Color.white;
        }
    }
}