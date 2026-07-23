using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class XpContentLayout : MonoBehaviour
{
    public int xpToUnlock;
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text.text = xpToUnlock.ToString() + "xp";
    }
}
