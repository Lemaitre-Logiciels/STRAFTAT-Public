using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ModelButtonTween : MonoBehaviour
{
    [SerializeField] private float hoverAmount = 0.15f;
    [SerializeField] private float pressAmount = 0.3f;
    [SerializeField] private float tweenDuration = 0.04f;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private Vector3 direction;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;

        if (direction == Vector3.zero) direction = Vector3.forward;
    }

    public void Hover()
    {
        transform.DOLocalMove(initialPosition - direction * hoverAmount, tweenDuration);
    }

    public void Leave()
    {
        transform.DOLocalMove(initialPosition, tweenDuration);
    }

    public void Press()
    {
        transform.DOLocalMove(initialPosition - direction * pressAmount, tweenDuration);
    }

    public void Release()
    {
        transform.DOLocalMove(initialPosition - direction * hoverAmount, tweenDuration);
    }

    void OnDisable()
    {
        if (initialPosition != Vector3.zero) transform.localPosition = initialPosition;
    }
}
