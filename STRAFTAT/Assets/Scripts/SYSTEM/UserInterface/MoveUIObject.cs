using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveUIObject : MonoBehaviour
{
    private Vector3 activePosition;
    [SerializeField] private Vector3 offset = new Vector3(3, 0, 0);
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private Ease ease = Ease.Linear;
    private bool state;
    bool initiated;
    // Start is called before the first frame update
    void Awake()
    {
        activePosition = transform.localPosition;
        transform.localPosition = activePosition + offset;
        state = false;
        initiated = true;
        transform.localPosition = activePosition + offset;
    }

    void OnEnable()
    {
        transform.localPosition = activePosition + offset;
        state = false;
    }

    void Update()
    {
    }

    [ContextMenu("ChangeState")]
    public void ChangeState()
    {
        state = !state;

        if (state == true) transform.DOLocalMove(activePosition, duration).SetEase(ease);
        else transform.DOLocalMove(activePosition + offset, duration).SetEase(ease);
    }

    public void ChooseState(bool state)
    {
        if (this.state == state) return;

        this.state = state;
        if (state == true) transform.DOLocalMove(activePosition, duration).SetEase(ease);
        else transform.DOLocalMove(activePosition + offset, duration).SetEase(ease);
    }
}
