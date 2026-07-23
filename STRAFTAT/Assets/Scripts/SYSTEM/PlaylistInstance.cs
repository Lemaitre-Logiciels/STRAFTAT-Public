using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlaylistInstance : MonoBehaviour {
    public string name;
    public int index;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private TMP_InputField nameInputField;
    public Button button;

    void Start()
    {
        text.text = name;
        nameInputField = GetComponentInChildren<TMP_InputField>();
    }

    public void DeletePlaylist()
    {
        Destroy(gameObject);
        MapsManager.Instance.RemovePlaylist(index);
    }

    public void OpenPlaylist() {
        MapsManager.Instance.PopulateMapsFromPlayList(index);
        MapsManager.Instance.currentPlaylistText.text = name;
        MapsManager.Instance.selectedPlaylistIndex = index;
    }

    public void DuplicatePlaylist() {
        MapsManager.Instance.DupePlaylist(index);
    }

    public void RenamePlaylist()
    {
        if (EventSystem.current.currentSelectedGameObject == nameInputField) EventSystem.current.SetSelectedGameObject(null);
        else nameInputField.Select();
    }

    public void UpdateName(TMP_InputField field)
    {
        if (field.text != "") 
        {
            name = field.text;
            MapsManager.Instance.Playlists[index].Name = name;
        }
    }

    string temptext;

    public void OnSelect(TMP_InputField input)
    {
        temptext = name;
        text.text = "";
    }

    public void OnDeselect(TMP_InputField input)
    {text.text = temptext;

    }
}
