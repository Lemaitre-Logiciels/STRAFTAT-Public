using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;

public class XpPopupInstance : MonoBehaviour
{
    public int index;
    public int mapIndex;
    public bool clicked;

    [SerializeField] private RawImage img;
    [SerializeField] private TextMeshProUGUI text; 
    [SerializeField] private TextMeshProUGUI challengeText; 
    [SerializeField] private TextMeshProUGUI titleText; 
    [SerializeField] private TextMeshProUGUI dlcText;
    [SerializeField] private GameObject spawnVfx;
    [SerializeField] private AudioClip spawnSfx;
    GameObject tempVfx;

    void Start()
    {
        SoundManager.Instance.PlaySound(spawnSfx);
        tempVfx = Instantiate(spawnVfx, GameObject.Find("XpPopupVfxPos").transform.position, Quaternion.identity);

        if (ProgressManager.Instance.instances[index].dlcExlusive) dlcText.text = "DLC";

        if (ProgressManager.Instance.instances[index].cosmetic != null)
        {
            titleText.text = "New Hat";
            img.texture = ProgressManager.Instance.instances[index].cosmetic.sprite.texture;
            text.text = ProgressManager.Instance.instances[index].cosmetic.cosmeticName;
            challengeText.text = (ProgressManager.Instance.instances[index].cosmetic.unlockWithXp ? "Gain " + ProgressManager.Instance.instances[index].xpToUnlock + " xp" : ProgressManager.Instance.instances[index].cosmetic.challengeDescription);
        }
        else
        {
            titleText.text = "New Map";
            
            //string mapsNames = "";
            /*for (int i=0; i < ProgressManager.Instance.instances[index].maps.Length; i++)
            {
                mapsNames = mapsNames + ProgressManager.Instance.instances[index].maps[i] + "/n";
            }*/
            text.text = ProgressManager.Instance.instances[index].maps[mapIndex];
            challengeText.text = "Gain " + ProgressManager.Instance.instances[index].xpToUnlock + " xp";
            #if UNITY_EDITOR
            if (Application.isEditor) img.texture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Resources/MapSprites/" + text.text + ".png", typeof(Texture2D));
            #endif
            if (!Application.isEditor) img.texture = (Texture2D)Resources.Load("MapSprites/" + text.text, typeof(Texture2D));
        }
    }

    void OnDisable()
    {
        if (tempVfx != null) Destroy(tempVfx);
    }

    public void PassPopup()
    {
        clicked = true;
    }

    public void SkipAll() {
        ProgressManager.Instance.skipAll = true;
    }

}
