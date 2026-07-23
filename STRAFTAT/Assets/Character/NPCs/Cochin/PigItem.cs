using FishNet.Component.ColliderRollback.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigItem : MonoBehaviour
{
    SkinnedMeshRenderer visual;

    Animator anim;

    public GameObject pigReal;
    public GameObject hatHeld;
    MeshRenderer hatMesh;

    float angerTimeMin = 3f;
    float angerTimeMax = 5f;
    float angerTime = 0.0f;
    float angerTimer = 0.0f;

    void Awake() {
        //transform.parent = null;
        visual = GetComponentInChildren<SkinnedMeshRenderer>();
        visual.enabled = false;
    }

    void Start()
    {
        Physics.IgnoreCollision(pigReal.GetComponent<Collider>(), GetComponent<Collider>());
        anim = GetComponent<Animator>();
        hatMesh = hatHeld.GetComponent<MeshRenderer>();
        hatMesh.enabled = false;
        SetDropped();
        Debug.Log("caca");
    }

    bool held = false;

    void SetHeld()
    {
        pigReal.SetActive(false);
        visual.enabled = true;
        transform.localRotation = Quaternion.Euler(3.1f, 30.0f, 0.0f);
        transform.localPosition = new Vector3(0.15f, -0.759f, 1.199f);
        angerTime = Random.Range(angerTimeMin, angerTimeMax);

        if (hatHeld.activeSelf)
            hatMesh.enabled = true;

        held = true;
    }

    void SetDropped()
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = true;
        visual.enabled = false;
        pigReal.transform.position = transform.position;
        pigReal.SetActive(true);
        if (hatHeld.activeSelf)
            hatMesh.enabled = false;

        held = false;
    }

    void Update()
    {
        if (pigReal == null) Destroy(this);
        if (transform.root.CompareTag("Player"))
        {
            if (!held)
                SetHeld();
        }
        else
        {
            if (held)
                SetDropped();
        }
    }

    private void LateUpdate()
    {
        if (!held)
        {
            transform.position = pigReal.transform.position;
        }
        else
        {
            angerTimer += Time.deltaTime;

            if (angerTimer > angerTime)
            {
                Anger();
                angerTimer = 0;
            }
        }
    }

    void Anger()
    {
        anim.SetTrigger("HeldAngry");
    }
}
