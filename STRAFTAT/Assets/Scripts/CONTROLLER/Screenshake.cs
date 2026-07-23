using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshake : MonoBehaviour
{
    public bool start;

    public float smallFallDuration = 1f;
    public float bigFallDuration = 1f;
    [HideInInspector] public float duration = 1f;
    
    public AnimationCurve smallCurve;
    public AnimationCurve bigCurve;
    [HideInInspector] public AnimationCurve strengthCurve;

    private float timer;
    private Vector3 startingPos;

    void Start()
    {
        startingPos = Vector3.zero;
    }

    void Update() {
        timer -= Time.deltaTime;
        if (start) {
            timer = duration;
            start = false;
            StartCoroutine(Shaking());
        }

        if (timer < 0) transform.localPosition = Vector3.Lerp(transform.localPosition, startingPos, 4f * Time.deltaTime);
    }

    IEnumerator Shaking() {
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float strength = strengthCurve.Evaluate(elapsedTime / duration);
            transform.localPosition = startingPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.localPosition = startingPos;
    }
}
