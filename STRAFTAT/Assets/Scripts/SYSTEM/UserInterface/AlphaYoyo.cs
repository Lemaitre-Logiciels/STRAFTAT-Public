using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlphaYoyo : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private float floorLimit = 0.5f;
    [SerializeField] private float ceilLimit = 1;
    TextMeshProUGUI text;
    float alpha;
    float timer;
    bool enabled = true;
    Color initialColor;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        initialColor = text.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;
        timer += Time.deltaTime * speed;
        alpha = Mathf.Lerp(floorLimit, ceilLimit, Mathf.Abs(Mathf.Cos(timer)));


        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }

    public void InvertState()
    {
        enabled = !enabled;

        if (!enabled) text.color = initialColor;
    }

    public void SetState(bool state)
    {
        enabled = state;

        if (!enabled) text.color = initialColor;
    }
}
