using FishNet;
using FishNet.Managing.Logging;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FishNet.Component.Spawning;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using FishNet.Managing;
using System.Threading.Tasks;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public string[] SCENE_NAME;
    public int sceneIndex;
    private float respawnTimer;

    public string currentScene;
    private float timer;
    public bool canSwitch = true;

    public PlayerTracker manager;
    [SerializeField] public GameObject _loaderCanvas;
    [SerializeField] public TextMeshProUGUI sceneText;

    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    

    private void Start()
    {
        Shuffle(SCENE_NAME);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            LoadScene();
        }
        currentScene = UnitySceneManager.GetActiveScene().name;
    }

    public void LoadScene()
    {
        if (canSwitch)
            LoadSceneInternal();
    }

    public void LoadSceneInternal()
    {
        StartCoroutine(CanStartAgain());

        var players = GameObject.FindGameObjectsWithTag("ClientInstance");

        SceneLoadData sld = new SceneLoadData(SCENE_NAME[sceneIndex]);
        //sld.MovedNetworkObjects = new NetworkObject[players.Length];

        /*for (int i = 0; i < players.Length; i++)
        {
            sld.MovedNetworkObjects[i] = players[i].GetComponent<NetworkObject>();
        }*/
        
        sld.ReplaceScenes = ReplaceOption.All;
        
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        if (sceneIndex < SCENE_NAME.Length-1) sceneIndex ++;
        else sceneIndex = 0;
    }

    private IEnumerator CanStartAgain()
    {
        canSwitch = false;
        yield return new WaitForSeconds(0.5f);
        canSwitch = true;
    }

    public void LoadSceneSteam()
    {
        var players = GameObject.FindGameObjectsWithTag("ClientInstance");

        SceneLoadData sld = new SceneLoadData(SCENE_NAME[sceneIndex]);
        sld.MovedNetworkObjects = new NetworkObject[players.Length];

        LoadOptions loadOptions = new LoadOptions
        {
            AllowStacking = true,
        };
        sld.Options = loadOptions;

        for (int i = 0; i < players.Length; i++)
        {
            sld.MovedNetworkObjects[i] = players[i].GetComponent<NetworkObject>();
        }

        
        sld.ReplaceScenes = ReplaceOption.All;
        
        InstanceFinder.SceneManager.LoadConnectionScenes(sld);

        //if (sceneIndex < SCENE_NAME.Length-1) sceneIndex ++;
        //else sceneIndex = 0;
    }

    public void LoadSceneFromMenu()
    {
        SceneLoadData sld = new SceneLoadData(SCENE_NAME[sceneIndex]);
        
        sld.ReplaceScenes = ReplaceOption.All;
        
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);

        if (sceneIndex < SCENE_NAME.Length-1) sceneIndex ++;
        else sceneIndex = 0;
    }

    public void ReturnToMenu()
    {
        UnitySceneManager.LoadScene("MainMenu");
    }

    public void Shuffle(string[] texts)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < texts.Length; t++ )
        {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
    }

    public async void LoadScene(string sceneName)
    {
        //_target = 0;
        //_progressBar.fillAmount = 0;

        var scene = UnitySceneManager.LoadSceneAsync(sceneName);
        //scene.allowSceneActivation = false;

        _loaderCanvas.SetActive(true);

        do {
            await Task.Delay(100);

            //_progressBar.fillAmount = scene.progress;

        } while (scene.progress < 0.9f);

        //await Task.Delay(100);

        //scene.allowSceneActivation = true;
        _loaderCanvas.SetActive(false);
    }

    
}
