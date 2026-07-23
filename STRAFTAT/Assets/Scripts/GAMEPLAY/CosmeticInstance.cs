using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CosmeticInstance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isHat = true;
    public bool isCig = false;
    public GameObject hat;
    private Button button;
    public Sprite sprite;
    public bool acquired;
    private Image img;
   
    [Space] public string challengeDescription;
    [Space] public string cosmeticName;
    [Space] public int index;

    [Space]
    public bool unlockWithXp;
    public bool unlockWithDlc;
    public bool unlockWithSupporterDlc;
    public bool unlockWithChallenge;

    [Space]
    [SerializeField] private Color unlockedColor;
    [SerializeField] private Color lockedColor;

    void Start()
    {
        transform.Find("Image").GetComponent<Image>().sprite = this.sprite;
        img = transform.Find("Image").GetComponent<Image>();
        button = GetComponent<Button>();

        if (unlockWithXp)
        {
            foreach (var ins in ProgressManager.Instance.instances)
            {
                if (ins.cosmetic != null)
                {
                    if (ins.cosmetic == this) challengeDescription = "Reach " + (ins.xpToUnlock) + " xp to unlock";
                }
            }
        }

        img.color = acquired ? unlockedColor : lockedColor;

        if (SteamLobby.ownDlc0 && unlockWithDlc) Unlock();
        if (SteamLobby.ownDlc1 && unlockWithSupporterDlc) Unlock();
    }

    void OnEnable()
    { 
        if (SteamLobby.ownDlc0 && unlockWithDlc) Unlock();
        if (SteamLobby.ownDlc1 && unlockWithSupporterDlc) Unlock();
    }

    public void ChangeDress()
    {
        CosmeticsManager.Instance.ChangeDress(this);
        
    }

    void Update()
    {
        button.interactable = acquired;
        if (img != null) img.color = acquired ? unlockedColor : lockedColor;
    }

    public void Unlock()
    {
        acquired = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unlockWithSupporterDlc){
            if (!acquired) {
                FloatingName.Instance.nameToShow = "Get the Supporter Edition DLC !";
            }
            else {
                if (isHat)
                    FloatingName.Instance.nameToShow = "Supporter DLC Hat unlocked !";
                else if (isCig)
                    FloatingName.Instance.nameToShow = "Supporter DLC Cig unlocked !";
                else
                    FloatingName.Instance.nameToShow = "Supporter DLC Suit unlocked !";
            }
        }
        else if (unlockWithDlc){
            if (!acquired) {
                FloatingName.Instance.nameToShow = "Get the DLC !";
            }
            else {
                if (isHat)
                    FloatingName.Instance.nameToShow = "DLC Hat unlocked !";
                else if (isCig)
                    FloatingName.Instance.nameToShow = "DLC Cig unlocked !";
                else
                    FloatingName.Instance.nameToShow = "DLC Suit unlocked !";
            }
        }
        else {
            if (!acquired) {
                FloatingName.Instance.nameToShow = challengeDescription;
            }
            else 
                FloatingName.Instance.nameToShow = (challengeDescription != "" ? "<s>"+challengeDescription+"</s>"+" Done!" : "");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FloatingName.Instance.nameToShow = "";
    }

}
