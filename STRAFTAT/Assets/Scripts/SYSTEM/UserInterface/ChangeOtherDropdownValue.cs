using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeOtherDropdownValue : MonoBehaviour
{
    private TMP_Dropdown self;

    void Start()
    {
        if (GetComponent<TMP_Dropdown>() != null) self = GetComponent<TMP_Dropdown>();
    }

    public void ChangeDropdownValue(TMP_Dropdown dropdown)
    {
        if (!gameObject.activeInHierarchy) return;
        dropdown.value = self.value;
    }
}
