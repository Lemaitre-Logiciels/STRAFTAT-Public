using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    [SerializeField] private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            anim.SetBool("Crouch", false);
            anim.SetBool("Slide", false);
            anim.SetFloat("MovementSpeed", 0);
            anim.SetFloat("crouchMove", 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anim.SetBool("Crouch", true);
            anim.SetBool("Slide", false);
            anim.SetFloat("MovementSpeed", 0);
            anim.SetFloat("crouchMove", 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            anim.SetBool("Crouch", false);
            anim.SetBool("Slide", true);
            anim.SetFloat("MovementSpeed", 0);
            anim.SetFloat("crouchMove", 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            anim.SetBool("Crouch", false);
            anim.SetBool("Slide", false);
            anim.SetFloat("MovementSpeed", 0.5f);
            anim.SetFloat("crouchMove", 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            anim.SetBool("Crouch", false);
            anim.SetBool("Slide", false);
            anim.SetFloat("MovementSpeed", 1);
            anim.SetFloat("crouchMove", 0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            anim.SetBool("Crouch", true);
            anim.SetBool("Slide", false);
            anim.SetFloat("MovementSpeed", 0);
            anim.SetFloat("crouchMove", 1);
        }
    }
}
