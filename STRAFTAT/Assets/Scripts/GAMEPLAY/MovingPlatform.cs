using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private bool needPlayerGrounded;
    /*void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            entered = true;

            if (!needPlayerGrounded) {
                other.transform.SetParent(transform);
                other.GetComponent<FirstPersonController>().onMovingPlatform = true;
                other.GetComponent<FirstPersonController>().CmdChangeRootMotion(false);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && entered)
        {
            if (!other.GetComponent<FirstPersonController>().isGrounded) return;
            
            entered = false;
            other.transform.SetParent(transform);
            other.GetComponent<FirstPersonController>().onMovingPlatform = true;
            other.GetComponent<FirstPersonController>().CmdChangeRootMotion(false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
            other.GetComponent<FirstPersonController>().onMovingPlatform = false;
            other.GetComponent<FirstPersonController>().CmdChangeRootMotion(true);
        }
    }*/
    
}
