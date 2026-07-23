using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public GameObject toDestroy;
    public float damageAmount = 3.5f;
    public float damageInterval = 3.5f;

    void OnEnable()
    {
        PauseManager.OnBeforeSpawn += StartNewRound;
    }

    void OnDisable()
    {
        PauseManager.OnBeforeSpawn -= StartNewRound;
    }

    void StartNewRound()
    {
        Destroy(toDestroy);
    }

}