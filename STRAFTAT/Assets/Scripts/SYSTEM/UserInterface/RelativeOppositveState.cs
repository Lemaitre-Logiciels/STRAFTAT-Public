using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeOppositveState : MonoBehaviour
{
    [SerializeField] private Transform obj;

    // Update is called once per frame
    void Update()
    {
        transform.localScale = (obj.localScale == Vector3.one ? Vector3.zero : Vector3.one);
    }
}
