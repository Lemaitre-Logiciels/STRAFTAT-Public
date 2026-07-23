using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BettaOutButton : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    // Start is called before the first frame update
    void Awake()
    {
        button = GetComponent<Button>();
        
    }

    void Start()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
