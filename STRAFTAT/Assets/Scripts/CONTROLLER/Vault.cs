using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    [SerializeField] private float lerpTime = 0.4f;
    [SerializeField] private float vaultHeight = 1;
    [SerializeField] private LayerMask vaultLayer;
    public bool vaulting;
    public Transform cam;
    private float playerHeight = 4f;
    private float playerRadius = 0.5f;

    private CharacterController controller;

    void Start(){

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vaulting();
    }
    private void Vaulting()
    {
        if (controller.isGrounded) return;

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out var firstHit, 2f, vaultLayer) && Input.GetAxisRaw("Vertical") > 0)
        {
            if (Vector3.Angle(firstHit.normal, Vector3.up) < 87 || Vector3.Angle(firstHit.normal, Vector3.up) > 93) return;
            print("vaultable in front");
            if (Physics.Raycast(firstHit.point + (transform.forward) + (Vector3.up * vaultHeight), Vector3.down, out var secondHit, playerHeight))
            {
                print("found place to land");
                StartCoroutine(LerpVault(secondHit.point, lerpTime));
            }
        }

    }
    IEnumerator LerpVault(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        vaulting = true;

        while (time < duration -0.1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            if (GetComponent<CharacterController>().velocity.magnitude == 0) break;
            yield return null;
        }

        vaulting = false;
    }

    Vector3 Caca(Vector3 vector3, int decimalPlaces)
	{
		float multiplier = 1;
		for (int i = 0; i < decimalPlaces; i++)
		{
			multiplier *= 10f;
		}
		return new Vector3(
			Mathf.Round(vector3.x * multiplier) / multiplier,
			Mathf.Round(vector3.y * multiplier) / multiplier,
			Mathf.Round(vector3.z * multiplier) / multiplier);
	}
}