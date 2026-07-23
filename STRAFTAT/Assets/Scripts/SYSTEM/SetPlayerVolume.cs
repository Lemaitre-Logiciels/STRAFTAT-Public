using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerVolume : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private AudioSource audio;
    // Start is called before the first frame update
    void Awake()
    {
        source = SoundManager.Instance._effectsSource;
    }

    // Update is called once per frame
    void Update()
    {
        if (audio.volume != source.volume)
       	    audio.volume = source.volume;
    }
}
