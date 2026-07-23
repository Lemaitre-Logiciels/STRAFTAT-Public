using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetEffectVolume : MonoBehaviour
{
    private AudioSource source;
    private AudioSource audio;
    // Start is called before the first frame update
    void Awake()
    {
        source = SoundManager.Instance._effectsSource;
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (audio.volume != source.volume)
       	    audio.volume = source.volume;
    }
}
