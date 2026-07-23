using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeConstrains : MonoBehaviour
{
    [SerializeField] private float constrain = 0.1f;
    public float baseSpeed = 3;
    public bool rotateBack;
    [SerializeField] private float highSpeed = 25;


    // Update is called once per frame
    void Update()
    {

        /*transform.localPosition = new Vector3(Mathf.Lerp(transform.localPosition.x, 0, ((transform.localPosition.x > constrain || transform.localPosition.x < -constrain ? highSpeed : baseSpeed) * Time.deltaTime)), 
            Mathf.Lerp(transform.localPosition.y, 0, ((transform.localPosition.y > constrain || transform.localPosition.y < -constrain  ? highSpeed : baseSpeed) * Time.deltaTime)), 
            Mathf.Lerp(transform.localPosition.z, 0, ((transform.localPosition.z > constrain || transform.localPosition.z < -constrain ? highSpeed : baseSpeed) * Time.deltaTime)));*/
        if (!rotateBack) return;
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, 0), baseSpeed * Time.deltaTime);
    }
}
