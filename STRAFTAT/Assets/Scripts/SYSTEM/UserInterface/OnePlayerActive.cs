using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnePlayerActive : MonoBehaviour
{
    private SteamLobby manager;
    private Vector3 initScale;
    // Start is called before the first frame update
    void Start()
    {
        manager = SteamLobby.Instance;
        initScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = (manager.players.Count > 1 ? Vector3.zero : initScale);
    }
}
