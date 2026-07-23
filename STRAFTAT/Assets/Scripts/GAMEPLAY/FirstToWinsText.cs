using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using TMPro;

public class FirstToWinsText : MonoBehaviour
{
    [SerializeField] private bool onEndRoundScreen;
    [SerializeField] private TextMeshProUGUI text;
    SceneMotor sceneMotor;
    ButtonSizeTween buttonScript;
    PauseManager pauseManager;
    // Start is called before the first frame update
    void Awake()
    {
        if (!onEndRoundScreen) buttonScript = GetComponent<ButtonSizeTween>();
    }

    void Start() {
        pauseManager = PauseManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!onEndRoundScreen) {
            if (SceneMotor.Instance == null) return;
            if (sceneMotor == null) sceneMotor = SceneMotor.Instance;

            if (InstanceFinder.NetworkManager.IsServer) transform.localScale = Vector3.zero;
            else transform.localScale = Vector3.one;

            text.text = $"Win {sceneMotor.roundAmount} Rounds";
            buttonScript.customText = $"First Player who wins {sceneMotor.roundAmount} Rounds wins the Match";
        }
        else {
            if (!pauseManager.onEndRoundScreen){
                transform.localScale = Vector3.zero;
                return;
            }
            transform.localScale = Vector3.one;

            if (SceneMotor.Instance == null) return;
            if (sceneMotor == null) sceneMotor = SceneMotor.Instance;

            text.text = $"First To {sceneMotor.roundAmount}";
        }

    }
}
