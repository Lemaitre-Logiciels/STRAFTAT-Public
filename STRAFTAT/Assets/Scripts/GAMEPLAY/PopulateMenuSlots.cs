using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateMenuSlots : MonoBehaviour
{
    private SteamLobby steamLobby;
    [SerializeField] private int id;
    // Start is called before the first frame update
    void Start()
    {
        steamLobby = SteamLobby.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (id+1 <= steamLobby.maxPlayers) transform.localScale = Vector3.one;
        else transform.localScale = Vector3.zero;
    }
}
