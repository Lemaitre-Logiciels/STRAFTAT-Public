using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class ToggleEventInteract : BaseEventInteract {
    private bool IsToggled = false;

    [Header("These events trigger when the player toggles this object on.")]
    [FoldoutGroup("On Enable Events")] [SerializeField] private UnityEvent onEnableEvent;
    [FoldoutGroup("On Enable Events")] [SerializeField] private UnityEvent onEnableServerEvent;
    [Header("These events trigger when the player toggles this object off.")]
    [FoldoutGroup("On Disable Events")] [SerializeField] private UnityEvent onDisableEvent;
    [FoldoutGroup("On Disable Events")] [SerializeField] private UnityEvent onDisableServerEvent;
    
    [ObserversRpc]
    protected override void OnInteractObserverRpc() {
        IsToggled = !IsToggled;
        if (IsToggled) {
            if (IsServer) { onEnableServerEvent?.Invoke(); }
            onEnableEvent?.Invoke();
        }
        else {
            if (IsServer) { onDisableServerEvent?.Invoke(); }
            onDisableEvent?.Invoke();
        }
    }
}