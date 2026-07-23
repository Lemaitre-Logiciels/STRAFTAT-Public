using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine.InputSystem;

public class PlayerShoot : NetworkBehaviour
{
    public PlayerControls playerControls;
    private InputAction fire1, fire2;

    [SerializeField] private LayerMask playerLayer;

    public float timeBetweenFire = 0.2f;
    public int damage = 1;
    float fireTimer;

    Camera cam;


    void Awake()
    {
        playerControls = InputManager.inputActions;
        cam = transform.GetChild(0).GetComponent<Camera>();
    }

    private void OnEnable()
    {
        fire1 = playerControls.Player.LeftClick;
        fire1.Enable();
        fire1.performed += Fire;

        fire2 = playerControls.Player.RightClick;
        fire2.Enable();
        fire2.performed += Fire;
    }
    private void OnDisable()
    {
        fire1.Disable();
        fire1.performed -= Fire;

        fire2.Disable();
        fire2.performed -= Fire;
    }

    private void Update()
    {
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        if (!base.IsOwner) return;

        if (fireTimer > 0) return;

        ShootServer(damage, cam.transform.position, cam.transform.forward);

        fireTimer = timeBetweenFire;
    }

    [ServerRpc (RequireOwnership = false)]
    private void ShootServer(float damageToGive, Vector3 position, Vector3 direction)
    {
        if (Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity) && hit.transform.TryGetComponent(out PlayerHealth enemyHealth))
        {
            if (hit.transform.gameObject != gameObject) enemyHealth.health -= damageToGive;
        }
        if (Physics.Raycast(position, direction, out RaycastHit doll, Mathf.Infinity) && doll.transform.TryGetComponent(out DollHealth dollHealth))
        {
            dollHealth.health -= damageToGive;
        }
    }

    

}
