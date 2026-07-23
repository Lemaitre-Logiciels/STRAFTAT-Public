using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamIdDropdown : MonoBehaviour
{
    void OnEnable() {
        GetComponentInParent<PlayerListItem>().OnDropdownTeamIdEnabled();
    }

    
}
