using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("tutorialClicked")) {
            if (PlayerPrefs.GetInt("tutorialClicked") == 1) gameObject.SetActive(false);
        }
    }

    public void DisableObject() {
        PlayerPrefs.SetInt("tutorialClicked", 1);
        gameObject.SetActive(false);
    }
}
