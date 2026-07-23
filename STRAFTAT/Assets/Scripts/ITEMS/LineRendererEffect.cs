using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererEffect : MonoBehaviour
{
    private LineRenderer line;
    [SerializeField] private Color colorOne;
    [SerializeField] private Color colorTwo;
    float timer;
    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            if (line.startColor == colorOne)
            {
            }
        }
    }
}
