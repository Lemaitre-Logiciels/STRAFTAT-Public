using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

public class DollHealth : NetworkBehaviour
{
    public float health = 4;
    private float maxHealth;
    [SerializeField] private InputAction menu;

    void Start()
    {
        maxHealth = health;
    }

    private void OnEnable()
    {
        menu.Enable();
        menu.performed += Menu;
    }
    private void OnDisable()
    {
        menu.Disable();
        menu.performed -= Menu;
    }

    void Update()
    {
        if (health < 0)
        {
            DollDeath();
        }
    }

    void Menu(InputAction.CallbackContext ctx)
    {
        health = maxHealth;
        DollRevive();
    }

    [ServerRpc (RequireOwnership = false)]
    private void ServerDollDeath()
    {
        DollDeath();
    }

    [ServerRpc (RequireOwnership = false)]
    private void ServerDollRevive()
    {
        DollRevive();
    }

    [ObserversRpc]
    private void DollDeath()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    [ObserversRpc]
    private void DollRevive()
    {
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
