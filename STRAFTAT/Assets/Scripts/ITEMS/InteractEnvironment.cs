using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public abstract class InteractEnvironment : NetworkBehaviour
{
    public virtual void Awake()
    {
        gameObject.layer = 19;
    }

    public string popupText;
    
    public abstract void OnInteract(Transform player);
    public abstract void OnFocus();
    public abstract void OnLoseFocus();
}
