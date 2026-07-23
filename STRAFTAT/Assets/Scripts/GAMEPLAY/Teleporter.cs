using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public bool dontTranslateRotation;
    [Space]
    public Transform teleportPoint;
    public Transform selfOrientation;
    public float propulsionPower = 10;
    public float propulsionDecel = 8;

    public float anglesDifference;

    private void Start()
    {
        if (anglesDifference == 0 && selfOrientation != null)
            anglesDifference = Mathf.Abs(selfOrientation.eulerAngles.y) - Mathf.Abs(teleportPoint.eulerAngles.y);
    }
}
