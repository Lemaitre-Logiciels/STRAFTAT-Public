using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class HealthTween : MonoBehaviour
{
    public float health;
    [SerializeField] private Color firstColor;
    [SerializeField] private Color lastColor;
    [Space]
    [SerializeField] private Vector3 sizeTween = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private int vibrato;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Transform heart;
    [SerializeField] private float timeBetweenHeartBeat = 1;
    [SerializeField] private float heartScale = 1.09f;
    [SerializeField] private float littleHeartScale = 1.04f;
    [SerializeField] private float heartSpeed = 0.08f;
    float timeBetweenHeartBeatFunc;
    float timer;

    void Start()
    {
        timer = timeBetweenHeartBeat;
        timeBetweenHeartBeatFunc = timeBetweenHeartBeat;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        text.text = Mathf.Ceil((health/4) * 100).ToString();

        if (timer < 0)
        {
            timer = timeBetweenHeartBeatFunc;
            heart.DOPunchScale(new Vector3(heartScale,heartScale,heartScale), heartSpeed).OnComplete(() =>
            {
                heart.DOPunchScale(new Vector3(littleHeartScale,littleHeartScale,littleHeartScale), heartSpeed);
            });
        }

        timeBetweenHeartBeatFunc = Mathf.Lerp(timeBetweenHeartBeat, 0.43f, 1 - health/4);
    }


    // Update is called once per frame
    public void ChangeState()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(sizeTween, duration);

        var lerpedColor = Color.Lerp(firstColor, lastColor, 1 - health/4);

        text.color = lerpedColor;
        text.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, lerpedColor);
        text.outlineColor = lerpedColor;
    }
}
