using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private float maxDistance = 8;
    [SerializeField] private float minDistance = 5;

    Transform target;
    TMP_Text text;
    Vector3 initScale;

    // Start is called before the first frame update
    void Start()
    {
        initScale = transform.localScale;
        transform.localScale = new Vector3(-initScale.x, initScale.y, initScale.z);
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) {
            if (GameObject.FindGameObjectWithTag("Player") != null)
                target = GameObject.FindGameObjectWithTag("Player").transform;
        } 

        if (target == null) return;

        Vector3 targetPostition = new Vector3( target.position.x, 
                                       transform.position.y, 
                                       target.position.z ) ;
        transform.LookAt( targetPostition ) ;

        Color targetColor = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(0, 1, (1 - ((Vector3.Distance(target.position, transform.position)-minDistance) / maxDistance) )));

        text.color = targetColor;


    }
}
