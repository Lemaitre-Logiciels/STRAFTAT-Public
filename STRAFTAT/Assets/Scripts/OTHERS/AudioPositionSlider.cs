using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioPositionSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] private SetMenuMusicVolume musicController;

    public Slider slider;
    private bool isDragging = false;

    void Awake() { slider = GetComponent<Slider>(); }
    void Update() {
        if (!musicController.audio.isPlaying || isDragging) { return; }
        slider.value = musicController.audio.time / musicController.audio.clip.length;
    }

    public void OnPointerDown(PointerEventData eventData){ isDragging = true; }

    public void OnPointerUp(PointerEventData eventData){
        musicController.SetAudioPosition(slider.value);
        isDragging = false;
    }
}
