using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveFromRef : MonoBehaviour
{
    [SerializeField] private GameObject reference;
    [SerializeField] private Vector3 openScale;

    // Update is called once per frame
    void Update()
    {
        transform.localScale = (reference.activeSelf ? openScale : Vector3.zero);
    }
}
