using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlingTrigger : MonoBehaviour
{
    public float cooldown = 2.0f;
    float cooldownTimer = 0.0f;
    bool active = true;

    public Pig pig;

    private void Update()
    {
        if (!active)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer > cooldown)
            {
                active = true;
                cooldownTimer = 0.0f;
            }
        }
    }

    Collider c;

    private void OnTriggerEnter(Collider other)
    {
        if (!active)
            return;

        if (other.gameObject.tag == "Player")
        {
            c = other;

            pig.Fling();

            Invoke("Fling", 0.25f);

            active = false;
        }
    }

    void Fling()
    {
        if (c == null)
            return;

        c.gameObject.GetComponent<FirstPersonController>().AddVerticalForce(Vector3.up, 25f);
    }
}
