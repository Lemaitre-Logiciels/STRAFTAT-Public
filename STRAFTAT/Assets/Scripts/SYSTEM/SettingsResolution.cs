using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsResolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private List<Resolution> sortedResolutions;
    int highestRefreshRate = 0;
    
    void Start() {
        sortedResolutions = new List<Resolution>(Screen.resolutions);

        sortedResolutions.Sort((a, b) => {
            if (a.width != b.width) { return b.width.CompareTo(a.width); } // width first
            if (a.height != b.height) { return b.height.CompareTo(a.height); } // height
            return b.refreshRate.CompareTo(a.refreshRate); // refresh rate
        });
        
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (Resolution resolution in sortedResolutions) {
            string aspectRatio = GetAspectRatio(resolution.width, resolution.height);
            string resolutionOption = $"{resolution.width}x{resolution.height} ({aspectRatio}) {resolution.refreshRate:0.##} Hz";
            options.Add(resolutionOption);
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("resolution", 0));
    }
    
    public void SetResolution(int resolutionIndex) {
        if (sortedResolutions.Count == 0 || resolutionIndex < 0 || resolutionIndex >= sortedResolutions.Count) {
            Debug.LogWarning("Invalid resolution index: " + resolutionIndex);
            return;
        }
        
        Resolution resolution = sortedResolutions[resolutionIndex];
        
        Settings settings = Settings.Instance;
        if (settings.isFullscreen) {
            Screen.SetResolution(resolution.width, resolution.height, Settings.Instance.exclusiveFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.FullScreenWindow, resolution.refreshRate);
        } else {
            Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.Windowed);
        }

        Settings.Instance.ChangeResolution(resolutionIndex);
    }

    public void SetResolutionReal(TMP_Dropdown dropdown) { SetResolution(dropdown.value); }

    private string GetAspectRatio(int width, int height) {
        int gcd = GCD(width, height);
        return $"{width / gcd}:{height / gcd}";
    }

    private int GCD(int a, int b) {
        while (b != 0) {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}