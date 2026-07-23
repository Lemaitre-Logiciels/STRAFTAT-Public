using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetAllBindings : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void ResetAll()
    {
        var bindings = GetComponentsInChildren<ReBindUI>();

        foreach (var bind in bindings)
        {
            bind.ResetBinding();
        }
    }
}
