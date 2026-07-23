using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScaleTweenLoop : MonoBehaviour
{
    [SerializeField] private Ease ease;
    [SerializeField] private float time = 0.5f;
    [SerializeField] private float size = 1.5f;
    [SerializeField] private int loopsAmount = 2;
    [SerializeField] private bool loopOnStart;
    

    // Update is called once per frame
    void Start()
    {
        if (loopOnStart) transform.DOScale(transform.localScale * size, time).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
    }

    public void TempTween()
    {
        transform.DOScale(transform.localScale * size, time).SetEase(ease).SetLoops(loopsAmount, LoopType.Yoyo);
    }
}
