using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class DisplayLoadValues : MonoBehaviour
{
    [SerializeField] private bool toggleTwoAxis = false;
    [SerializeField] private bool invertMouseX = false;
    [SerializeField] private bool invertMouseY = false;
    [SerializeField] private bool sprintToggle = false;
    [SerializeField] private bool aimToggle = false;
    [SerializeField] private bool leanToggle = false;
    [SerializeField] private bool crouchToggle = false;
    [SerializeField] private bool enableVoiceChat = false;
    [SerializeField] private bool voiceChatVolume = false;
    [SerializeField] private bool reverseSprintBind = false;
    [SerializeField] private bool inverseFireBinding = false;

    [SerializeField] private bool mouseSensitivity;
    [SerializeField] private bool mouseAimScopeSensitivity;
    [SerializeField] private bool mouseAimSensitivity;
    [SerializeField] private bool mouseXSensitivity;
    [SerializeField] private bool mouseYSensitivity;
    [SerializeField] private bool fovValue;
    [SerializeField] private bool brightness;
    [SerializeField] private bool damageIntensity;

    [SerializeField] private bool effectsVolume;
    [SerializeField] private bool ambientVolume;
    [SerializeField] private bool musicVolume;
    [SerializeField] private bool masterVolume;
    [SerializeField] private bool menuMusicVolume;

    [SerializeField] private bool targetFps;
    [SerializeField] private bool targetFpsToggle;
    [SerializeField] private bool useVsync;
    [SerializeField] private bool graphics;
    [SerializeField] private bool isFullscreen;
    [SerializeField] private bool minimalistUi;
    [SerializeField] private bool motionBlur;
    [FormerlySerializedAs("pushToTalk")] [SerializeField] private bool voiceChatMode;
    [SerializeField] private bool exclusiveFullscreen;

    [SerializeField] private bool enableFixedCrosshair;
    [SerializeField] private bool showSpeedometer;
    [SerializeField] private bool disableCrosshair;
    [SerializeField] private bool inGameMusic;
    [SerializeField] private bool reduceVFX;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadValues());
    }
    
    IEnumerator LoadValues()
    {
        yield return new WaitForSeconds(0.1f);
        if (toggleTwoAxis) GetComponent<Toggle>().isOn = Settings.Instance.toggleTwoAxis;
        if (invertMouseX) GetComponent<Toggle>().isOn = Settings.Instance.invertMouseX;
        if (invertMouseY) GetComponent<Toggle>().isOn = Settings.Instance.invertMouseY;
        if (sprintToggle) GetComponent<Toggle>().isOn = Settings.Instance.sprintToggle;
        if (aimToggle) GetComponent<Toggle>().isOn = Settings.Instance.aimToggle;
        if (leanToggle) GetComponent<Toggle>().isOn = Settings.Instance.leanToggle;
        if (crouchToggle) GetComponent<Toggle>().isOn = Settings.Instance.crouchToggle;
        if (reverseSprintBind) GetComponent<Toggle>().isOn = Settings.Instance.reverseSprintBind;
        if (enableVoiceChat) GetComponent<Toggle>().isOn = Settings.Instance.enableVoiceChat;
        if (isFullscreen) GetComponent<Toggle>().isOn = Settings.Instance.isFullscreen;
        if (minimalistUi) GetComponent<Toggle>().isOn = Settings.Instance.minimalistUi;
        if (motionBlur) GetComponent<Toggle>().isOn = Settings.Instance.motionBlur;
        if (inverseFireBinding) GetComponent<Toggle>().isOn = Settings.Instance.inverseFireBinding;
        if (voiceChatMode) GetComponent<TMP_Dropdown>().value = (int)Settings.Instance.voiceChatMode;
        if (inGameMusic) GetComponent<Toggle>().isOn = Settings.Instance.inGameMusic;
        if (enableFixedCrosshair) GetComponent<Toggle>().isOn = Settings.Instance.enableFixedCrosshair;
        if (showSpeedometer) GetComponent<Toggle>().isOn = Settings.Instance.showSpeedometer;
        if (disableCrosshair) GetComponent<Toggle>().isOn = Settings.Instance.disableCrosshair;
        if (exclusiveFullscreen) GetComponent<Toggle>().isOn = Settings.Instance.exclusiveFullscreen;

        if (mouseSensitivity) GetComponent<Slider>().value = PlayerPrefs.GetFloat("mouseSensitivity");
        if (mouseAimScopeSensitivity) GetComponent<Slider>().value = PlayerPrefs.GetFloat("mouseAimScopeSensitivity");
        if (mouseAimSensitivity) GetComponent<Slider>().value = PlayerPrefs.GetFloat("mouseAimSensitivity");
        if (mouseXSensitivity) GetComponent<Slider>().value = PlayerPrefs.GetFloat("mouseXSensitivity");
        if (mouseYSensitivity) GetComponent<Slider>().value = PlayerPrefs.GetFloat("mouseYSensitivity");
        if (fovValue) GetComponent<Slider>().value = PlayerPrefs.GetFloat("fovValue");
        if (brightness) GetComponent<Slider>().value = PlayerPrefs.GetFloat("brightness");
        if (damageIntensity) GetComponent<Slider>().value = PlayerPrefs.GetFloat("damageIntensity");
        if (voiceChatVolume) GetComponent<Slider>().value = PlayerPrefs.GetFloat("voiceChatVolume");


        if (effectsVolume) GetComponent<Slider>().value = PlayerPrefs.GetFloat("effectsVolume");
        if (musicVolume) GetComponent<Slider>().value = PlayerPrefs.GetFloat("musicVolume");
        if (masterVolume) GetComponent<Slider>().value = PlayerPrefs.GetFloat("masterVolume");
        if (menuMusicVolume) GetComponent<Slider>().value = PlayerPrefs.GetFloat("menuMusicVolume");
        if (ambientVolume) GetComponent<Slider>().value = PlayerPrefs.GetFloat("ambientVolume");

        if (targetFps) {
            int targetFpsIndex = PlayerPrefs.GetInt("targetFps");
            if (targetFpsIndex < 30) { targetFpsIndex = 30; }
            GetComponent<TMP_InputField>().text = targetFpsIndex.ToString();
        }
        if (targetFpsToggle) GetComponent<Toggle>().isOn = Settings.Instance.targetFpsToggle;
        
        if (graphics) GetComponent<TMP_Dropdown>().value = PlayerPrefs.GetInt("qualitySetting");
        if (useVsync) GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("useVsync") == 1 ? true : false;
        if (reduceVFX) GetComponent<Toggle>().isOn = Settings.Instance.reduceVFX;
    }

    
}
