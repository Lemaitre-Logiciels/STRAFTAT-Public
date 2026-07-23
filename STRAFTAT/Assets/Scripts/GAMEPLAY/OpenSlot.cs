using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet;

public class OpenSlot : MonoBehaviour
{
    private SteamLobby steamLobby;
    // Start is called before the first frame update
    void Start()
    {
        steamLobby = SteamLobby.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (InstanceFinder.NetworkManager == null) return;

        if (InstanceFinder.NetworkManager.IsServer && steamLobby.maxPlayers <4) transform.localScale = Vector3.one;
        else transform.localScale = Vector3.zero;
    }

    public void AddSlot(TMP_Dropdown dropdown){
        dropdown.value = dropdown.value+1;
    }


}
