using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class AboubiPreview : MonoBehaviour
{
    public static AboubiPreview Instance;
    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private float rotateSpeed = 20;
    [SerializeField] private GameObject[] meshesToChange;
    [SerializeField] private Transform hatToWearPosition;
    GameObject currentHat;
    GameObject currentCig;

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

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
