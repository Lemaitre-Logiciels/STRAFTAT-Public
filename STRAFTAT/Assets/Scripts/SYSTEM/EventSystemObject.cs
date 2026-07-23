using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystemObject : MonoBehaviour
{
    public static EventSystemObject Instance;
    void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}
