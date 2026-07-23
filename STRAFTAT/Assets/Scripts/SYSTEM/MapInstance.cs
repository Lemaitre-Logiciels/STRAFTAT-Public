using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.EventSystems;

public class MapInstance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string name;
    public bool selected = false;

    [Space]
    public Transform mapsViewport;
    public Transform mapsAddedViewport;
    [Space]

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private TextMeshProUGUI addText;
    [SerializeField] private Texture2D sprite;
    [SerializeField] private RawImage img;
    [SerializeField] private Button button;

    private void Awake()
    {
        transform.GetComponentInChildren<TextMeshProUGUI>().text = name;
    }

    void Start() {
        sprite = (Texture2D)Resources.Load("MapSprites/" + name, typeof(Texture2D));

        img.texture = sprite;
        UpdateUI();
    }


    public void UpdateUI()
    {
        transform.GetComponentInChildren<TextMeshProUGUI>().text = name;
        addText.text = (!selected ? "+" : "-");
        var colors = button.colors;
        colors.normalColor = (selected ? selectedColor : deselectedColor);
        colors.selectedColor = (selected ? selectedColor : deselectedColor);
        colors.disabledColor = (selected ? selectedColor : deselectedColor);
        button.colors = colors;
    }

    public void ChangeState() {
        selected = !selected;

        addText.text = (!selected ? "+" : "-");
        var colors = button.colors;
        colors.normalColor = (selected ? selectedColor : deselectedColor);
        colors.selectedColor = (selected ? selectedColor : deselectedColor);
        colors.disabledColor = (selected ? selectedColor : deselectedColor);
        button.colors = colors;
        MapsManager.Instance.ChangeMapsState(name);
    }

    public void OpenMap()
    {
        SteamLobby.Instance.EnterExplorationMap(name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MapsManager.Instance.ChangePicture(sprite, name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //MapsManager.Instance.ChangePicture(null, "");
    }

    void OnDisable()
    {
        MapsManager.Instance.ChangePicture(null, "");
    }

}
