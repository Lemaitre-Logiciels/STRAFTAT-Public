using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatPosition : MonoBehaviour
{
    public Transform reference;
    

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null || reference == null) return;
        transform.position = reference.position;
        transform.rotation = reference.rotation;

    }
}
