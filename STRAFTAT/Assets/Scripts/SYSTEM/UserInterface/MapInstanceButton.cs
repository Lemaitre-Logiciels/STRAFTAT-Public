using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapInstanceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isExplore;
    [SerializeField] private MapInstance parentScript;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isExplore) FloatingName.Instance.nameToShow = "Click to explore map";
        else FloatingName.Instance.nameToShow = (parentScript.selected ? "Click to remove map": "Click to add map");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FloatingName.Instance.nameToShow = "";
    }
}
