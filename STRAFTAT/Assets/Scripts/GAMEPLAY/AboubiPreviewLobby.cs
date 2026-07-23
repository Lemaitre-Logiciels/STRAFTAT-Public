using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using DG.Tweening;

public class AboubiPreviewLobby : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 20;
    [SerializeField] public GameObject previewObject;
    [SerializeField] private GameObject[] meshesToChange;
    [SerializeField] private Transform hatToWearPosition;
    GameObject currentHat;
    GameObject currentCig;

    SteamLobby steamLobbyScript;

    public GameObject parentObj;
    Vector3 initialPosition;
    Vector3 initialDirection;

    void Start()
    {
        steamLobbyScript = SteamLobby.Instance;
        initialPosition = transform.position;
        initialDirection = transform.forward;
    }

    public void ChangeDress(GameObject hat, Material mat, GameObject cig)
    {
        if (hat != null){
            if (currentHat != null) Destroy(currentHat);
            currentHat = Instantiate(hat, hatToWearPosition.position, hatToWearPosition.rotation, hatToWearPosition);
            currentHat.AddComponent<HatPosition>();
            currentHat.GetComponent<HatPosition>().reference = hatToWearPosition;
            currentHat.transform.localPosition = Vector3.zero;
            Destroy(currentHat.GetComponent<NetworkObject>());
            currentHat.SetActive(true);
            SetGameLayerRecursive(currentHat, 23);
            //currentHat.transform.forward = transform.forward; 
        } 
        if (cig != null){
            if (currentCig != null) Destroy(currentCig);
            currentCig = Instantiate(cig, hatToWearPosition.position, hatToWearPosition.rotation, hatToWearPosition);
            currentCig.AddComponent<HatPosition>();
            currentCig.GetComponent<HatPosition>().reference = hatToWearPosition;
            currentCig.transform.localPosition = Vector3.zero;
            Destroy(currentCig.GetComponent<NetworkObject>());
            currentCig.SetActive(true);
            SetGameLayerRecursive(currentCig, 23);
        } 
        if (mat != null)
        {
            foreach (var obj in meshesToChange)
            {
                obj.GetComponent<SkinnedMeshRenderer>().material = mat;
            }
        }
    }

    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;
 
            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);
         
        }
    }

    void Update() {
        if (isRunning) transform.forward = -Vector3.forward;
        else transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        
        if (parentObj == null && previewObject.activeSelf) { previewObject.SetActive(false); }

        if (!meshesToChange[0].GetComponent<SkinnedMeshRenderer>().enabled) {
            foreach (var obj in meshesToChange)
            {
                obj.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }
    }

    bool isRunning;

    public IEnumerator RunIntoLobby()
    {
        isRunning = true;
        
        transform.position = initialPosition + Vector3.forward * 6;

        transform.DOMove(initialPosition, 1);

        yield return new WaitForSeconds(1);
        isRunning = false;
    }
}
