using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DlcStorePageButton : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private GameObject notOwnedImage;
    [SerializeField] private GameObject[] notOwnedObjects;

    [SerializeField] private GameObject ownedImage;
    [SerializeField] private GameObject ownedImage1;
    
    [SerializeField] private MeshRenderer screen;
    [SerializeField] private Material notOwnedMat;
    [SerializeField] private Material ownedMat;
    [SerializeField] private Material owned1Mat;


    private bool Dlc0Owned = false;
    private bool Dlc1Owned = false;
    
    void Update() {
        if (Dlc0Owned != SteamLobby.ownDlc0) { UpdateUI(); }
        if (Dlc1Owned != SteamLobby.ownDlc1) { UpdateUI(); }
    }

    private void UpdateUI() {
        Dlc0Owned = SteamLobby.ownDlc0;
        Dlc1Owned = SteamLobby.ownDlc1;
        
        if (!Dlc0Owned) {
            notOwnedImage.SetActive(true);
            ownedImage.SetActive(false);
            ownedImage1.SetActive(false);
            UpdateMaterial(notOwnedMat);
            foreach (GameObject obj in notOwnedObjects) { obj.SetActive(true); }
        } else if (Dlc1Owned) {
            notOwnedImage.SetActive(false);
            ownedImage.SetActive(false);
            ownedImage1.SetActive(true);
            UpdateMaterial(owned1Mat);
            foreach (GameObject obj in notOwnedObjects) { obj.SetActive(false); }
        } else {
            notOwnedImage.SetActive(false);
            ownedImage.SetActive(true);
            ownedImage1.SetActive(false);
            UpdateMaterial(ownedMat);
            foreach (GameObject obj in notOwnedObjects) { obj.SetActive(false); }
        }
    }
    
    private void UpdateMaterial(Material mat) {
        Material[] tempMats = screen.materials;
        tempMats[1] = mat;
        screen.materials = tempMats;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!SteamLobby.ownDlc0) { FloatingName.Instance.nameToShow = "Get the DLC !"; } 
        else if (SteamLobby.ownDlc1) { FloatingName.Instance.nameToShow = "Open the Supporter Edition Files"; } 
        else { FloatingName.Instance.nameToShow = "DLC Owned"; }
    }

    public void OnPointerExit(PointerEventData eventData) { FloatingName.Instance.nameToShow = ""; }
    
    public void OnClick() {
        if (!SteamLobby.ownDlc0) { SteamLobby.Instance.OpenDLCStorePage(); } 
        else if (SteamLobby.ownDlc1) {
            string dataPath = System.IO.Path.GetFullPath(Application.dataPath);
            string rootFolderDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(dataPath, ".."));
            string supporterEditionPath = System.IO.Path.Combine(rootFolderDirectory, "Straftat_SupporterEdition");
            OpenSpecificFolder(supporterEditionPath);
        } 
        else { SteamLobby.Instance.OpenSupporterDLCStorePage(); }
    }

    private void OpenSpecificFolder(string folderPath) {
        if (System.IO.Directory.Exists(folderPath)) { Process.Start(folderPath); }
        else { PauseManager.Instance.ShowInfoPopup($"Could not find the folder:\n{folderPath}"); }
    }
}
