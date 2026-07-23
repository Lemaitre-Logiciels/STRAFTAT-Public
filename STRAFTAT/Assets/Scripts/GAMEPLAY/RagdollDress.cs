using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollDress : MonoBehaviour
{
    public GameObject[] meshesToChange;

    void OnEnable()
    {
        PauseManager.OnRoundStarted += StartNewRound;
    }

    void OnDisable()
    {
        PauseManager.OnRoundStarted -= StartNewRound;

    }

    void StartNewRound()
    {
        if (ClientInstance.playerInstances.Count > 2) { Destroy(gameObject); }
    }

}
