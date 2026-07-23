using UnityEngine;

public class Pig : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;

    float speed = 1.0f;
    float speedLimit = 2.0f;

    float walkTimeMin = 1f;
    float walkTimeMax = 2f;


    float walkTime = 0.0f;
    float walkTimer = 0.0f;

    Vector3 walkDir;

    bool stopped = false;

    float stoppedTimeMin = 2f;
    float stoppedTimeMax = 5f;

    float stoppedTime = 0.0f;
    float stoppedTimer = 0.0f;

    float sniffChance = 0.2f;
    float gruntChance = 0.4f;

    float walkGruntChance = 0.2f;
    bool walkGrunted = false;

    AudioSource sound;
    public AudioClip squeal;
    public AudioClip grunt;
    public AudioClip grunt2;
    public AudioClip sniff;
    public AudioClip fallFar;
    public AudioClip step;

    bool fellFar = false;


    Vector3 lastTrueVel;

    float stepTimer = 0.0f;
    float stepTime = 0.5f;

    //TODO
    //Wall stuck detection
    //Weird rotation fix
    //Damage + sfx/anim reaction + blood spawn
    //Repulsor blast?


    //anim + held anger anim
    //Pick up and drop use case?

    public GameObject hat;
    public GameObject hatHeld;

    private void OnDisable()
    {
        
    }

    private void OnEnable()
    {
        StopWalking();
        Grunt();
    }

    void Awake() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        sound = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartWalking();

        float rng = Random.Range(0f, 1000f);

        if (rng < 1f)
        {
            hat.SetActive(true);
            hatHeld.SetActive(true);
        }
    }

    public void Interact()
    {
        //???
    }

    public void Fling()
    {
        StopWalking();
        sound.pitch = 1f + Random.Range(-0.1f, 0.1f);
        sound.PlayOneShot(squeal);
        anim.SetTrigger("Fling");
    }


    public void Grunt()
    {
        float rng = Random.Range(0.0f, 1f);

        sound.pitch = 1f + Random.Range(-0.1f, 0.1f);

        if (rng < 0.5f)
            sound.PlayOneShot(grunt);
        else
            sound.PlayOneShot(grunt2);
    }

    public void Sniff()
    {
        sound.pitch = 1f + Random.Range(-0.1f, 0.1f);
        sound.PlayOneShot(sniff);
    }

    void FixedUpdate()
    {
        //fall joke
        if (rb.velocity.y < -20.0f && !fellFar)
        {
            FallFar();
        }

        //State Logic

        if (!stopped) //MOVING
        {
            walkTimer += Time.deltaTime;

            if (walkTimer > walkTime * 0.5 && !walkGrunted)
            {
                float rng = Random.Range(0.0f, 1f);

                if (rng < walkGruntChance)
                {
                    Grunt();
                }
                walkGrunted = true;
            }

            if (walkTimer > walkTime)
            {
                StopWalking();
            }

            stepTimer += Time.deltaTime;

            if (stepTimer > stepTime)
            {
                sound.pitch = 1f + Random.Range(-0.1f, 0.1f);
                sound.PlayOneShot(step);
                stepTimer = 0.0f;
            }

            //if not moving very much (on wall or slope etc) try walking elsewhere
            //if (rb.velocity.magnitude < 0.2f && walkTimer > walkTime * 0.75)
            //{
            //    StartWalking();
            //}

            rb.AddForce(walkDir * -speed, ForceMode.Impulse);

            //Speed clamp
            if (rb.velocity.magnitude > speedLimit)
            {
                Vector3 newVel = rb.velocity.normalized * speedLimit;
                newVel.y = rb.velocity.y;
                rb.velocity = newVel;
            }

            //Rotate while walking
            if (rb.velocity.magnitude > 0.1f)
            {
                Orient(rb.velocity * -1);
            }

            //store vel for stationary rotation later
            if (rb.velocity.magnitude > 0.5)
            {
                lastTrueVel = rb.velocity * -1f;
            }
        }
        else //STOPPED
        {
            stoppedTimer += Time.deltaTime;

            if (stoppedTimer > stoppedTime)
            {
                StartWalking();
            }

            Orient(lastTrueVel);
        }
    }

    public void FallFar()
    {
        sound.spatialBlend = 0.6f;
        sound.PlayOneShot(fallFar);
        anim.SetTrigger("Fling");
        fellFar = true;
        enabled = false;
        Invoke("Die", 4.0f);
    }


    void Orient(Vector3 travelDir)
    {
        Quaternion rotation = Quaternion.LookRotation(travelDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2.5f);
    }

    void StartWalking()
    {
        walkDir = new Vector3(Random.Range(-1f, 1f), 0.0f, Random.Range(-1f, 1f));
        walkTime = Random.Range(walkTimeMin, walkTimeMax);
        walkTimer = 0.0f;
        stopped = false;
        anim.ResetTrigger("Sniff");
        anim.SetBool("Walking", true);
        walkGrunted = false;
        stepTimer = 0.0f;
    }

    void StopWalking()
    {
        walkTimer = 999.0f;
        Vector3 newVel = rb.velocity;
        newVel.x = newVel.z = 0.0f;
        rb.velocity = newVel;
        stoppedTime = Random.Range(stoppedTimeMin, stoppedTimeMax);
        stoppedTimer = 0.0f;
        stopped = true;
        anim.SetBool("Walking", false);

        float rng = Random.Range(0.0f, 1f);
        if (rng < gruntChance)
        {
            if (rng < sniffChance)
            {
                anim.SetTrigger("Sniff");
                Invoke("Sniff", 0.3f);
            }
            else
            {
                anim.SetTrigger("Greet");
                Invoke("Grunt", 1.0f);
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        //Instantiate(vfx, transform.position, Quaternion.Euler(-90, 0, 0));
    }
}
