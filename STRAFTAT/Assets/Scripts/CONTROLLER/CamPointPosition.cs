using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPointPosition : MonoBehaviour
{
    [SerializeField] private Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = cam.transform.position + cam.transform.forward * 2;
    }
}
