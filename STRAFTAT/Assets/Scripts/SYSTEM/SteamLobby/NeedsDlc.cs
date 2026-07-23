using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedsDlc : MonoBehaviour
{
    [SerializeField] private bool dlc0;
    [SerializeField] private bool dlc1;


    // Start is called before the first frame update
    void Start()
    {
        if (dlc0) {
            if (!SteamLobby.ownDlc0) transform.localScale = Vector3.zero;
        }
    }

}
