using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ModelButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private ModelButtonTween button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        button.Hover();
    }

    public void OnPointerDown(PointerEventData eventData){
        button.Press();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        button.Leave();
    }

    public void OnPointerUp(PointerEventData eventData){
        button.Release();
    }

    public void OnSelect (BaseEventData eventData) 
	{
        if (PauseManager.Instance.gamepad) button.Hover();
	}

    public void OnDeselect (BaseEventData eventData) 
	{
        if (PauseManager.Instance.gamepad) button.Leave();
	}
}
