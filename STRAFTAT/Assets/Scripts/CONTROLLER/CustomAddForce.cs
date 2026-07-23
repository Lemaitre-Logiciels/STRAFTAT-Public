using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAddForce : MonoBehaviour
{
    [SerializeField] float mass = 3.0F; // defines the character mass
    Vector3 impact = Vector3.zero;
    private CharacterController character;
    private FirstPersonController controller;

    [SerializeField] private float airdeceleration = 0.5f;
    [SerializeField] private float deceleration = 5;
    // Use this for initialization
    void Start () {
    	character = GetComponent<CharacterController>();
    	controller = GetComponent<FirstPersonController>();
    }
    
    // Update is called once per frame
    void Update () {
     // apply the impact force:
        if (impact.magnitude > 0.2F) controller.customForceFinal = impact;
        else controller.customForceFinal = Vector3.zero;

        if (controller.isGrounded) impact.y = 0;

    	// consumes the impact energy each cycle:
    	impact = Vector3.Lerp(impact, Vector3.zero, (!controller.isGrounded ? airdeceleration : deceleration) *Time.deltaTime);
    }
    // call this function to add an impact force:
    public void AddForce(Vector3 dir, float force){
        dir.Normalize();
        impact += dir.normalized * force / mass;
    }
}
