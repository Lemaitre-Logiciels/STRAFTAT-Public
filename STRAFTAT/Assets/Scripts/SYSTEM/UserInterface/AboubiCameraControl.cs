using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboubiCameraControl : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private PauseManager pauseManager;
    // Start is called before the first frame update
    void Start()
    {
        pauseManager = PauseManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.enabled != (pauseManager.inMainMenu || pauseManager.tabScreen.activeSelf)) cam.enabled = pauseManager.inMainMenu || pauseManager.tabScreen.activeSelf;
    }
}
