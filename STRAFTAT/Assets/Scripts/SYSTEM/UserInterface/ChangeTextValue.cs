using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangeTextValue : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Slider _slider;
    [SerializeField] private float factor = 0.01f;
    [SerializeField] private float factor2 = 100;
    [SerializeField] private string correct = "F2";

    private void Awake()
    {
	    textMesh = transform.GetComponentInChildren<TextMeshProUGUI>();
        _slider = transform.GetComponentInChildren<Slider>();
    }

    void Start()
    {
        if ((Mathf.Round(_slider.value * factor2) * factor) % 2 == 0 || (Mathf.Round(_slider.value * factor2) * factor) % 2 == 1) 
            textMesh.text = (Mathf.Round(_slider.value * factor2) * factor).ToString() + ".0";
        else
            textMesh.text = (Mathf.Round(_slider.value * factor2) * factor).ToString(correct);
    }

    public void ChangeIntValue(Slider value)
    {
        if ((Mathf.Round(value.value * factor2) * factor) % 2 == 0 || (Mathf.Round(value.value * factor2) * factor) % 2 == 1) 
            textMesh.text = (Mathf.Round(value.value * factor2) * factor).ToString() + ".0";
        else
    		textMesh.text = (Mathf.Round(value.value * factor2) * factor).ToString(correct);
    }
}
