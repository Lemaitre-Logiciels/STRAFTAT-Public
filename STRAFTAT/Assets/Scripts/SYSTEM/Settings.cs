using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System;
using FishNet;
using UnityEngine.SceneManagement;
using Achievements = HeathenEngineering.SteamworksIntegration.API.StatsAndAchievements.Client;
using HeathenEngineering.SteamworksIntegration;
using Newtonsoft.Json.Linq;

public enum VoiceChatMode
{
    OpenMic,
    PushToTalk,
    ToggleMute
}

public class Settings : MonoBehaviour, ISaveable
{
    public static Settings Instance;
    public FirstPersonController localPlayer;
    private MapsManager mapsManager;

    private void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }

        if (PlayerPrefs.GetInt("firstTimeSave") != 256)
            FirstTimeSetup();
    }

    public List<string> recentlyPlayedWithPlayers = new List<string>();
    public void AddPlayerToHistory(ClientInstance player) {
        SteamFriends.SetPlayedWith((CSteamID)player.PlayerSteamID);
        string tmp = player.PlayerName + " (SteamID : " + player.PlayerSteamID + ") (Time : " + DateTime.Now.ToString() + ")";
        recentlyPlayedWithPlayers.Insert(0, tmp);
        while (recentlyPlayedWithPlayers.Count > 500) {
            recentlyPlayedWithPlayers.RemoveAt(recentlyPlayedWithPlayers.Count - 1);
        }
    }

    public void CopyMatchHistory()
    {
        string tmp = "PLAYERNAME | STEAMID | DATE OF PLAY" + Environment.NewLine + Environment.NewLine;
        for (int i=0; i<recentlyPlayedWithPlayers.Count; i++) {
            tmp += i+": " + recentlyPlayedWithPlayers[i] + Environment.NewLine;
        }
        PauseManager.Instance.WriteOfflineLog("Match history copied to clipboard");
        GUIUtility.systemCopyBuffer = tmp;
    }

    [Header("Player settings")]
    public bool toggleTwoAxis = false;
    public bool invertMouseX = false;
    public bool invertMouseY = false;
    public bool sprintToggle = false;
    public bool aimToggle = false;
    public bool leanToggle = false;
    public bool crouchToggle = false;
    public bool reverseSprintBind = false;
    public bool inverseFireBinding = false;
    public float mouseSensitivity = 2.0f;
    public float mouseAimScopeSensitivity = 0.8f;
    public float mouseAimSensitivity = 2;
    public float horizontalSensitivity = 2.0f;
    public float verticalSensitivity = 2.0f;
    public float normalFovValue = 75.0f;
    public float brightness = 1;
    public float damageIntensity = 1;

    public bool disableCrosshair = false;
    public bool enableFixedCrosshair = false;
    public bool showSpeedometer = false;

    [Header("Audio settings")]
    public float voiceChatVolume = 1.0f;
    public bool enableVoiceChat = true;
    public VoiceChatMode voiceChatMode = VoiceChatMode.PushToTalk;

    public bool inGameMusic = false; // New

    [Header("Application settings")]
    public int useVsync = 0;
    public bool isFullscreen = true;
    public bool exclusiveFullscreen = false;
    public bool minimalistUi = false;
    public bool motionBlur = false;
    public int targetFps = 120;
    public bool targetFpsToggle;
    
    public bool reduceVFX = false;

    void Start()
    {
        mapsManager = MapsManager.Instance;
        FirstTimeSetup();
    }

    void FirstTimeSetup()
    {
        if (PlayerPrefs.HasKey("toggleTwoAxis")) toggleTwoAxis = (PlayerPrefs.GetInt("toggleTwoAxis") == 1 ? true : false);
        if (PlayerPrefs.HasKey("invertMouseX")) invertMouseX = (PlayerPrefs.GetInt("invertMouseX") == 1 ? true : false);
        if (PlayerPrefs.HasKey("invertMouseY")) invertMouseY = (PlayerPrefs.GetInt("invertMouseY") == 1 ? true : false);
        if (PlayerPrefs.HasKey("sprintToggle")) sprintToggle = (PlayerPrefs.GetInt("sprintToggle") == 1 ? true : false);
        if (PlayerPrefs.HasKey("aimToggle")) aimToggle = (PlayerPrefs.GetInt("aimToggle") == 1 ? true : false);
        if (PlayerPrefs.HasKey("leanToggle")) leanToggle = (PlayerPrefs.GetInt("leanToggle") == 1 ? true : false);
        if (PlayerPrefs.HasKey("crouchToggle")) crouchToggle = (PlayerPrefs.GetInt("crouchToggle") == 1 ? true : false);
        if (PlayerPrefs.HasKey("reverseSprintBind")) reverseSprintBind = (PlayerPrefs.GetInt("reverseSprintBind") == 1 ? true : false);
        if (PlayerPrefs.HasKey("isFullscreen")) isFullscreen = (PlayerPrefs.GetInt("isFullscreen") == 1 ? true : false);
        if (PlayerPrefs.HasKey("enableVoiceChat")) enableVoiceChat = (PlayerPrefs.GetInt("enableVoiceChat") == 1 ? true : false);
        if (PlayerPrefs.HasKey("minimalistUi")) minimalistUi = (PlayerPrefs.GetInt("minimalistUi") == 1 ? true : false);
        if (PlayerPrefs.HasKey("motionBlur")) motionBlur = (PlayerPrefs.GetInt("motionBlur") == 1 ? true : false);
        if (PlayerPrefs.HasKey("inverseFireBinding")) inverseFireBinding = (PlayerPrefs.GetInt("inverseFireBinding") == 1 ? true : false);
        if (PlayerPrefs.HasKey("mouseSensitivity")) mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
        if (PlayerPrefs.HasKey("mouseAimScopeSensitivity")) mouseAimScopeSensitivity = PlayerPrefs.GetFloat("mouseAimScopeSensitivity");
        if (PlayerPrefs.HasKey("mouseAimSensitivity")) mouseAimSensitivity = PlayerPrefs.GetFloat("mouseAimSensitivity");
        if (PlayerPrefs.HasKey("mouseXSensitivity")) horizontalSensitivity = PlayerPrefs.GetFloat("mouseXSensitivity");
        if (PlayerPrefs.HasKey("mouseYSensitivity")) verticalSensitivity = PlayerPrefs.GetFloat("mouseYSensitivity");
        if (PlayerPrefs.HasKey("fovValue")) normalFovValue = PlayerPrefs.GetFloat("fovValue");
        if (PlayerPrefs.HasKey("brightness")) brightness = PlayerPrefs.GetFloat("brightness");
        if (PlayerPrefs.HasKey("damageIntensity")) damageIntensity = PlayerPrefs.GetFloat("damageIntensity");
        if (PlayerPrefs.HasKey("voiceChatVolume")) voiceChatVolume = PlayerPrefs.GetFloat("voiceChatVolume");
        if (PlayerPrefs.HasKey("qualitySetting")) qualitySetting = PlayerPrefs.GetInt("qualitySetting");
        if (PlayerPrefs.HasKey("targetFps")) targetFps = PlayerPrefs.GetInt("targetFps", 120);
        if (PlayerPrefs.HasKey("targetFpsToggle")) targetFpsToggle = (PlayerPrefs.GetInt("targetFpsToggle") == 1);
        if (PlayerPrefs.HasKey("resolution")) resolution = PlayerPrefs.GetInt("resolution", 0);
        if (PlayerPrefs.HasKey("useVsync")) useVsync = PlayerPrefs.GetInt("useVsync", 0);
        if (PlayerPrefs.HasKey("exclusiveFullscreen")) exclusiveFullscreen = (PlayerPrefs.GetInt("exclusiveFullscreen") == 1 ? true : false);
        if (PlayerPrefs.HasKey("voiceChatMode")) voiceChatMode = (VoiceChatMode)PlayerPrefs.GetInt("voiceChatMode");
        if (PlayerPrefs.HasKey("reduceVFX")) reduceVFX = (PlayerPrefs.GetInt("reduceVFX", 0) == 1 ? true : false);

        if (PlayerPrefs.HasKey("inGameMusic")) inGameMusic = (PlayerPrefs.GetInt("inGameMusic") == 1 ? true : false);
        if (PlayerPrefs.HasKey("enableFixedCrosshair")) enableFixedCrosshair = (PlayerPrefs.GetInt("enableFixedCrosshair") == 1 ? true : false);
        if (PlayerPrefs.HasKey("showSpeedometer")) showSpeedometer = (PlayerPrefs.GetInt("showSpeedometer") == 1 ? true : false);
        if (PlayerPrefs.HasKey("disableCrosshair")) disableCrosshair = (PlayerPrefs.GetInt("disableCrosshair") == 1 ? true : false);

        if (PlayerPrefs.GetInt("firstTimeSave") == 256) {
            AudioListener.volume = PlayerPrefs.GetFloat("masterVolume");
            SoundManager.Instance._effectsSource.volume = PlayerPrefs.GetFloat("effectsVolume");
            SoundManager.Instance._musicSource.volume = PlayerPrefs.GetFloat("musicVolume");
            SoundManager.Instance._ambientSource.volume = PlayerPrefs.GetFloat("ambientVolume");
            SoundManager.Instance.menuMusicVolume = PlayerPrefs.GetFloat("menuMusicVolume");
            
        }
        
        if (!PlayerPrefs.HasKey("toggleTwoAxis")) PlayerPrefs.SetInt("toggleTwoAxis", toggleTwoAxis ? 1 : 0);
        if (!PlayerPrefs.HasKey("invertMouseX")) PlayerPrefs.SetInt("invertMouseX", invertMouseX ? 1 : 0);
        if (!PlayerPrefs.HasKey("invertMouseY")) PlayerPrefs.SetInt("invertMouseY", invertMouseY ? 1 : 0);
        if (!PlayerPrefs.HasKey("sprintToggle")) PlayerPrefs.SetInt("sprintToggle", sprintToggle ? 1 : 0);
        if (!PlayerPrefs.HasKey("aimToggle")) PlayerPrefs.SetInt("aimToggle", aimToggle ? 1 : 0);
        if (!PlayerPrefs.HasKey("leanToggle")) PlayerPrefs.SetInt("leanToggle", leanToggle ? 1 : 0);
        if (!PlayerPrefs.HasKey("crouchToggle")) PlayerPrefs.SetInt("crouchToggle", crouchToggle ? 1 : 0);
        if (!PlayerPrefs.HasKey("reverseSprintBind")) PlayerPrefs.SetInt("reverseSprintBind", reverseSprintBind ? 1 : 0);
        if (!PlayerPrefs.HasKey("isFullscreen")) PlayerPrefs.SetInt("isFullscreen", isFullscreen ? 1 : 0);
        if (!PlayerPrefs.HasKey("enableVoiceChat")) PlayerPrefs.SetInt("enableVoiceChat", enableVoiceChat ? 1 : 0);
        if (!PlayerPrefs.HasKey("minimalistUi")) PlayerPrefs.SetInt("minimalistUi", minimalistUi ? 1 : 0);
        if (!PlayerPrefs.HasKey("motionBlur")) PlayerPrefs.SetInt("motionBlur", motionBlur ? 1 : 0);
        if (!PlayerPrefs.HasKey("inverseFireBinding")) PlayerPrefs.SetInt("inverseFireBinding", inverseFireBinding ? 1 : 0);
        if (!PlayerPrefs.HasKey("mouseSensitivity")) PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
        if (!PlayerPrefs.HasKey("mouseAimScopeSensitivity")) PlayerPrefs.SetFloat("mouseAimScopeSensitivity", mouseAimScopeSensitivity);
        if (!PlayerPrefs.HasKey("mouseAimSensitivity")) PlayerPrefs.SetFloat("mouseAimSensitivity", mouseSensitivity);
        if (!PlayerPrefs.HasKey("mouseXSensitivity")) PlayerPrefs.SetFloat("mouseXSensitivity", horizontalSensitivity);
        if (!PlayerPrefs.HasKey("mouseYSensitivity")) PlayerPrefs.SetFloat("mouseYSensitivity", verticalSensitivity);
        if (!PlayerPrefs.HasKey("fovValue")) PlayerPrefs.SetFloat("fovValue", normalFovValue);
        if (!PlayerPrefs.HasKey("brightness")) PlayerPrefs.SetFloat("brightness", brightness);
        if (!PlayerPrefs.HasKey("damageIntensity")) PlayerPrefs.SetFloat("damageIntensity", damageIntensity);
        if (!PlayerPrefs.HasKey("voiceChatVolume")) PlayerPrefs.SetFloat("voiceChatVolume", voiceChatVolume);
        if (!PlayerPrefs.HasKey("qualitySetting")) PlayerPrefs.SetInt("qualitySetting", 3);
        if (!PlayerPrefs.HasKey("targetFps")) PlayerPrefs.SetInt("targetFps", 120);
        if (!PlayerPrefs.HasKey("targetFpsToggle")) PlayerPrefs.SetInt("targetFpsToggle", targetFpsToggle ? 1 : 0);
        if (!PlayerPrefs.HasKey("resolution")) PlayerPrefs.SetInt("resolution", 0);
        if (!PlayerPrefs.HasKey("useVsync")) PlayerPrefs.SetInt("useVsync", 0);
        if (!PlayerPrefs.HasKey("exclusiveFullscreen")) PlayerPrefs.SetInt("exclusiveFullscreen", exclusiveFullscreen ? 1 : 0);
        if (!PlayerPrefs.HasKey("voiceChatMode")) PlayerPrefs.SetInt("voiceChatMode", (int)voiceChatMode);
        if (!PlayerPrefs.HasKey("reduceVFX")) PlayerPrefs.SetInt("reduceVFX", reduceVFX ? 1 : 0);

        if (!PlayerPrefs.HasKey("inGameMusic")) PlayerPrefs.SetInt("inGameMusic", inGameMusic ? 1 : 0);
        if (!PlayerPrefs.HasKey("enableFixedCrosshair")) PlayerPrefs.SetInt("enableFixedCrosshair", enableFixedCrosshair ? 1 : 0);
        if (!PlayerPrefs.HasKey("showSpeedometer")) PlayerPrefs.SetInt("showSpeedometer", showSpeedometer ? 1 : 0);
        if (!PlayerPrefs.HasKey("disableCrosshair")) PlayerPrefs.SetInt("disableCrosshair", disableCrosshair ? 1 : 0);

        if (PlayerPrefs.GetInt("firstTimeSave") != 256) {
            PlayerPrefs.SetInt("firstTimeSave", 256);
            
            PlayerPrefs.SetFloat("masterVolume", 1);
            PlayerPrefs.SetFloat("effectsVolume", 1);
            PlayerPrefs.SetFloat("musicVolume", 1);
            PlayerPrefs.SetFloat("ambientVolume", 1);
            PlayerPrefs.SetFloat("menuMusicVolume", 0.6f);

        }

        PlayerPrefs.Save();

        if (targetFpsToggle) {
            if (Application.targetFrameRate != targetFps && targetFps > 30) {
                Application.targetFrameRate = targetFps;
            }
        } else {
            Application.targetFrameRate = -1; // No target FPS
        }
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl)){ SendAchievementUnlockText("Test Achievement"); }
        #endif

        if (QualitySettings.vSyncCount != useVsync) QualitySettings.vSyncCount = useVsync;
        
        SteamAchievementsCheck();
        
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            tauntsAmountText.text = "taunts : " + tauntsAmount.ToString();
            killsAmountText.text = "kills : " + killsAmount.ToString();
            deathsAmountText.text = "deaths : " + deathsAmount.ToString();
            suicidesAmountText.text = "suicides : " + suicidesAmount.ToString();
            bodyshotsAmountText.text = "bodyshots : " + bodyshotsAmount.ToString();
            headshotsAmountText.text = "headshots : " + headshotsAmount.ToString();
            timeSpentInGameText.text = "time spent in game : " + ((Mathf.Floor(timeSpentInGame / 3600)) + ":" + (int)((timeSpentInGame/60) % 60) + ":" + (int)(timeSpentInGame % 60)).ToString() + " hours";
            timeSpentInAirText.text = "time spent in air : " + ((int)(Mathf.Floor(timeSpentInAir / 3600)) + ":" + (int)((timeSpentInAir/60) % 60) + ":" + (int)(timeSpentInAir % 60)).ToString() + " hours";
            timeSpentOnGroundText.text = "time spent on ground : " + ((int)(Mathf.Floor(timeSpentOnGround / 3600)) + ":" + (int)((timeSpentOnGround/60) % 60) + ":" + (int)(timeSpentOnGround % 60)).ToString() + " hours";

            roundsWonText.text = "rounds won : " + roundsWon.ToString();
            roundsLostText.text = "rounds lost : " + roundsLost.ToString();
            roundsPlayedText.text = "rounds played : " + roundsPlayed.ToString();
            gamesLostText.text = "games lost : " +  gamesLost.ToString();
            gamesWonText.text = "games won : " +  gamesWon.ToString();
            gamesPlayedText.text = "games played : " + gamesPlayed.ToString();
            mapsUnlockedText.text = "maps unlocked : " + mapsManager.unlockedMaps.Length.ToString() + "/" + mapsManager.allMaps.Length.ToString();
        }

        if (localPlayer == null) return;

	    if (toggleTwoAxis) {
            localPlayer.lookSpeedX = this.horizontalSensitivity;
            localPlayer.lookSpeedY = this.verticalSensitivity;
        }
        else {
            localPlayer.lookSpeedX = mouseSensitivity;
            localPlayer.lookSpeedY = mouseSensitivity;
            localPlayer.lookSpeedAim = mouseAimScopeSensitivity;
            localPlayer.lookSpeedAimNoScope = mouseAimSensitivity;
	    }

        localPlayer.sprintToggle = sprintToggle;
        localPlayer.aimToggle = aimToggle;
        localPlayer.leanToggle = leanToggle;
        localPlayer.invertX = invertMouseX;
        localPlayer.invertY = invertMouseY;
        localPlayer.crouchToggle = crouchToggle;
        localPlayer.reverseSprintBind = reverseSprintBind;
    }

    [Space]
    [SerializeField] private LeaderboardManager leaderboardManager;
    
    public void UpdateElo()
    {
        int score = (int)(gamesWon * 4 + roundsWon);
        if (score > 150000) { leaderboardManager.ForceScore(int.MinValue); }
        else { leaderboardManager.UploadScore(score); }
    }

    public void ChangeVoiceChatVolume(Slider _slider){
        voiceChatVolume = _slider.value;
        PlayerPrefs.SetFloat("voiceChatVolume", voiceChatVolume);
        PlayerPrefs.Save();
    }

    public void ChangeMouseSensitivity(Slider _slider){
        mouseSensitivity = _slider.value;
        PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
        PlayerPrefs.Save();
    }

    public void ChangeAimScopeMouseSensitivity(Slider _slider){
        mouseAimScopeSensitivity = _slider.value;
        PlayerPrefs.SetFloat("mouseAimScopeSensitivity", mouseAimScopeSensitivity);
        PlayerPrefs.Save();
    }

    public void ChangeAimMouseSensitivity(Slider _slider){
        mouseAimSensitivity = _slider.value;
        PlayerPrefs.SetFloat("mouseAimSensitivity", mouseAimSensitivity);
        PlayerPrefs.Save();
    }

    public void ChangeMouseXSensitivity(Slider _slider){
        horizontalSensitivity = _slider.value;
        PlayerPrefs.SetFloat("mouseXSensitivity", horizontalSensitivity);
        PlayerPrefs.Save();
    }

    public void ChangeMouseYSensitivity(Slider _slider){
        verticalSensitivity = _slider.value;
        PlayerPrefs.SetFloat("mouseYSensitivity", verticalSensitivity);
        PlayerPrefs.Save();
    }

    public void ChangeFov(Slider _slider){
        normalFovValue = _slider.value;
        PlayerPrefs.SetFloat("fovValue", normalFovValue);
        PlayerPrefs.Save();
    }

    public void ChangeBrightness(Slider _slider){
        brightness = _slider.value;
        PlayerPrefs.SetFloat("brightness", brightness);
        PlayerPrefs.Save();
    }

    public void ChangeDamageIntensity(Slider _slider){
        damageIntensity = _slider.value;
        PlayerPrefs.SetFloat("damageIntensity", damageIntensity);
        PlayerPrefs.Save();
    }

    public void ToggleTwoAxisMouse(Toggle _toggle){
        toggleTwoAxis = _toggle.isOn;
        PlayerPrefs.SetInt("toggleTwoAxis", toggleTwoAxis ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleVerticalInvert(Toggle _toggle){
        invertMouseY = _toggle.isOn;
        PlayerPrefs.SetInt("invertMouseY", invertMouseY ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleHorizontalInvert(Toggle _toggle){
        invertMouseX = _toggle.isOn;
        PlayerPrefs.SetInt("invertMouseX", invertMouseX ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSprint(Toggle _toggle){
        sprintToggle = _toggle.isOn;
        PlayerPrefs.SetInt("sprintToggle", sprintToggle ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleAim(Toggle _toggle){
        aimToggle = _toggle.isOn;
        PlayerPrefs.SetInt("aimToggle", aimToggle ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleLean(Toggle _toggle){
        leanToggle = _toggle.isOn;
        PlayerPrefs.SetInt("leanToggle", leanToggle ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleCrouch(Toggle _toggle){
        crouchToggle = _toggle.isOn;
        PlayerPrefs.SetInt("crouchToggle", crouchToggle ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleReverseSprintBind(Toggle _toggle){
        reverseSprintBind = _toggle.isOn;
        PlayerPrefs.SetInt("reverseSprintBind", reverseSprintBind ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleMinimalistUi(Toggle _toggle){
        minimalistUi = _toggle.isOn;
        PlayerPrefs.SetInt("minimalistUi", minimalistUi ? 1 : 0);

        if (localPlayer && qualitySetting != 0) localPlayer.transform.GetComponent<PlayerSetup>().HideHUD(minimalistUi);
        PlayerPrefs.Save();
    }

    public void ToggleMotionBlur(Toggle _toggle){
        motionBlur = _toggle.isOn;
        PlayerPrefs.SetInt("motionBlur", motionBlur ? 1 : 0);
        if (localPlayer) localPlayer.transform.GetComponent<PlayerSetup>().ChangeMotionBlur(motionBlur);
        PlayerPrefs.Save();
    }

    public void InverseFireBinding(Toggle _toggle){
        inverseFireBinding = _toggle.isOn;
        PlayerPrefs.SetInt("inverseFireBinding", inverseFireBinding ? 1 : 0);
        PlayerPrefs.Save();
    }


    public void ChangeFullscreen(Toggle _toggle){
        isFullscreen = _toggle.isOn;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("isFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeVSync(Toggle _toggle){
        useVsync = _toggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("useVsync", useVsync);
        PlayerPrefs.Save();
    }

    public void ChangeVoiceChat(Toggle _toggle){
        enableVoiceChat = _toggle.isOn;
        PlayerPrefs.SetInt("enableVoiceChat", enableVoiceChat ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangePushToTalk(int _dropdown){
        voiceChatMode = (VoiceChatMode)_dropdown;
        PlayerPrefs.SetInt("voiceChatMode", (int)voiceChatMode);
        PlayerPrefs.Save();
    }

    public void EnableFixedCrosshair(Toggle _toggle){
        enableFixedCrosshair = _toggle.isOn;
        PlayerPrefs.SetInt("enableFixedCrosshair", enableFixedCrosshair ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void EnableSpeedometer(Toggle _toggle){
        showSpeedometer = _toggle.isOn;
        PlayerPrefs.SetInt("showSpeedometer", showSpeedometer ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void DisableCrosshair(Toggle _toggle){
        disableCrosshair = _toggle.isOn;
        PlayerPrefs.SetInt("disableCrosshair", disableCrosshair ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void InGameMusic(Toggle _toggle){
        inGameMusic = _toggle.isOn;
        PlayerPrefs.SetInt("inGameMusic", inGameMusic ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeExclusiveFullscreen(Toggle _toggle){
        exclusiveFullscreen = _toggle.isOn;
        PlayerPrefs.SetInt("exclusiveFullscreen", exclusiveFullscreen ? 1 : 0);
        if (isFullscreen) { Screen.fullScreenMode = (exclusiveFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.FullScreenWindow); }
        PlayerPrefs.Save();
    }


    public void ChangeResolution(int index){
        resolution = index;
        PlayerPrefs.SetInt("resolution", resolution);
        PlayerPrefs.Save();
    }


    public void ChangeFrameRate(TMP_InputField _inputField) {
        int newTargetFps;
        try { newTargetFps = int.Parse(_inputField.text); }
        catch {
            newTargetFps = 120; 
            _inputField.text = "120";
        }

        if (newTargetFps < 30) {
            newTargetFps = 30;
            _inputField.SetTextWithoutNotify("30");
        }
        
        PlayerPrefs.SetInt("targetFps", newTargetFps);
        PlayerPrefs.Save();

        targetFps = newTargetFps;
        
        if (targetFpsToggle) {
            Application.targetFrameRate = newTargetFps;
        } else {
            Application.targetFrameRate = -1; // No target FPS
        }
    }
    
    public void ChangeTargetFpsToggle(Toggle _toggle) {
        targetFpsToggle = _toggle.isOn;
        PlayerPrefs.SetInt("targetFpsToggle", targetFpsToggle ? 1 : 0);
        PlayerPrefs.Save();

        if (targetFpsToggle) {
            Application.targetFrameRate = targetFps;
        } else {
            Application.targetFrameRate = -1; // No target FPS
        }
    }
    
    public void ReduceVFX(Toggle _toggle) {
        reduceVFX = _toggle.isOn;
        PlayerPrefs.SetInt("reduceVFX", reduceVFX ? 1 : 0);
        PlayerPrefs.Save();
    }

    [Space]
    [SerializeField] private TMPro.TMP_Dropdown qualityDropdown;
    public int resolution = 0;
    public int qualitySetting = 3;

    public void SetQualityLevelDropdown()
    {
        var index = qualityDropdown.value;
        QualitySettings.SetQualityLevel(index, true);
        qualitySetting = qualityDropdown.value;

        if (qualitySetting == 0 && localPlayer) localPlayer.transform.GetComponent<PlayerSetup>().HideHUD(true);
        else if (localPlayer) localPlayer.transform.GetComponent<PlayerSetup>().HideHUD(false);

        PlayerPrefs.SetInt("qualitySetting", index);
        PlayerPrefs.Save();
    }

    public void SteamAchievementsCheck()
    {
        if (rocketJumps > 0 && !rocketJumpsHat.acquired){
            if (!rocketJumpsHatAch.IsAchieved){
                rocketJumpsHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Rocket Jump");
            }

            rocketJumpsHat.acquired = true;
            
        }
        if (windowsBroken > 19 && !windowsBrokenHat.acquired){
            if (!windowsBrokenHatAch.IsAchieved){
                windowsBrokenHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Broken Windows");
            }

            windowsBrokenHat.acquired = true;
        }
        if (headshotsAmount > 0 && !headshotHat.acquired){
            if (!headshotHatAch.IsAchieved){
                headshotHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Headshot");
            }

            headshotHat.acquired = true;
        }
        if (ragdollsThrownAway > 0 && !ragdollsThrownAwayHat.acquired){
            if (!ragdollsThrownAwayHatAch.IsAchieved){
                ragdollsThrownAwayHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Body Toss");
            }

            ragdollsThrownAwayHat.acquired = true;
        }
        if (noscope > 0 && !noscopeHat.acquired){
            if (!noscopeHatAch.IsAchieved){
                noscopeHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Noscope");
            }
            
            noscopeHat.acquired = true;
        }
        if (propKills > 0 && !propKillsHat.acquired){
            if (!propKillsHatAch.IsAchieved){
                propKillsHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Props Kill");
            }

            propKillsHat.acquired = true;
        }
        if (taserShots > 0 && !taserShotsHat.acquired){
            if (!taserShotsHatAch.IsAchieved){
                taserShotsHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Taser Shot");
            }

            taserShotsHat.acquired = true;
        }
        if (potsBroken > 49 && !potsBrokenHat.acquired){
            if (!potsBrokenHatAch.IsAchieved){
                potsBrokenHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Broken Pots");
            }

            potsBrokenHat.acquired = true;
        }
        if (gamesPlayed > 4 && !fiveGamesHat.acquired){
            if (!fiveGamesHatAch.IsAchieved){
                fiveGamesHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Straftat Gamer");
            }

            fiveGamesHat.acquired = true;
        }
        if (killsAmount > 42 && !killsHat.acquired){
            if (!killsHatAch.IsAchieved){
                killsHatAch.Unlock();
                Achievements.StoreStats();
                SendAchievementUnlockText("Straftat Killer");
            }

            killsHat.acquired = true;
        }
    }

    [Space]
    [Header ("Stats")]
    public float tauntsAmount;
    public float killsAmount;
    public float deathsAmount;
    public float suicidesAmount;
    public float bodyshotsAmount;
    public float headshotsAmount;
    public float roundsWon;
    public float roundsLost;
    public float roundsPlayed;
    public float gamesWon;
    public float gamesLost;
    public float gamesPlayed;

    public float timeSpentInGame;
    public float timeSpentInAir;
    public float timeSpentOnGround;
    
    public float mapsUnlocked;

    [Header ("Challenges Stats")]
    public float rocketJumps;
    public float windowsBroken;
    public float ragdollsThrownAway;
    public float taserShots;
    public float propKills;
    public float potsBroken;
    public float noscope;

    [Header ("Texts")]

    [SerializeField] private TextMeshProUGUI tauntsAmountText;
    [SerializeField] private TextMeshProUGUI killsAmountText;
    [SerializeField] private TextMeshProUGUI deathsAmountText;
    [SerializeField] private TextMeshProUGUI suicidesAmountText;
    [SerializeField] private TextMeshProUGUI bodyshotsAmountText;
    [SerializeField] private TextMeshProUGUI headshotsAmountText;
    [SerializeField] private TextMeshProUGUI timeSpentInGameText;
    [SerializeField] private TextMeshProUGUI timeSpentInAirText;
    [SerializeField] private TextMeshProUGUI timeSpentOnGroundText;

    [SerializeField] private TextMeshProUGUI roundsWonText;
    [SerializeField] private TextMeshProUGUI roundsLostText;
    [SerializeField] private TextMeshProUGUI roundsPlayedText;
    [SerializeField] private TextMeshProUGUI gamesWonText;
    [SerializeField] private TextMeshProUGUI gamesLostText;
    [SerializeField] private TextMeshProUGUI gamesPlayedText;
    [SerializeField] private TextMeshProUGUI mapsUnlockedText;

    [Header ("Challenges Hats")]
    public CosmeticInstance rocketJumpsHat;
    public CosmeticInstance windowsBrokenHat;
    public CosmeticInstance headshotHat;
    public CosmeticInstance ragdollsThrownAwayHat;
    public CosmeticInstance taserShotsHat;
    public CosmeticInstance noscopeHat;
    public CosmeticInstance propKillsHat;
    public CosmeticInstance potsBrokenHat;
    public CosmeticInstance fiveGamesHat;
    public CosmeticInstance killsHat;
    public AchievementObject rocketJumpsHatAch;
    public AchievementObject windowsBrokenHatAch;
    public AchievementObject headshotHatAch;
    public AchievementObject ragdollsThrownAwayHatAch;
    public AchievementObject taserShotsHatAch;
    public AchievementObject noscopeHatAch;
    public AchievementObject propKillsHatAch;
    public AchievementObject potsBrokenHatAch;
    public AchievementObject fiveGamesHatAch;
    public AchievementObject killsHatAch;


    public void IncreaseRoundsWon(){ roundsWon ++;}
    public void IncreaseRoundsPlayed(){ roundsPlayed ++;}
    public void IncreaseGamesPlayed(){ gamesPlayed ++;}
    public void IncreaseGamesWon(){ gamesWon ++;}
    public void IncreaseMapsUnlocked(){ mapsUnlocked ++;}
    public void IncreaseTauntsAmount(){ tauntsAmount ++;}
    public void IncreaseKillsAmount(){ killsAmount ++;}
    public void IncreaseDeathsAmount(){ deathsAmount ++;}
    public void IncreaseSuicidesAmount(){ suicidesAmount ++;}
    public void IncreaseBodyshotsAmount(){ bodyshotsAmount ++;}
    public void IncreaseHeadshotsAmount(){ headshotsAmount ++;}
    public void IncreaseGamesLost(){ gamesLost ++;}
    public void IncreaseRoundsLost(){ roundsLost ++;}

    public object SaveState()
    {
        return new SaveData()
        {
            tauntsAmount = this.tauntsAmount,
            killsAmount = this.killsAmount,
            deathsAmount = this.deathsAmount,
            suicidesAmount = this.suicidesAmount,
            bodyshotsAmount = this.bodyshotsAmount,
            headshotsAmount = this.headshotsAmount,
            roundsWon = this.roundsWon,
            roundsLost = this.roundsLost,
            roundsPlayed = this.roundsPlayed,
            gamesWon = this.gamesWon,
            gamesLost = this.gamesLost,
            gamesPlayed = this.gamesPlayed,
            timeSpentInGame = this.timeSpentInGame,
            timeSpentInAir = this.timeSpentInAir,
            timeSpentOnGround = this.timeSpentOnGround,
            mapsUnlocked = this.mapsUnlocked,
            rocketJumps = this.rocketJumps,
            windowsBroken = this.windowsBroken,
            ragdollsThrownAway = this.ragdollsThrownAway,
            taserShots = this.taserShots,
            propKills = this.propKills,
            potsBroken = this.potsBroken,
            noscope = this.noscope,
            recentlyPlayedWithPlayers = this.recentlyPlayedWithPlayers
            
        };
    }
    public void LoadState(JContainer state)
    {
        SaveData saveData = state.ToObject<SaveData>();

        tauntsAmount = saveData.tauntsAmount;
        killsAmount = saveData.killsAmount;
        deathsAmount = saveData.deathsAmount;
        suicidesAmount = saveData.suicidesAmount;
        bodyshotsAmount = saveData.bodyshotsAmount;
        headshotsAmount = saveData.headshotsAmount;
        roundsWon = saveData.roundsWon;
        roundsLost = saveData.roundsLost;
        roundsPlayed = saveData.roundsPlayed;
        gamesWon = saveData.gamesWon;
        gamesLost = saveData.gamesLost;
        gamesPlayed = saveData.gamesPlayed;
        timeSpentInGame = saveData.timeSpentInGame;
        timeSpentInAir = saveData.timeSpentInAir;
        timeSpentOnGround = saveData.timeSpentOnGround;

        mapsUnlocked = saveData.mapsUnlocked;

        rocketJumps = saveData.rocketJumps;
        windowsBroken = saveData.windowsBroken;
        ragdollsThrownAway = saveData.ragdollsThrownAway;
        taserShots = saveData.taserShots;
        propKills = saveData.propKills;
        potsBroken = saveData.potsBroken;
        noscope = saveData.noscope;

        recentlyPlayedWithPlayers = saveData.recentlyPlayedWithPlayers ?? new List<string>();

        UpdateElo();
    }

    [Serializable]
    public struct SaveData
    {
        public float tauntsAmount;
        public float killsAmount;
        public float deathsAmount;
        public float suicidesAmount;
        public float bodyshotsAmount;
        public float headshotsAmount;
        public float roundsWon;
        public float roundsLost;
        public float roundsPlayed;
        public float gamesWon;
        public float gamesLost;
        public float gamesPlayed;
        public float timeSpentInGame;
        public float timeSpentInAir;
        public float timeSpentOnGround;
            
        public float mapsUnlocked;

        public float rocketJumps;
        public float windowsBroken;
        public float ragdollsThrownAway;
        public float taserShots;
        public float propKills;
        public float potsBroken;
        public float noscope;

        public List<string> recentlyPlayedWithPlayers;

    }




    public void ChangeTeammateOutline(string color) {
        PlayerPrefs.Save();
    }
    
    public void ChangeEnemyOutline(string color) {
        PlayerPrefs.Save();
    }
    
    public void SendAchievementUnlockText(string achievementName) {
        if (InstanceFinder.NetworkManager.IsOffline) {
            string personalName = SteamFriends.GetPersonaName();
            string message = $"<b>{personalName}</b> has unlocked: <color=#FFD700><b>{achievementName}</b></color>";
            PauseManager.Instance.WriteOfflineLog(message);
        }
        else {
            ClientInstance localClient = ClientInstance.Instance;
            string nameTag = localClient.PlayerNameTag;
            string message = $"<b>{nameTag}</b> has unlocked: <color=#FFD700><b>{achievementName}</b></color>";
            PauseManager.Instance.WriteLog(message);
        }
    }
}
