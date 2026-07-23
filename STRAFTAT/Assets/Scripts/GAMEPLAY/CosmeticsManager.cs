using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FishNet;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class CosmeticsManager : MonoBehaviour, ISaveable
{
    public static CosmeticsManager Instance;

    [SerializeField] private Transform suitsParent;
    [SerializeField] private Transform hatsParent;
    [SerializeField] private Transform cigsParent;


    public Material[] mats;
    public Material[] fparmsMats;
    public GameObject[] hats;
    public GameObject[] cigs;

    public int suitIndex;
    public int hatIndex;
    public int cigIndex;

    CosmeticInstance[] suitsChildren;
    CosmeticInstance[] hatsChildren;
    CosmeticInstance[] cigsChildren;

    bool inMenu;
    bool activate;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);

        //if (SceneManager.GetActiveScene().name == "MainMenu") return;

        

    }

    void Start()
    {
        hatsChildren = hatsParent.GetComponentsInChildren<CosmeticInstance>();
        hats = new GameObject[hatsChildren.Length];

        for (int i = 0; i < hats.Length; i++)
        {
            hats[i] = hatsChildren[i].hat;
            hatsChildren[i].index = i;
        }

        suitsChildren = suitsParent.GetComponentsInChildren<CosmeticInstance>();
        cigsChildren = cigsParent.GetComponentsInChildren<CosmeticInstance>();
        StartCoroutine(DressAboubi());
        
        Debug.Log($"DLC Status: DLC0: {SteamLobby.ownDlc0}, DLC1: {SteamLobby.ownDlc1}");
    }

    public void UpdateUnlockable()
    {
        ProgressManager progressManager = ProgressManager.Instance;

        foreach (var instance in progressManager.instances)
        {
            if (instance.cosmetic != null && instance.xpToUnlock <= progressManager.xp) instance.cosmetic.Unlock();
        }
    }

    IEnumerator DressAboubi()
    {
        yield return new WaitForSeconds(1);
        //if (!MapsManager.Instance.beta) SaveLoadSystem.Instance.Load();
        LoadDress();
    }

    [ContextMenu ("Load Dress")]
    void LoadDress() {
        ChangeDress(suitsChildren[suitIndex]);
        ChangeDress(hatsChildren[hatIndex]);
        ChangeDress(cigsChildren[cigIndex]);
    }


    void Update()
    {
        
        if (PauseManager.Instance.inMainMenu && !inMenu)
        {
            inMenu = true;
            StartCoroutine(DressAboubi());
        }
        else if (!PauseManager.Instance.inMainMenu) inMenu = false;
    }

    public GameObject currenthat;
    public int currentsuitIndex;
    public int currentcigIndex;

    public void ChangeDress(CosmeticInstance cosmeticInstance) {
        if (cosmeticInstance.isHat) {
            currenthat = cosmeticInstance.hat;
            hatIndex = cosmeticInstance.index;
        } else if (cosmeticInstance.isCig) {
            currentcigIndex = cosmeticInstance.index;
            cigIndex = cosmeticInstance.index;
        } else {
            currentsuitIndex = cosmeticInstance.index;
            suitIndex = cosmeticInstance.index;
        }

        // Check bounds
        hatIndex = (hatIndex + hatsChildren.Length) % hatsChildren.Length;
        suitIndex = (suitIndex + suitsChildren.Length) % suitsChildren.Length;
        cigIndex = (cigIndex + cigsChildren.Length) % cigsChildren.Length;
        
        AboubiPreview.Instance.ChangeDress(hatsChildren[hatIndex].hat, mats[suitIndex], cigs[cigIndex]);

        if (LobbyController.Instance.LocalPlayerController != null) {
            LobbyController.Instance.LocalPlayerController.DressAboubi(currenthat, currentsuitIndex, currentcigIndex, LobbyController.Instance.LocalPlayerController.PlayerId);
        }

        //SaveLoadSystem.Instance.Save();
    }

    //To unlock items, add Comestic Instances here as serialized components and public functions to be called from any script to unlock its respective boolean

    //Save hat and suit

    public object SaveState()
    {
        return new SaveData()
        {
            hatIndexSaved = this.hatIndex,
            suitIndexSaved = this.suitIndex,
            cigIndexSaved = this.cigIndex
            
        };
    }
    public void LoadState(JContainer state)
    {
        SaveData saveData = state.ToObject<SaveData>();

        hatIndex = saveData.hatIndexSaved;
        suitIndex = saveData.suitIndexSaved;
        cigIndex = saveData.cigIndexSaved;
    }

    [Serializable]
    private struct SaveData
    {
        public int hatIndexSaved;
        public int suitIndexSaved;
        public int cigIndexSaved;
    }
}
