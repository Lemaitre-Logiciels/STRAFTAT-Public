using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimation : MonoBehaviour
{
    [SerializeField] private Transform cube;
    [SerializeField] private float speed = -16;
    [SerializeField] private Vector3 axis;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cube.Rotate(axis * speed * Time.deltaTime);
    }
}
