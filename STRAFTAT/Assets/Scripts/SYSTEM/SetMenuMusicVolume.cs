using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public struct MusicTrack {
    public readonly string AudioPath;
    public readonly string TrackName;
    public readonly string ArtistName;
    public readonly int TrackNumber;
    public readonly AudioType AudioType;

    public MusicTrack(string audioPath, string trackName, string artistName, int trackNumber, AudioType audioType) {
        AudioPath = audioPath;
        AudioType = audioType;
        ArtistName = artistName;
        TrackNumber = trackNumber;
        TrackName = trackName;
    }
}

public class SetMenuMusicVolume : MonoBehaviour {
    private string MusicFolderPath = Path.Combine(Application.streamingAssetsPath, "Music");
    
    public static readonly Dictionary<string, AudioType> ExtensionToAudioType = new Dictionary<string, AudioType> { 
        { ".mp3", AudioType.MPEG },
        { ".ogg", AudioType.OGGVORBIS },
        { ".wav", AudioType.WAV },
        { ".aiff", AudioType.AIFF },
        { ".aif", AudioType.AIFF },
    }; 
    
    [HideInInspector] public List<MusicTrack> MusicTracks = new List<MusicTrack>();
    private readonly Dictionary<int, int> TrackNumberToIndex = new Dictionary<int, int>();
    private bool loadingMusic = true;
    private bool errorLoadingMusic = false;
    
    [SerializeField] private TextMeshProUGUI trackText;
    [SerializeField] private TextMeshProUGUI trackTextInPlayer;
    [SerializeField] private TextMeshProUGUI playbackTimeText;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject playButton;
    [SerializeField] private AudioPositionSlider audioPositionSlider;
    [Space]
    [SerializeField] private Transform musicPlayerTransform;
    [SerializeField] private Transform optionsPosition;
    [SerializeField] private Transform playMenuPosition;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject mapsPanel;
    [Space]
    [HideInInspector] public AudioSource audio;

    private Settings settings;
    private SoundManager soundManager;
    private AudioListener customListener;

    public int currentTrackIndex;

    public static SetMenuMusicVolume Instance { get; private set; }
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        audio = GetComponent<AudioSource>();
        audio.loop = false;
        
        customListener = GetComponent<AudioListener>();

        GetAllMusic();
        
