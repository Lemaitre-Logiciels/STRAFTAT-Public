using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DF_SkyRotation : MonoBehaviour {
    public float skyboxRotationX = 1.5f;
    void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxRotationX);
        transform.Rotate(0, -(Time.deltaTime * skyboxRotationX), 0);
    }
}
