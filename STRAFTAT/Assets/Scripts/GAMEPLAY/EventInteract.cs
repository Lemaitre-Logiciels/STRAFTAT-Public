using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class EventInteract : BaseEventInteract {
    [Header("These events trigger when the player interacts with this object.")]
    [FoldoutGroup("On Interact Events")] [SerializeField] private UnityEvent onInteractEvent;
    [FoldoutGroup("On Interact Events")] [SerializeField] private UnityEvent onInteractServerEvent;

    [ObserversRpc]
    protected override void OnInteractObserverRpc() {
        if (IsServer) { onInteractServerEvent?.Invoke(); }
        onInteractEvent?.Invoke();
    }
}