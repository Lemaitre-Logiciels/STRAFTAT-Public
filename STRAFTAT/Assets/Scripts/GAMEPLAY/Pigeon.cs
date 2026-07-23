using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public class Pigeon : NetworkBehaviour
{
    [SerializeField] private GameObject dieVfx;
    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private float maxDistance = 5;
    [SerializeField] private float flyTime = 20;
    [SerializeField] private float flySpeed = 2;
    [SerializeField] private float verticalMovement = 1;
    [SerializeField] private float flyTimer;
    [SerializeField] private bool randomRot = true;
    [SerializeField] private bool animate = true;
    [SerializeField] private Animator anim;
    private GameManager gameManager;
    private float walkTimer;

    private bool fly;
    private bool walk;

    private Vector3 initPos;
    private Vector3 flyDir;
    private Vector3 rotation;

    private GameObject player;

    [SerializeField] private float jumpTimer = 3;
    private float value;


    [SerializeField] private AudioClip flyClip;
    // Start is called before the first frame update

    void Start()
    {
        jumpTimer = Random.Range(1,2f);

        if (randomRot) transform.rotation = Quaternion.Euler(0, Random.Range(0, 360),0);
        initPos = transform.position;

        StartCoroutine(UpdatePlayer());
    }

    IEnumerator UpdatePlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var caca in players)
        {
            if (caca.GetComponent<FirstPersonController>().IsOwner)
                player = caca;
        }

        yield return new WaitForSeconds(2);

        StartCoroutine(UpdatePlayer());
    }

    public void Die()
    {
        Instantiate(dieVfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);

        flyTimer -= Time.deltaTime;

        walkTimer = Mathf.Sin(Time.time) * Random.Range(1,1);

        anim.SetBool("fly", fly);
        
        if (distanceFromPlayer < maxDistance)
        {
            if (!fly)
            {
                SoundManager.Instance.PlaySound(flyClip);
                flyDir = -(player.transform.position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(flyDir);
                fly = true;
                flyTimer = flyTime;
            }

        }

        if (fly)
        {
            transform.position += new Vector3(flyDir.x * Time.deltaTime * flySpeed, verticalMovement * Time.deltaTime, flyDir.z * Time.deltaTime * flySpeed);
            
        }
        else if (animate)
        {
            jumpTimer -= Time.deltaTime;

            if (jumpTimer < 0.1f && jumpTimer > 0) {
                value = Random.Range(-1, 1);
                jumpTimer = 0;
            }

            if (jumpTimer < 0) {
                transform.Rotate(0, value > 0 ? 290 * Time.deltaTime : -290 * Time.deltaTime, 0);

                if (jumpTimer > -0.2f) transform.position += new Vector3(0, 2 * Time.deltaTime, 0);
                else if (jumpTimer > -0.35f) transform.position -= new Vector3(0, 2 * Time.deltaTime, 0);
                else jumpTimer = Random.Range(2, 8f);
            }
        }

        if (flyTimer < 0 && Vector3.Distance(initPos, player.transform.position) > 20)
        {
            fly = false;
            transform.position = initPos;
        }

        
    }

}
