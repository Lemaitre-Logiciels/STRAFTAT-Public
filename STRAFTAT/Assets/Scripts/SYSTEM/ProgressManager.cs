using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public class ProgressManager : MonoBehaviour, ISaveable
{
    private float xpTimer;
    public float xp;
    [SerializeField] private GameObject xpPopupPrefab;
    public ProgressInstance[] instances;
    public bool[] unlocked;
    PauseManager pauseManager;

    [Space]
    [SerializeField] private Transform parentObject;
    [SerializeField] private Transform contentArea;
    [SerializeField] private Transform firstPosition;
    [SerializeField] private Transform lastPosition;
    [SerializeField] private Image xpBarProgressImage;
    [SerializeField] private GameObject xpContentPrefab;
    [SerializeField] private GameObject verticalGroupPrefab;
    [SerializeField] private TextMeshProUGUI xpText;
    [HideInInspector] public List<XpContentInstance> xpContentInstances = new List<XpContentInstance>();

    public static ProgressManager Instance;

    public bool skipAll;

    void Awake()
    {
        if (Instance == null) Instance = this;

        for (int i = 0; i < instances.Length; i++)
        {
            instances[i].index = i;
        }

        //Automatically add alt maps to dlc maps array (and remove duplicates)
        int l = SceneManager.sceneCountInBuildSettings-6;
        string[] allMaps = new string[l];
        for (int i = 0; i < l; i++) {
            string mapName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i + 6));
            allMaps[i] = mapName;
        }

        List<string> tempDlcMaps = new List<string>();
        tempDlcMaps.AddRange(instances[0].maps);
        foreach(var map in instances[0].maps) { if (map.ToLower().EndsWith("_alt")) tempDlcMaps.Remove(map); }
        foreach (var map in allMaps) { if (map.ToLower().EndsWith("_alt")) tempDlcMaps.Add(map); }
        instances[0].maps = tempDlcMaps.ToArray();
    }

    // Start is called before the first frame update
    void Start()
    {
        pauseManager = PauseManager.Instance;
        StartCoroutine(BootupPopups());
        StartCoroutine(UIBootupCoroutine());

        
    }

    IEnumerator UIBootupCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        UIBootup();
    }

    IEnumerator BootupPopups()
    {
        yield return new WaitForSeconds(2);
        RunAllPopups();
    }

    // Update is called once per frame
    bool lastDLCState = false;
    void Update()
    {
        HandleUIUpdate();
        if (!SceneMotor.Instance) return;

        if (!pauseManager.inMainMenu && !pauseManager.inVictoryMenu && !SceneMotor.Instance.testMap)
        {
            if (xp <= 10000) xp += Time.deltaTime * 1.4f;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.N)  && Application.isEditor)
            {
                xp = 30000;
            }
        }
        
        if (lastDLCState != SteamLobby.ownDlc0 && SteamLobby.ownDlc0) {
            lastDLCState = SteamLobby.ownDlc0;
            RunAllPopups();
        }
    }

    [ContextMenu("ActivatePopups")]
    public void RunAllPopups()
    {
        List<ProgressInstance> instancePopups = new List<ProgressInstance>();
        for (int i=0; i < instances.Length; i++)
        {
            if (instances[i].xpToUnlock <= xp) instancePopups.Add(instances[i]);
        }
        StartCoroutine(Popups(instancePopups));
        skipAll = false;

        foreach (var ins in xpContentInstances)
        {
            ins.UpdateUI();
        }

        MapsManager.Instance.UpdateUnlockedMaps();
        CosmeticsManager.Instance.UpdateUnlockable();

        Settings.Instance.mapsUnlocked = MapsManager.Instance.unlockedMaps.Length;
    }

    IEnumerator Popups(List<ProgressInstance> instancePopups)
    {
        for (int i=0; i < instancePopups.Count; i++)
        {
            if ((!instancePopups[i].unlocked && (instancePopups[i].dlcExlusive ? SteamLobby.ownDlc0 : true))) {
                instancePopups[i].unlocked = true;
                unlocked[instancePopups[i].index] = true;
                
                if (instancePopups[i].cosmetic == null) {
                    for (int j=0; j < instancePopups[i].maps.Length; j++)
                    {
                        yield return StartCoroutine(GetPopup(instancePopups[i], instancePopups[i].index, j));
                    }
                }
                else
                    yield return StartCoroutine(GetPopup(instancePopups[i], instancePopups[i].index, 0));
            }
            else 
                yield return null;
        }

        foreach (var ins in xpContentInstances)
        {
            ins.UpdateUI();
        }
    }

    IEnumerator GetPopup(ProgressInstance ins, int index, int mapIndex)
    {
        if (ins.cosmetic != null)
        {
            var tempPopup = Instantiate(xpPopupPrefab, parentObject.position, Quaternion.identity, parentObject);
            var popupScript = tempPopup.GetComponent<XpPopupInstance>();
            popupScript.index = index;
            yield return new WaitUntil(()=>popupScript.clicked || skipAll);
            if (skipAll) yield return new WaitForSeconds(0.01f);
            Destroy(tempPopup);
        }
        else 
        {
            var tempPopup = Instantiate(xpPopupPrefab, parentObject.position, Quaternion.identity, parentObject);
            var popupScript = tempPopup.GetComponent<XpPopupInstance>();
            popupScript.index = index;
            popupScript.mapIndex = mapIndex;
            yield return new WaitUntil(()=>popupScript.clicked || skipAll);
            if (skipAll) yield return new WaitForSeconds(0.01f);
            Destroy(tempPopup);
        }

        SaveLoadSystem.Instance.Save();

        MapsManager.Instance.UpdateUnlockedMaps();
        CosmeticsManager.Instance.UpdateUnlockable();
        Settings.Instance.mapsUnlocked = MapsManager.Instance.unlockedMaps.Length;
        
    }

    void UIBootup()
    {

        for (int i=0; i<instances.Length; i++)
        {
            Vector3 tempPos = Vector3.Lerp(firstPosition.position, lastPosition.position, (float)instances[i].xpToUnlock/(float)instances[instances.Length-1].xpToUnlock);
            var tempLayout = Instantiate(verticalGroupPrefab, tempPos, Quaternion.identity, contentArea);
            tempLayout.GetComponent<XpContentLayout>().xpToUnlock = instances[i].xpToUnlock;

            if (instances[i].cosmetic != null)
            {
                var tempContent = Instantiate(xpContentPrefab, Vector3.zero, Quaternion.identity, tempLayout.transform);
                var script = tempContent.GetComponent<XpContentInstance>();
                script.index = i;
            }
            else 
            {
                for (int j=0; j < instances[i].maps.Length; j++)
                {
                    var tempContent = Instantiate(xpContentPrefab, Vector3.zero, Quaternion.identity, tempLayout.transform);
                    var script = tempContent.GetComponent<XpContentInstance>();
                    script.index = i;
                    script.mapIndex = j;
                }   
            }
        }

        



        foreach (var ins in xpContentInstances)
        {
            ins.UpdateUI();
        }
    }

    void HandleUIUpdate()
    {
        xpBarProgressImage.fillAmount = (float)xp/(float)instances[instances.Length-1].xpToUnlock;

        xpText.text = ((int)xp).ToString() + "/" + instances[instances.Length-1].xpToUnlock.ToString();
    }

    public object SaveState()
    {
        return new SaveData()
        {
            xp = this.xp,
            unlocked = this.unlocked
            
        };
    }

    public void LoadState(JContainer state)
    {
        SaveData saveData = state.ToObject<SaveData>();

        xp = saveData.xp;
        unlocked = saveData.unlocked;
        
        if (instances.Length != unlocked.Length) {
            Debug.LogError($"ProgressManager: Save data unlocked array length ({unlocked.Length}) does not match progress instances length ({instances.Length})");
            bool[] tempArray = new bool[instances.Length];
            int min = Mathf.Min(instances.Length, unlocked.Length);
            for (int i=0; i < min; i++) {
                tempArray[i] = unlocked[i];
            }
            unlocked = tempArray;
        }
        
        for (int i=0; i < instances.Length; i++)
        {
            if (instances[i].dlcExlusive ? SteamLobby.ownDlc0 : true) instances[i].unlocked = unlocked[i];
        }
    }

    [Serializable]
    private struct SaveData
    {
        public float xp;
        public bool[] unlocked;
    }
}
