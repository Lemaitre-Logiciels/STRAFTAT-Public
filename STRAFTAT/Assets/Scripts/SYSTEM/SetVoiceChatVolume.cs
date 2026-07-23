using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVoiceChatVolume : MonoBehaviour
{
    private Settings settings;
    private AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        settings = Settings.Instance;
        if (GetComponent<AudioSource>() != null) source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (source.volume != settings.voiceChatVolume)
            source.volume = settings.voiceChatVolume;
    }
}
