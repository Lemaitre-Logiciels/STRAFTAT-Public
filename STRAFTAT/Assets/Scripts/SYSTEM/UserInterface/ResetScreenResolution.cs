using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResetScreenResolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Settings.Instance.resolution != 0)
            transform.localScale = Vector3.one;
        else
            transform.localScale = Vector3.zero;
    }

    public void ResetResolution()
    {
        resolutionDropdown.SetValueWithoutNotify(0);
        resolutionDropdown.GetComponent<SettingsResolution>().SetResolutionReal(resolutionDropdown);
    }
}
