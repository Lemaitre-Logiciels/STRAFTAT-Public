using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string customText;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Application.isFocused) return;
        FloatingName.Instance.nameToShow = customText;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Application.isFocused) return;
        FloatingName.Instance.nameToShow = "";
    }
}
