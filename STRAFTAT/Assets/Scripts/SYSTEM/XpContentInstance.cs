using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class XpContentInstance : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public int index;
    public int mapIndex;

    [SerializeField] private RawImage img; 
    [SerializeField] private RawImage frame; 
    [SerializeField] private Color unlockedColor; 
    [SerializeField] private Color DlcunlockedColor; 
    [SerializeField] private Color lockedColor; 
    [SerializeField] private Color DlclockedColor; 
    [SerializeField] private Color DlcFrameColor; 
    [SerializeField] private Color DlcLockedTextColor; 
    [SerializeField] private Color LockedTextColor; 
    [SerializeField] private Color FrameColor; 
    [SerializeField] private TextMeshProUGUI text; 
    [SerializeField] private TextMeshProUGUI lockedText; 
    XpContentLayout contentLayout;

    void Start()
    {
        contentLayout = GetComponentInParent<XpContentLayout>();
        frame.color = (ProgressManager.Instance.instances[index].dlcExlusive ? DlcFrameColor : FrameColor);
        lockedText.color = (ProgressManager.Instance.instances[index].dlcExlusive ? DlcLockedTextColor : LockedTextColor);
        ProgressManager.Instance.xpContentInstances.Add(this);
        UpdateUI();
        if (ProgressManager.Instance.instances[index].cosmetic != null)
        {
            if (ProgressManager.Instance.instances[index].cosmetic.sprite) img.texture = ProgressManager.Instance.instances[index].cosmetic.sprite.texture;
        }
        else
        {
            #if UNITY_EDITOR
            if (Application.isEditor) img.texture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Resources/MapSprites/" + ProgressManager.Instance.instances[index].maps[mapIndex] + ".png", typeof(Texture2D));
            #endif
            if (!Application.isEditor) img.texture = (Texture2D)Resources.Load("MapSprites/" + ProgressManager.Instance.instances[index].maps[mapIndex], typeof(Texture2D));
        }


    }

    public void UpdateUI()
    {
        if (ProgressManager.Instance.instances[index].unlocked) {
            lockedText.text = (ProgressManager.Instance.instances[index].dlcExlusive ? "unlocked  DLC" : "unlocked");
            text.color = (ProgressManager.Instance.instances[index].dlcExlusive ? DlcunlockedColor : unlockedColor);
            if (ProgressManager.Instance.instances[index].cosmetic != null)
                text.text = ProgressManager.Instance.instances[index].cosmetic.cosmeticName;
            else
                text.text = ProgressManager.Instance.instances[index].maps[mapIndex];

            contentLayout.text.color = unlockedColor;
        }
        else {
            lockedText.text = (ProgressManager.Instance.instances[index].dlcExlusive ? "locked  DLC" : "locked");
            text.color = (ProgressManager.Instance.instances[index].dlcExlusive ? DlclockedColor : lockedColor);
            text.text = "???";

            contentLayout.text.color = lockedColor;
        }
    } 

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Application.isFocused) return;
        PauseManager.Instance.PlayMenuClip(PauseManager.Instance.genericMenuClip);

        if (ProgressManager.Instance.instances[index].dlcExlusive) FloatingName.Instance.nameToShow = "DLC exclusive content";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Application.isFocused) return;
        PauseManager.Instance.PlayMenuClip(PauseManager.Instance.genericMenuClip);

        if (ProgressManager.Instance.instances[index].dlcExlusive) FloatingName.Instance.nameToShow = "";
    }
    
}
