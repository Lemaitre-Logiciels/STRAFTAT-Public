using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public PauseManager pauseManager;
    private Settings settings;

    [SerializeField] public AudioSource _musicSource, _effectsSource, _ambientSource;
    [SerializeField] private AudioLowPassFilter lowpassFilter;
    [SerializeField] private AudioHighPassFilter highpassFilter;
    [SerializeField] private float lowpass = 12000;
    [SerializeField] private float highpass = 14000;
    public bool SetLowpass;
    public bool SetHighpass;

    public float menuMusicVolume = 1;
    public float lerpSpeed = 1;

    void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }

        settings = GetComponent<Settings>();
    }

    void Start() 
    {
        pauseManager = PauseManager.Instance;
    }

    void Update()
    {
        //return if no filters set in inspector
        if (!highpassFilter || !lowpassFilter)
            return;


        if (!pauseManager.inMainMenu) 
        {
            highpassFilter.cutoffFrequency = 10;
            lowpassFilter.cutoffFrequency = 22000;
        }
        else if (SetLowpass) {
            highpassFilter.cutoffFrequency = Mathf.Lerp(highpassFilter.cutoffFrequency, 10, lerpSpeed * Time.deltaTime);
            lowpassFilter.cutoffFrequency = Mathf.Lerp(lowpassFilter.cutoffFrequency, lowpass, lerpSpeed * Time.deltaTime);
        }
        else if (SetHighpass) {
            highpassFilter.cutoffFrequency = Mathf.Lerp(highpassFilter.cutoffFrequency, highpass, lerpSpeed * Time.deltaTime);
            lowpassFilter.cutoffFrequency = Mathf.Lerp(lowpassFilter.cutoffFrequency, 22000, lerpSpeed * Time.deltaTime);
        }
        else 
        {
            highpassFilter.cutoffFrequency = Mathf.Lerp(highpassFilter.cutoffFrequency, 10, lerpSpeed*0.05f * Time.deltaTime);
            lowpassFilter.cutoffFrequency = Mathf.Lerp(lowpassFilter.cutoffFrequency, 22000, lerpSpeed *0.05f* Time.deltaTime);
        }
    }

    public void PlaySound(AudioClip clip){
        _effectsSource.pitch = 1;
        _effectsSource.PlayOneShot(clip);
    }

    public void PlaySoundWithPitch(AudioClip clip, float pitch){
        _effectsSource.pitch = pitch;
        _effectsSource.PlayOneShot(clip);
    }

    public void PlayAmbientSound(AudioClip clip){
        _ambientSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip, int loop){
        _musicSource.PlayOneShot(clip);
        if (loop == 1) _musicSource.loop = true;
        if (loop == 0) _musicSource.loop = false;
    }

    public void StopAllMusic()
    {
        _musicSource.Stop();
    }

    public void StopSound()
    {
        _effectsSource.Stop();
    }

    public void ChangeMasterVolume(float value){
        AudioListener.volume = value;
    }
    public void ChangeMusicVolume(float value){
        _musicSource.volume = value;
    }
    public void ChangeEffectVolume(float value){
        _effectsSource.volume = value;
    }
    public void ChangeAmbientVolume(float value){
        _ambientSource.volume = value;
    }


    public void ChangeMasterVolumeSlider(Slider value){
        AudioListener.volume = value.value;
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
    }
    public void ChangeMusicVolumeSlider(Slider value){
        _musicSource.volume = value.value;
        PlayerPrefs.SetFloat("musicVolume", _musicSource.volume);
    }
    public void ChangeEffectVolumeSlider(Slider value){
        _effectsSource.volume = value.value;
        PlayerPrefs.SetFloat("effectsVolume", _effectsSource.volume);
    }
    public void ChangeAmbientVolumeSlider(Slider value){
        _ambientSource.volume = value.value;
        PlayerPrefs.SetFloat("ambientVolume", _ambientSource.volume);
    }
    public void ChangeMenuMusicVolumeSlider(Slider value){
        menuMusicVolume = value.value;
        PlayerPrefs.SetFloat("menuMusicVolume", menuMusicVolume);
    }

    public void ToggleEffects(){
        _effectsSource.mute = !_effectsSource.mute;
    }

    public void ToggleMusic(){
        _musicSource.mute = !_musicSource.mute;
    }
}
