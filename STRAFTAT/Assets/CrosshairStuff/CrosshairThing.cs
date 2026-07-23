using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100000)]
public class CrosshairThing : MonoBehaviour {
    [SerializeField] Crosshair crosshair;
    [SerializeField] ColorInputSetting tintColorSettings;
    [SerializeField] TMP_InputField sizeSlider;
    [SerializeField] Toggle disableFilteringToggle;
    
    private Color _currentColor = Color.white;
    public void TintColor(Color color) {
        _currentColor = color;
        crosshair.image.color = _currentColor;
        
        string hexColor = ColorUtility.ToHtmlStringRGBA(color);
        PlayerPrefs.SetString("CrosshairTintColor", hexColor);
        PlayerPrefs.Save();
    }
    
    private float _currentSize = 1f;
    public void SizeSlider(string sizeText) {
        if (!float.TryParse(sizeText, out float value)) {
            sizeSlider.SetTextWithoutNotify(_currentSize.ToString("0.000"));
            return;
        }
        value = Mathf.Clamp(value, float.Epsilon, 100f);
        sizeSlider.SetTextWithoutNotify(value.ToString("0.000"));
        
        _currentSize = value;
        crosshair.image.rectTransform.localScale = Vector3.one * _currentSize;
        
        PlayerPrefs.SetFloat("CrosshairSize", value);
        PlayerPrefs.Save();
    }

    private void NonImageThings() {
        if (PlayerPrefs.HasKey("CrosshairTintColor")) {
            string hexColor = PlayerPrefs.GetString("CrosshairTintColor");
            if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color)) {
                _currentColor = color;
            }
        }
        
        if (PlayerPrefs.HasKey("CrosshairSize")) {
            float size = PlayerPrefs.GetFloat("CrosshairSize");
            _currentSize = Mathf.Clamp(size, float.Epsilon, 100f);
        }
        
        if (PlayerPrefs.HasKey("CrosshairFilteringDisabled")) {
            _filteringDisabled = PlayerPrefs.GetInt("CrosshairFilteringDisabled") == 1;
        }

        crosshair.image.rectTransform.localScale = Vector3.one * _currentSize;
        sizeSlider.SetTextWithoutNotify(_currentSize.ToString("0.000"));
        crosshair.image.color = _currentColor;
        tintColorSettings.color = _currentColor;
        disableFilteringToggle.isOn = _filteringDisabled;
    }
    
    public void Awake() {
        crosshair.fixedCrosshair = crosshair.defaultFixedCrosshair;
        
        NonImageThings();
        
        if (PlayerPrefs.HasKey("CurrentCrosshairPath")) {
            string path = PlayerPrefs.GetString("CurrentCrosshairPath");
            _currentCrosshairPath = path;
        }
        
        LoadCrosshairPaths();
        SetCrosshair();
    }
    
    [SerializeField] Image previewImage;
    public static readonly string CrosshairFolderPath = Application.streamingAssetsPath + "/Crosshairs";
    public void OpenFolder() {
        if (!Directory.Exists(CrosshairFolderPath)) { Directory.CreateDirectory(CrosshairFolderPath); }
        
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Process.Start(CrosshairFolderPath);
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", CrosshairFolderPath);
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Process.Start("open", CrosshairFolderPath);
        #endif
    }

    private string[] _supportedExtensions = { ".png", ".jpg", ".jpeg" };
    private List<string> _crosshairPaths = new List<string>();
    private string _currentCrosshairPath = "";
    private int _currentCrosshairIndex = 0;
    public void LoadCrosshairPaths() {
        if (!Directory.Exists(CrosshairFolderPath)) { Directory.CreateDirectory(CrosshairFolderPath); }
        string[] files = Directory.GetFiles(CrosshairFolderPath);
        _crosshairPaths.Clear();
        _crosshairPaths.Add(""); // Default crosshair option
        
        string oldPath = _currentCrosshairPath;
        _currentCrosshairPath = "";
        _currentCrosshairIndex = 0;
        
        foreach (string file in files) {
            if (!_supportedExtensions.Contains(Path.GetExtension(file).ToLower())) { continue; }

            _crosshairPaths.Add(file);
            
            if (oldPath == file) {
                _currentCrosshairIndex = _crosshairPaths.Count - 1;
                _currentCrosshairPath = file;
            }
        }
    }
    
    public void UpArrow() {
        _currentCrosshairIndex++;
        if (_currentCrosshairIndex >= _crosshairPaths.Count) { _currentCrosshairIndex = 0; }
        SetCrosshair();
    }

    public void DownArrow() {
        _currentCrosshairIndex--;
        if (_currentCrosshairIndex < 0) { _currentCrosshairIndex = _crosshairPaths.Count - 1; }
        SetCrosshair();
    }

    public void SetCrosshair() {
        string path = _crosshairPaths[_currentCrosshairIndex];
        if (string.IsNullOrEmpty(path)) {
            LoadDefaultCrosshair();
            return;
        }
        if (!LoadNewCrosshair(path, out Sprite sprite)) {
            PauseManager.Instance.WriteOfflineLog($"Failed to load crosshair at path: {path}");
            LoadDefaultCrosshair();
            return;
        }
        
        crosshair.fixedCrosshair = sprite;
        crosshair.fixedCrosshairSize = sprite.rect.size;
        previewImage.sprite = sprite;
        _currentCrosshairPath = path;
        
        PlayerPrefs.SetString("CurrentCrosshairPath", path);
        PlayerPrefs.Save();
    }
    
    public bool LoadNewCrosshair(string path, out Sprite sprite) {
        if (!File.Exists(path)) { sprite = null; return false; }
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(1, 1);
        texture.filterMode = _filteringDisabled ? FilterMode.Point : FilterMode.Bilinear;
        if (!texture.LoadImage(fileData)) { sprite = null; return false; }
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        return true;
    }
    
    public void LoadDefaultCrosshair() {
        crosshair.fixedCrosshair = crosshair.defaultFixedCrosshair;
        crosshair.fixedCrosshairSize = new Vector2(400, 400);
        previewImage.sprite = crosshair.defaultFixedCrosshair;
        _currentCrosshairPath = "";
        _currentCrosshairIndex = 0;
        
        PlayerPrefs.SetString("CurrentCrosshairPath", "");
        PlayerPrefs.Save();
    }
    
    bool _filteringDisabled = false;
    public void DisableFiltering(bool disable) {
        FilterMode mode = disable ? FilterMode.Point : FilterMode.Bilinear;
        crosshair.invisibleCrosshair.texture.filterMode = mode;
        crosshair.nogunCrosshair.texture.filterMode = mode;
        crosshair.standCrosshair.texture.filterMode = mode;
        crosshair.sprintCrosshair.texture.filterMode = mode;
        crosshair.defaultFixedCrosshair.texture.filterMode = mode;
        if (crosshair.fixedCrosshair) { crosshair.fixedCrosshair.texture.filterMode = mode; }

        _filteringDisabled = disable;
        PlayerPrefs.SetInt("CrosshairFilteringDisabled", disable ? 1 : 0);
        PlayerPrefs.Save();
    }
}
