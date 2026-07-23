using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeDetectionFix : MonoBehaviour
{
    [SerializeField] private GameObject cacouc;
    [SerializeField] private float dir = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = cacouc.transform.localPosition + cacouc.transform.forward * dir;
    }
}
