using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPersist : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
