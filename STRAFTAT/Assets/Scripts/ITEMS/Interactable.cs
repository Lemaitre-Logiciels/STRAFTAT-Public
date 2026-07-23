using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;

public abstract class Interactable : NetworkBehaviour
{
    public virtual void Awake()
    {
        gameObject.layer = 7;
        canTake = true;
    }
    
    public abstract void OnInteract();
    public abstract void OnFocus();
    public abstract void OnLoseFocus();

    public bool canTake;
}
