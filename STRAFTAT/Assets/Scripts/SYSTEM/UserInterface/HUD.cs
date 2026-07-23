using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Component.Utility;
using FishNet;
using FishNet.Managing;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    private PauseManager pauseManager;
    private TMP_Text text;
    [SerializeField] private bool ammoRight;
    [SerializeField] private bool ammoReloadRight;
    [SerializeField] private bool ammoLeft;
    [SerializeField] private bool ammoReloadLeft;
    [SerializeField] private bool pingDisplay;
    [SerializeField] private bool fpsDisplay;
    PingDisplay PingDisplay;

    private IEnumerator Start()
    {
        if (!fpsDisplay) yield break; 

        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Awake()
    {
        pauseManager = PauseManager.Instance;
        text = GetComponent<TMP_Text>();
        PingDisplay = InstanceFinder.NetworkManager.GetComponent<PingDisplay>();
    }

    // Update is called once per frame
    void Update()
    {

        if (ammoRight) {
            

            if (text.text != pauseManager.rightGunAmmo.text) {
                transform.DOKill();
                transform.parent.localScale = Vector3.one;
                transform.parent.DOPunchScale(new Vector3(1.15f,1.15f,1.15f), 0.1f);
            }
            else transform.parent.DOScale(Vector3.one, 0.3f);
            text.text = pauseManager.rightGunAmmo.text;
        }
        if (ammoLeft) {
            

            if (text.text != pauseManager.leftGunAmmo.text) {
                transform.DOKill();
                transform.parent.localScale = Vector3.one;
                transform.parent.DOPunchScale(new Vector3(1.15f,1.15f,1.15f), 0.1f);
            }
            else transform.parent.DOScale(Vector3.one, 0.3f);
            text.text = pauseManager.leftGunAmmo.text;
        }
        if (ammoReloadRight) { 
            
            text.text = pauseManager.rightGunAmmoReload.text;
        }
        if (ammoReloadLeft) {
            
            text.text = pauseManager.leftGunAmmoReload.text;
        }
        if (pingDisplay) {
            text.text = PingDisplay.ping.ToString();
            
        }
        if (fpsDisplay) {
            text.text = Mathf.Round(count).ToString();
            
        }
    }

    private float count;
    public bool display;

}
