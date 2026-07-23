using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TabScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI scoreOne;
    [SerializeField] private TextMeshProUGUI scoreTwo;
    [SerializeField] private TextMeshProUGUI scoreThree;
    [SerializeField] private TextMeshProUGUI scoreFour;
    [SerializeField] private TextMeshProUGUI mapText;

    PauseManager pauseManager;

    private SteamLobby manager;

    public static TabScreen Instance;


    void Awake()
    {
        if (Instance == null){
            Instance = this;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        pauseManager = PauseManager.Instance;

        manager = SteamLobby.Instance;

        
    }

    // Update is called once per frame
    
    void Update()
    {
        if (pauseManager.inMainMenu) return;

        roundText.text = "round " + SceneMotor.Instance.sceneIndex.ToString() + "   First to " + SceneMotor.Instance.roundAmount.ToString() + " wins";
        mapText.text = "current map :  " + SceneManager.GetActiveScene().name;
        
        scoreOne.text = ScoreManager.Instance.GetPoints(ScoreManager.Instance.GetTeamId(0)).ToString();
        scoreTwo.text = ScoreManager.Instance.GetPoints(ScoreManager.Instance.GetTeamId(1)).ToString();
        scoreThree.text = ScoreManager.Instance.GetPoints(ScoreManager.Instance.GetTeamId(2)).ToString();
        scoreFour.text = ScoreManager.Instance.GetPoints(ScoreManager.Instance.GetTeamId(3)).ToString();
    }
}
