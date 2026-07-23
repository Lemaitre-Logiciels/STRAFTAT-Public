using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slope : MonoBehaviour
{
    public Transform rearRayPos;
    public Transform frontRayPos;
    public LayerMask layerMask;

    private FirstPersonController controller;

    public float surfaceAngle;
    public bool uphill;
    public bool downhill;
    public bool flatSurface;

    void Awake()
    {
        controller = GetComponent<FirstPersonController>();
    }

    RaycastHit rearHit;
    bool rearCast;

    RaycastHit frontHit;
    bool frontCast;

    RaycastHit middleHit;
    bool middleCast;


    // Update is called once per frame
    void Update()
    {
        rearCast = Physics.Raycast(rearRayPos.position, -Vector3.up, out rearHit, Mathf.Infinity, layerMask);
        frontCast = Physics.Raycast(frontRayPos.position, -Vector3.up, out frontHit, Mathf.Infinity, layerMask);
        middleCast = Physics.Raycast(transform.position, -Vector3.up, out middleHit, Mathf.Infinity, layerMask);

        surfaceAngle = Vector3.Angle(middleHit.normal, Vector3.up);

        if(frontHit.distance < rearHit.distance)
        {
            flatSurface = false;
            uphill = true;
            downhill = false;
        }
        else if (frontHit.distance > rearHit.distance)
        {
            flatSurface = false;
            downhill = true;
            uphill = false;
        }
        else if (frontHit.distance == rearHit.distance)
        {
            flatSurface = true;
            uphill = false;
            downhill = false;
        }

    }
}
