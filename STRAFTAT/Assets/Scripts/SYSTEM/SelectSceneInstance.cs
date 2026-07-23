using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using DG.Tweening;
using UnityEngine.EventSystems;


public class SelectSceneInstance : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public string sceneName;
    public bool selected;

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private RawImage mapImg;
    private Button button;

    private Texture2D sprite;

    public MapSelection mapSelectionScript;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.transform.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
    }

    void Start()
    {
        #if UNITY_EDITOR
        if (Application.isEditor) sprite = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Resources/MapSprites/" + sceneName + ".png", typeof(Texture2D));
        #endif
        if (!Application.isEditor) sprite = (Texture2D)Resources.Load("MapSprites/" + sceneName, typeof(Texture2D));

        mapImg.texture = sprite;

        UpdateUI();
    }

    public void UpdateUI()
    {
        button.transform.GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
        var colors = button.colors;
        colors.normalColor = (selected ? selectedColor : deselectedColor);
        colors.selectedColor = (selected ? selectedColor : deselectedColor);
        colors.disabledColor = (selected ? selectedColor : deselectedColor);
        button.colors = colors;

        transform.DOScale((selected ? Vector3.one : new Vector3(0.9f,0.9f,0.9f)), 0.3f).SetEase(Ease.OutBounce);
    }

    public void ChangeState()
    {
        selected = !selected;
        var colors = button.colors;
        colors.normalColor = (selected ? selectedColor : deselectedColor);
        colors.selectedColor = (selected ? selectedColor : deselectedColor);
        colors.disabledColor = (selected ? selectedColor : deselectedColor);
        button.colors = colors;
        mapSelectionScript.UpdateScenes();


        transform.DOScale((selected ? Vector3.one : new Vector3(0.9f,0.9f,0.9f)), 0.3f).SetEase(Ease.OutBounce);

    }

    public void SetState(bool state)
    {
        selected = state;
        var colors = button.colors;
        colors.normalColor = (selected ? selectedColor : deselectedColor);
        colors.selectedColor = (selected ? selectedColor : deselectedColor);
        colors.disabledColor = (selected ? selectedColor : deselectedColor);
        button.colors = colors;

        transform.DOScale((selected ? Vector3.one : new Vector3(0.9f,0.9f,0.9f)), 0.3f).SetEase(Ease.OutBounce);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FloatingName.Instance.nameToShow = (selected ? "Unselect map" : "Select map");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FloatingName.Instance.nameToShow = "";
    }
}
