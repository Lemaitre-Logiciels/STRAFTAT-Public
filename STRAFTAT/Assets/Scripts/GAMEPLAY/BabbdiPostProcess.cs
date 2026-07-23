using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class BabbdiPostProcess : MonoBehaviour
{
    [SerializeField] private ColorfulFog colorfulFog;
    [SerializeField] private PostProcessVolume volume;
    [SerializeField] private PostProcessVolume weaponCam;
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Babbdi") return;

        colorfulFog.enabled = true;
        weaponCam.enabled = false;
        volume.enabled = false;
    }

}