        audio.spatialBlend = 0f;
        audio.reverbZoneMix = 0f;
        audio.minDistance = 5;
        audio.maxDistance = 5;
    }

    private void GetAllMusic() {
        trackTextInPlayer.text = "loading";
        trackText.text = $"loading";
        playbackTimeText.text = $"";
        audioPositionSlider.slider.value = 0;
        
        if (!Directory.Exists(MusicFolderPath)) { Directory.CreateDirectory(MusicFolderPath); } // let it fail the normal way if somebody removed the folder
        
        string[] files = Directory.GetFiles(MusicFolderPath).OrderBy(f => f).ToArray();
        foreach (string file in files) {
            string fileExtension = Path.GetExtension(file).ToLower();
            if (!ExtensionToAudioType.TryGetValue(fileExtension, out AudioType type)) { continue; }
            
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

            string[] parts = fileNameWithoutExtension.Split('-', 3);

            if (parts.Length != 3) { continue; }
            
            string trackNumber = parts[0].Trim();
            if (!int.TryParse(trackNumber, out int trackNumberInt)) { continue; }
            
            string trackName = parts[1].Trim();
            string author = parts[2].Trim();

            string fileUri = new Uri(file).AbsoluteUri;
            MusicTracks.Add(new MusicTrack(fileUri, trackName, author, trackNumberInt, type));
        }

        if (MusicTracks.Count == 0) {
            Debug.Log($"No Music was found ):");
            
            trackTextInPlayer.text = "No Music Found D:";
            trackText.text = $"No Music Found D:";
            playbackTimeText.text = $"";
            audioPositionSlider.slider.value = 0;
            
            return;
        }
        
        Shuffle();
        
        StartPlayingMusic();
    }

    void Start() {
        settings = Settings.Instance;
        soundManager = SoundManager.Instance;
    }

    void StartPlayingMusic() {
        int savedTrackNumber = PlayerPrefs.GetInt("SavedTrackNumber", 0);
        currentTrackIndex = TrackNumberToIndex.GetValueOrDefault(savedTrackNumber, 0);
        float savedTrackPosition = PlayerPrefs.GetFloat("SavedTrackPosition", 0f);
        bool savedTrackPaused = PlayerPrefs.GetInt("SavedTrackPaused", 0) == 1;

        StartCoroutine(LoadTrack(currentTrackIndex, savedTrackPosition, savedTrackPaused));
    }
    
    private void OnApplicationQuit() {
        SteamLobby.Instance.LeaveLobby(); // idk why this is here but Im not gonna remove it in fear of breaking the house of cards.

        if (loadingMusic) { return; }

        PlayerPrefs.SetInt("SavedTrackNumber", MusicTracks[currentTrackIndex].TrackNumber);
        PlayerPrefs.SetFloat("SavedTrackPosition", audio.time);
        PlayerPrefs.SetInt("SavedTrackPaused", audio.isPlaying ? 0 : 1);
        PlayerPrefs.Save();
    }

    bool loadingMainMenu;

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (loadingMusic) { return; }
        
        if (scene.name == "MainMenu" && !audio.isPlaying && pauseButton.activeSelf) { audio.Play(); loadingMainMenu = true; }
    }

    void Update() {
        if (PauseManager.Instance.inMainMenu) {
            musicPlayerTransform.position = (optionsMenu.activeSelf || mapsPanel.transform.localScale!=Vector3.zero ? optionsPosition.position : playMenuPosition.position);
            musicPlayerTransform.localScale = (optionsMenu.activeSelf || mapsPanel.transform.localScale!=Vector3.zero ? Vector3.one : new Vector3(0.75f, 0.75f, 0.75f));
            musicPlayerTransform.SetParent((optionsMenu.activeSelf || mapsPanel.transform.localScale!=Vector3.zero ? optionsPosition : playMenuPosition));
        }
        else {
            if (musicPlayerTransform.position != optionsPosition.position) musicPlayerTransform.position = optionsPosition.position;
            if (musicPlayerTransform.localScale != Vector3.one) musicPlayerTransform.localScale = Vector3.one;
            if (musicPlayerTransform.parent != optionsPosition) musicPlayerTransform.SetParent(optionsPosition);
        }

        UpdateMusic();
    }

    private void UpdateMusic() {
        if (loadingMusic) { return; }
        
        if (SceneManager.GetActiveScene().name == "MainMenu") loadingMainMenu = false;
        if (loadingMainMenu && !settings.inGameMusic) { return; }

        if (audio.volume != soundManager.menuMusicVolume) { audio.volume = soundManager.menuMusicVolume; }

        audio.mute = (SceneManager.GetActiveScene().name == "VictoryScene" || SceneManager.GetActiveScene().name == "EndGame");
        customListener.enabled = SceneManager.GetActiveScene().name == "MovedObjectsHolder";

        if (SceneManager.GetActiveScene().name != "MainMenu" && !settings.inGameMusic && audio.isPlaying) { audio.Stop(); }

        if (!audio.isPlaying) { return; }
        
        if (audio.time >= audio.clip.length - 0.1f) { NextTrack(); }
        playbackTimeText.text = $"{(int)(audio.time / 60)}:{(audio.time % 60 < 10 ? "0" : "")}{(int)(audio.time % 60)} / {(int)(audio.clip.length / 60)}:{(audio.clip.length % 60 < 10 ? "0" : "")}{(int)(audio.clip.length % 60)}";
    }

    public void NextTrack() {
        if (loadingMusic && !errorLoadingMusic) { return; }
        
        if (SceneManager.GetActiveScene().name != "MainMenu" && !settings.inGameMusic) return;
        
        currentTrackIndex = (currentTrackIndex+1 >= MusicTracks.Count ? 0 : currentTrackIndex+1);
        StartCoroutine(LoadTrack(currentTrackIndex, 0, false));

        pauseButton.SetActive(true);
        playButton.SetActive(false);
    }

    public void PreviousTrack() {
        if (loadingMusic && !errorLoadingMusic) { return; }

        if (SceneManager.GetActiveScene().name != "MainMenu" && !settings.inGameMusic) return;
        
        currentTrackIndex = (currentTrackIndex-1 < 0 ? MusicTracks.Count-1 : currentTrackIndex-1);
        StartCoroutine(LoadTrack(currentTrackIndex, 0, false));
        
        pauseButton.SetActive(true);
        playButton.SetActive(false);
    }

    public void Pause() {
        if (loadingMusic) { return; }
        
        audio.Pause();
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

    public void Play() {
        if (loadingMusic) { return; } // Until we've loaded the shit don't do anything
        
        if (SceneManager.GetActiveScene().name != "MainMenu" && !settings.inGameMusic) { return; }
        audio.Play();
        pauseButton.SetActive(true);
        playButton.SetActive(false);
    }

    public void SetAudioPosition(float value) {
        if (loadingMusic) { return; }
        audio.time = Mathf.Lerp(0, audio.clip.length, value);
    }

    public void Shuffle() {
        for (int i = 0; i < MusicTracks.Count; i++) {
            int randomIndex = Random.Range(i, MusicTracks.Count);
            MusicTrack temp = MusicTracks[i];
            MusicTracks[i] = MusicTracks[randomIndex];
            MusicTracks[randomIndex] = temp;
            
            // Somebody could define something with the same track number but that only makes it save the wrong thing so I don't care
            TrackNumberToIndex[MusicTracks[i].TrackNumber] = i;
        }
    }
    
    private IEnumerator LoadTrack(int trackIndex, float position, bool paused) {
        loadingMusic = true;
        errorLoadingMusic = false;
        
        MusicTrack musicTrack = MusicTracks[trackIndex];
        
        trackTextInPlayer.text = "loading";
        trackText.text = $"loading : {musicTrack.TrackName} - {musicTrack.ArtistName}";
        playbackTimeText.text = $"";
        audioPositionSlider.slider.value = 0;
        
        audio.Stop();
        
        using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(musicTrack.AudioPath, musicTrack.AudioType);
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result != UnityWebRequest.Result.Success) {
            Debug.Log($"Failed to load music: {musicTrack.AudioPath}");
            Debug.LogError($"Error: {webRequest.error}");
            trackTextInPlayer.text = $"Unable to load {musicTrack.TrackName} - {musicTrack.ArtistName}";
            trackText.text = $"Unable to load {musicTrack.TrackName} - {musicTrack.ArtistName}";
            playbackTimeText.text = $"";
            audioPositionSlider.slider.value = 0;
            errorLoadingMusic = true;
            yield break;
        }
        
        using DownloadHandlerAudioClip webRequestAudio = (DownloadHandlerAudioClip)webRequest.downloadHandler;
        webRequestAudio.streamAudio = true;

        // The example I found for DownloadHandlerAudioClip checked it again after so
        if (webRequest.result != UnityWebRequest.Result.Success) {
            Debug.Log($"Failed to load music: {musicTrack.AudioPath}");
            trackTextInPlayer.text = $"Unable to load {musicTrack.TrackName} - {musicTrack.ArtistName}";
            trackText.text = $"Unable to load {musicTrack.TrackName} - {musicTrack.ArtistName}";
            playbackTimeText.text = $"";
            audioPositionSlider.slider.value = 0;
            errorLoadingMusic = true;
            yield break;
        }

        AudioClip result = webRequestAudio.audioClip;
        result.name = musicTrack.TrackName;
        
        audio.clip = result;
        audio.time = position;
        if (paused) { audio.Pause(); }
        else { audio.Play(); }
        pauseButton.SetActive(!paused);
        playButton.SetActive(paused);
        
        trackTextInPlayer.text = $"{musicTrack.TrackName} - {musicTrack.ArtistName}";
        trackText.text = $"music playing : {musicTrack.TrackName} - {musicTrack.ArtistName}";
        playbackTimeText.text = (int)(position/60) + ":" + (position%60 < 10 ? "0" : "") + (int)(position%60) + " / " + (int)(audio.clip.length/60) + ":" + (audio.clip.length%60 < 10 ? "0" : "") + (int)(audio.clip.length%60);
        audioPositionSlider.slider.value = position / audio.clip.length;

        loadingMusic = false;
    }

    public void OpenMusicFolder() {
        if (!Directory.Exists(MusicFolderPath)) { Directory.CreateDirectory(MusicFolderPath); }
        
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Process.Start(MusicFolderPath);
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", MusicFolderPath);
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Process.Start("open", MusicFolderPath);
        #endif
    }
}
