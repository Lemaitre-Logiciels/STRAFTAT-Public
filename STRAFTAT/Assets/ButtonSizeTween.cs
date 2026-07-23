using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonSizeTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float tweenDuration;
    [SerializeField] private Ease easeType;
    [SerializeField] private bool parentScale;
    [SerializeField] private bool takeInitScale;
    Vector3 initScale;
    [Space]
    [SerializeField] private bool closeWindow;
    [Space]
    [SerializeField] private bool biggerWhenSelected;
    [SerializeField] private bool biggerWithGamepadOnly;
    [SerializeField] private Vector3 selectedScale;
    [SerializeField] private float tweenDuration2;
    [SerializeField] private Ease easeType2;
    [Space]
    [SerializeField] private bool scaleTextOnly;
    TextMeshProUGUI textChild;

    Button button;

    void Awake()
    {
        if (GetComponent<Button>())
            button = GetComponent<Button>();
        
        initScale = (takeInitScale ? (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).localScale : Vector3.one);
        if (scaleTextOnly) textChild = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        if (button) button.onClick.AddListener(TaskOnClick);
        
    }

    void OnEnable()
    {
        (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).localScale = initScale;

        if (dlcLockedButton) button.interactable = SteamLobby.ownDlc0;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Application.isFocused) return;
        PauseManager.Instance.PlayMenuClip(PauseManager.Instance.genericMenuClip);
        (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).transform.DOKill();
        if (PauseManager.Instance.gamepad ? true : !biggerWithGamepadOnly) (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).DOScale(targetScale, tweenDuration).SetEase(easeType);

        if (dlcLockedButton && !button.interactable) FloatingName.Instance.nameToShow = "Get the DLC !";
        if (addFriendButton) FloatingName.Instance.nameToShow = "Add friend on steam";
        if (customText != "") FloatingName.Instance.nameToShow = customText;
        if (randomizedLockedButton) {
            /*if (!GetComponent<Toggle>().interactable) {
                if (Settings.Instance.gamesPlayed >= 10) FloatingName.Instance.nameToShow= "Unlocked !";
                else FloatingName.Instance.nameToShow = "Play " + (int)(10-Settings.Instance.gamesPlayed) + " games to unlock";
            }
            if (Settings.Instance.gamesPlayed >= 10) GetComponent<Toggle>().interactable = true;*/

            if (!SteamLobby.ownDlc0) { FloatingName.Instance.nameToShow = "Requires the DLC and 10 played matches to unlock."; }
            else {
                if (Settings.Instance.gamesPlayed >= 10) { FloatingName.Instance.nameToShow= "Enable or disable randomized weapons"; }
                else { FloatingName.Instance.nameToShow = $"Play {(int)(10 - Settings.Instance.gamesPlayed)} more matches to unlock"; }
            }
        }
    }

    [Space]
    [SerializeField] private bool startGameButton;
    [SerializeField] private bool dlcLockedButton;
    [SerializeField] private bool addFriendButton;
    [SerializeField] private bool randomizedLockedButton;
    public string customText;

    public void OnPointerDown(PointerEventData eventData){
        PauseManager.Instance.PlayMenuClip(PauseManager.Instance.pressMenuClip);

        if (startGameButton)
        {
            if (GetComponent<Button>().interactable) return;
            
            if (ClientInstance.Instance.PlayerId != 0) {
                PauseManager.Instance.WriteOfflineLog("You are not the host, you can't start the game");
                
                if (!ClientInstance.Instance.Ready) { PauseManager.Instance.WriteOfflineLog("You must be ready for the host to start the game, click the Ready Up button"); } 
                else { PauseManager.Instance.WriteLog("Waiting on other players to be ready and the host to start the game..."); } // This is sent to all players, so they can see it
                
                return;
            }

            if (SteamLobby.Instance.players.Count < 2) {
                PauseManager.Instance.WriteOfflineLog("There must be 2 players to start a game");
                return;
            }

            PauseManager.Instance.WriteLog("Not all players are ready");
            GameObject.Find("ReadyUpButton").GetComponent<ScaleTweenLoop>().TempTween(); // TODO: network this as it would be funny
        }
    }

    public void OnPointerUp(PointerEventData eventData){
        //buttonPressed = false;
    }

    bool isSelected;

    public void OnSelect (BaseEventData eventData) 
	{
        if (biggerWhenSelected) {
            isSelected = true;
		    if (PauseManager.Instance.gamepad ? true : !biggerWithGamepadOnly) (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).DOScale(selectedScale, tweenDuration2).SetEase(easeType2);
        }

        if (PauseManager.Instance.gamepad)
        {
            PauseManager.Instance.PlayMenuClip(PauseManager.Instance.genericMenuClip);
            (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).transform.DOKill();
            if (PauseManager.Instance.gamepad ? true : !biggerWithGamepadOnly) (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).DOScale(targetScale, tweenDuration).SetEase(easeType);
        }
	}
    public void OnDeselect (BaseEventData eventData) 
	{
        if (biggerWhenSelected) {
            isSelected = false;
		    if (PauseManager.Instance.gamepad ? true : !biggerWithGamepadOnly) (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).DOScale(initScale, tweenDuration2).SetEase(easeType2);
        }

        if (PauseManager.Instance.gamepad)
        {
            if (isSelected) return;
            (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).transform.DOKill();
            if (PauseManager.Instance.gamepad ? true : !biggerWithGamepadOnly) (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).transform.DOScale(initScale, tweenDuration).SetEase(easeType);
        }
	}

    void TaskOnClick()
    {
        if (closeWindow) PauseManager.Instance.PlayMenuClip(PauseManager.Instance.closeMenuClip);
        else PauseManager.Instance.PlayMenuClip(PauseManager.Instance.releaseMenuClip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Application.isFocused) return;
        if (isSelected) return;
        (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).transform.DOKill();
        if (PauseManager.Instance.gamepad ? true : !biggerWithGamepadOnly) (scaleTextOnly ? textChild.transform : parentScale ? transform.parent : transform).transform.DOScale(initScale, tweenDuration).SetEase(easeType);

        FloatingName.Instance.nameToShow = "";
    }
}
