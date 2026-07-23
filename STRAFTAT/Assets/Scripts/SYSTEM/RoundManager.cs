using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using FishNet.Object;
using FishNet;
using System.Linq;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance;

    public string[] names;
    public int[] scores;
    public ClientInstance[] players;

    [SerializeField] private float tweenTime = 0.75f;

    [SerializeField] public AudioClip[] swooshClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip plusOneClip;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;

    private GameObject container;
    private GameObject background;

    private GameObject selfPlusOne;
    private GameObject[] enemyPlusOne = new GameObject[3];

    private TextMeshProUGUI playerName;
    private TextMeshProUGUI[] enemyName = new TextMeshProUGUI[3];

    private TextMeshProUGUI playerScore;
    private TextMeshProUGUI[] enemyScore = new TextMeshProUGUI[3];

    private TextMeshProUGUI winText;
    private TextMeshProUGUI roundText;

    private Transform nextRoundImageOne;
    private Transform nextRoundImageTwo;

//
    private Transform nextRoundImageThree;
    private Transform nextRoundImageFour;
    private Transform nextRoundImageFive;
    private Transform nextRoundImageSix;

//
    private Transform nextRoundImageOneRestPos;
    private Transform nextRoundImageOneActivePos;

    private Transform nextRoundImageTwoRestPos;
    private Transform nextRoundImageTwoActivePos;

    private Transform nextRoundImageThreeRestPos;
    private Transform nextRoundImageThreeActivePos;

    private Transform nextRoundImageFourRestPos;
    private Transform nextRoundImageFourActivePos;

    private Transform nextRoundImageFiveRestPos;
    private Transform nextRoundImageFiveActivePos;

    private Transform nextRoundImageSixRestPos;
    private Transform nextRoundImageSixActivePos;

    void Awake()
    {
        Instance = this;

        selfPlusOne = GameObject.Find("SelfPlusOne");
        enemyPlusOne[0] = GameObject.Find("Enemy1PlusOne");
        enemyPlusOne[1] = GameObject.Find("Enemy2PlusOne");
        enemyPlusOne[2] = GameObject.Find("Enemy3PlusOne");

        container = GameObject.Find("--END ROUND SCREEN--");
        background = GameObject.Find("RoundBg");
        
        nextRoundImageOne = GameObject.Find("Score_One").transform;
        nextRoundImageTwo = GameObject.Find("Score_Two").transform;
        nextRoundImageThree = GameObject.Find("Score_Three").transform;
        nextRoundImageFour = GameObject.Find("Score_Four").transform;
        nextRoundImageFive = GameObject.Find("Score_Five").transform;
        nextRoundImageSix = GameObject.Find("Score_Six").transform;

        nextRoundImageOneRestPos = GameObject.Find("PosOneRest").transform;
        nextRoundImageTwoRestPos = GameObject.Find("PosTwoRest").transform;
        nextRoundImageThreeRestPos = GameObject.Find("PosThreeRest").transform;
        nextRoundImageFourRestPos = GameObject.Find("PosFourRest").transform;
        nextRoundImageFiveRestPos = GameObject.Find("PosFiveRest").transform;
        nextRoundImageSixRestPos = GameObject.Find("PosSixRest").transform;

        nextRoundImageOneActivePos = GameObject.Find("PosOneActive").transform;
        nextRoundImageTwoActivePos = GameObject.Find("PosTwoActive").transform;
        nextRoundImageThreeActivePos = GameObject.Find("PosThreeActive").transform;
        nextRoundImageFourActivePos = GameObject.Find("PosFourActive").transform;
        nextRoundImageFiveActivePos = GameObject.Find("PosFiveActive").transform;
        nextRoundImageSixActivePos = GameObject.Find("PosSixActive").transform;


        playerName = nextRoundImageThree.GetChild(0).GetComponent<TextMeshProUGUI>();
        enemyName[0] = nextRoundImageFour.GetChild(0).GetComponent<TextMeshProUGUI>();
        enemyName[1] = nextRoundImageFive.GetChild(0).GetComponent<TextMeshProUGUI>();
        enemyName[2] = nextRoundImageSix.GetChild(0).GetComponent<TextMeshProUGUI>();

        playerScore = GameObject.Find("LocalPlayerScore").GetComponent<TextMeshProUGUI>();
        enemyScore[0] = GameObject.Find("Enemy1PlayerScore").GetComponent<TextMeshProUGUI>();
        enemyScore[1] = GameObject.Find("Enemy2PlayerScore").GetComponent<TextMeshProUGUI>();
        enemyScore[2] = GameObject.Find("Enemy3PlayerScore").GetComponent<TextMeshProUGUI>();

        winText = nextRoundImageOne.GetChild(0).GetComponent<TextMeshProUGUI>();
        roundText = nextRoundImageTwo.GetChild(0).GetComponent<TextMeshProUGUI>();

    }

    void Start()
    {
        container.transform.localScale = Vector3.zero;
        background.transform.localScale = Vector3.zero;
    }

    public IEnumerator InterfaceSetupCoroutine; 

    public void StopCoroutine()
    {
        if (InterfaceSetupCoroutine != null) {
            StopCoroutine(InterfaceSetupCoroutine);
            selfPlusOne.transform.localScale = Vector3.zero;
            enemyPlusOne[0].transform.localScale = Vector3.zero;
            enemyPlusOne[1].transform.localScale = Vector3.zero;
            enemyPlusOne[2].transform.localScale = Vector3.zero;

            nextRoundImageOne.DOMove(nextRoundImageOneRestPos.position, 0).SetEase(Ease.OutSine);
            nextRoundImageTwo.DOMove(nextRoundImageTwoRestPos.position, 0).SetEase(Ease.OutSine);
            nextRoundImageThree.DOMove(nextRoundImageThreeRestPos.position, 0).SetEase(Ease.OutSine);
            nextRoundImageFour.DOMove(nextRoundImageFourRestPos.position, 0).SetEase(Ease.OutSine);
            nextRoundImageFive.DOMove(nextRoundImageFourRestPos.position, 0).SetEase(Ease.OutSine);
            nextRoundImageSix.DOMove(nextRoundImageFourRestPos.position, 0).SetEase(Ease.OutSine);

            container.transform.localScale = Vector3.zero;
            background.transform.localScale = Vector3.zero;

            InterfaceSetupCoroutine = null;
        }


    }

    [ServerRpc(RequireOwnership=false)]
    public void CmdEndRound(int winningTeamId)
    {
        EndRoundObservers(winningTeamId);
    }

    [ObserversRpc]
    public void EndRoundObservers(int winningTeamId)
    {
        NextRoundCall(LobbyController.Instance.LocalPlayerController.PlayerId, ScoreManager.Instance.GetTeamId(LobbyController.Instance.LocalPlayerController.PlayerId)==winningTeamId, winningTeamId);
    }

    public void NextRoundCall(int playerId, bool won, int winningTeamId)
    {

        names = new string[4];
        scores = new int[4];
        players = new ClientInstance[SteamLobby.Instance.players.Count];

        if (won) Settings.Instance.IncreaseRoundsWon();
        else Settings.Instance.IncreaseRoundsLost();

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = SteamLobby.Instance.players[i].GetComponent<ClientInstance>();
        }

        for (int i = 0; i < names.Length; i++)
        {
            if (ClientInstance.playerInstances.TryGetValue(i, out ClientInstance instance)) {
                names[i] = instance.PlayerName;
            }
            else names[i] = "";
            
        }

        for (int i = 0; i < scores.Length; i++)
        {
            scores[i] = ScoreManager.Instance.GetPoints(ScoreManager.Instance.GetTeamId(i));
        }

        InterfaceSetupCoroutine = InterfaceSetup(playerId, won, winningTeamId);

        StartCoroutine(InterfaceSetupCoroutine);
    }

    private IEnumerator InterfaceSetup(int playerId, bool won, int winningTeamId)
    {
        PauseManager.Instance.onEndRoundScreen = true;
        yield return new WaitForSeconds(0f);
        DynamicLoading(playerId);


        selfPlusOne.transform.localScale = Vector3.zero;
        enemyPlusOne[0].transform.localScale = Vector3.zero;
        enemyPlusOne[1].transform.localScale = Vector3.zero;
        enemyPlusOne[2].transform.localScale = Vector3.zero;

        SoundManager.Instance.PlaySound(won ? winClip : loseClip);

        container.transform.localScale = Vector3.one;
        background.transform.localScale = Vector3.one;

        playerName.text = names[playerId].ToLower();
        playerScore.text = (won && scores[playerId]-1 >= 0 ? scores[playerId]-1 : scores[playerId]).ToString();
        if (names.Length >= 2) {
            for (int i=0; i<4; i++) {
                if (ClientInstance.playerInstances.ContainsKey(i) && playerId != i) {
                    enemyName[(playerId < i ? i-1 : i)].text = names[i];
                }
            }
        }
        if (scores.Length >= 2) {
            for (int i=0; i<4; i++) {
                if (ClientInstance.playerInstances.ContainsKey(i) && playerId != i) {
                    bool isWinner = ScoreManager.Instance.GetTeamId(i)==winningTeamId;
                    enemyScore[(playerId < i ? i-1 : i)].text = (isWinner ? scores[i]-1 : scores[i]).ToString();
                }
            }
        }
        winText.color = (won ? winColor : loseColor);
        winText.text = (won ? "victory" : "defeat");
        //roundText.text = "set n° " + SceneMotor.Instance.sceneIndex.ToString();
        roundText.text = "";

        SoundManager.Instance.PlaySound(swooshClip[0]);
        nextRoundImageOne.DOMove(nextRoundImageOneActivePos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageTwo.DOMove(nextRoundImageTwoActivePos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageThree.DOMove(nextRoundImageThreeActivePos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageFour.DOMove(nextRoundImageFourActivePos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageFive.DOMove(nextRoundImageFiveActivePos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageSix.DOMove(nextRoundImageSixActivePos.position, tweenTime).SetEase(Ease.OutSine);

        yield return new WaitForSeconds(0.75f);

        SoundManager.Instance.PlaySound(plusOneClip);
        if (won) selfPlusOne.transform.localScale = Vector3.one;
        for (int i=0; i<4; i++) {
            if (ClientInstance.playerInstances.ContainsKey(i) && playerId != i) {
                bool isWinner = ScoreManager.Instance.GetTeamId(i)==winningTeamId;
                if (isWinner) enemyPlusOne[(playerId < i ? i-1 : i)].transform.localScale = Vector3.one;
            }
        }

        yield return new WaitForSeconds(0.8f);

        if (won) selfPlusOne.transform.DOScale(Vector3.zero, 0.2f);
        for (int i=0; i<4; i++) {
            if (ClientInstance.playerInstances.ContainsKey(i) && playerId != i) {
                bool isWinner = ScoreManager.Instance.GetTeamId(i)==winningTeamId;
                if (isWinner) enemyPlusOne[(playerId < i ? i-1 : i)].transform.DOScale(Vector3.zero, 0.2f);
            }
        }

        yield return new WaitForSeconds(0.2f);

        playerScore.text = scores[playerId].ToString();
        if (scores.Length >= 2) {
            for (int i=0; i<4; i++) {
                if (ClientInstance.playerInstances.ContainsKey(i) && playerId != i) {
                    enemyScore[(playerId < i ? i-1 : i)].text = scores[i].ToString();
                }
            }
        }

        selfPlusOne.transform.localScale = Vector3.zero;
        enemyPlusOne[0].transform.localScale = Vector3.zero;
        enemyPlusOne[1].transform.localScale = Vector3.zero;
        enemyPlusOne[2].transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(1.25f);

        SoundManager.Instance.PlaySound(swooshClip[1]);
        nextRoundImageOne.DOMove(nextRoundImageOneRestPos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageTwo.DOMove(nextRoundImageTwoRestPos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageThree.DOMove(nextRoundImageThreeRestPos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageFour.DOMove(nextRoundImageFourRestPos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageFive.DOMove(nextRoundImageFiveRestPos.position, tweenTime).SetEase(Ease.OutSine);
        nextRoundImageSix.DOMove(nextRoundImageSixRestPos.position, tweenTime).SetEase(Ease.OutSine);

        yield return new WaitForSeconds(1f);

        container.transform.localScale = Vector3.zero;
        background.transform.localScale = Vector3.zero;

        SceneMotor.Instance.ShowLoadingScreen();

        PauseManager.Instance.onEndRoundScreen = false;

    }

    private void DynamicLoading(int playerId) {
        switch (playerId) {
            case 0: {
                if (!ClientInstance.playerInstances.ContainsKey(1)) nextRoundImageFour.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(2)) nextRoundImageFive.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(3)) nextRoundImageSix.transform.localScale = Vector3.zero;
                if (ClientInstance.playerInstances.ContainsKey(1)) nextRoundImageFour.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(2)) nextRoundImageFive.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(3)) nextRoundImageSix.transform.localScale = Vector3.one;
                break;
            }
            case 1: {
                if (!ClientInstance.playerInstances.ContainsKey(0)) nextRoundImageFour.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(2)) nextRoundImageFive.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(3)) nextRoundImageSix.transform.localScale = Vector3.zero;
                if (ClientInstance.playerInstances.ContainsKey(0)) nextRoundImageFour.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(2)) nextRoundImageFive.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(3)) nextRoundImageSix.transform.localScale = Vector3.one;
                break;
            }
            case 2: {
                if (!ClientInstance.playerInstances.ContainsKey(0)) nextRoundImageFour.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(1)) nextRoundImageFive.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(3)) nextRoundImageSix.transform.localScale = Vector3.zero;
                if (ClientInstance.playerInstances.ContainsKey(0)) nextRoundImageFour.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(1)) nextRoundImageFive.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(3)) nextRoundImageSix.transform.localScale = Vector3.one;
                break;
            }
            case 3: {
                if (!ClientInstance.playerInstances.ContainsKey(0)) nextRoundImageFour.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(1)) nextRoundImageFive.transform.localScale = Vector3.zero;
                if (!ClientInstance.playerInstances.ContainsKey(2)) nextRoundImageSix.transform.localScale = Vector3.zero;
                if (ClientInstance.playerInstances.ContainsKey(0)) nextRoundImageFour.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(1)) nextRoundImageFive.transform.localScale = Vector3.one;
                if (ClientInstance.playerInstances.ContainsKey(2)) nextRoundImageSix.transform.localScale = Vector3.one;
                break;
            }
        }
    }


}
