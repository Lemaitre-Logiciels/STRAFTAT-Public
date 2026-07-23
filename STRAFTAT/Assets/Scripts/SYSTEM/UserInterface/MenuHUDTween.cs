using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MenuHUDTween : MonoBehaviour
{
    public bool displayed;
    [Space]
    [SerializeField] private Transform hiddenPosition;
    [SerializeField] private Transform displayedPosition;
    [Space]
    [SerializeField] private Ease easeType;
    [SerializeField] private float lerpTime = 0;

    void Start()
    {
        if (displayed) {
            transform.localScale = Vector3.one;
            transform.position = displayedPosition.position;
        }
        else {
            transform.localScale = Vector3.zero;
            transform.position = hiddenPosition.position;
        }
    }

    void Update()
    {
        if (transform.position == hiddenPosition.position) transform.localScale = Vector3.zero;
        else transform.localScale = Vector3.one;
    }

    public void ChangeState()
    {
        transform.DOKill();

        if (displayed)
        {
            transform.DOMove(hiddenPosition.position, lerpTime).SetEase(easeType);
        }
        else
        {
            transform.DOMove(displayedPosition.position, lerpTime).SetEase(easeType);
        }

        displayed = !displayed;
    }

    public void SetEnabled()
    {
        transform.DOKill();
        transform.DOMove(displayedPosition.position, lerpTime).SetEase(easeType);
        displayed = true;
    }

    public void SetDisabled()
    {
        transform.DOKill();
        transform.DOMove(hiddenPosition.position, lerpTime).SetEase(easeType);
        displayed = false;
    }
}
