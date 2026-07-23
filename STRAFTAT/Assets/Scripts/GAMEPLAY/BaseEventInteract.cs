using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseEventInteract : InteractEnvironment {
    // Prevent interaction before all the players have spawned.
    private bool IsReady = false;
    protected virtual void OnEnable() { PauseManager.OnRoundStarted += RoundStart; }
    protected virtual void OnDisable() { PauseManager.OnRoundStarted -= RoundStart; }
    private void RoundStart() { IsReady = true; }

    // This is necessary cuz fishnet is dumb.
    public override void Awake() { base.Awake(); }

    // OnInteract
    public override void OnInteract(Transform player) {
        if (!IsReady) { return; }
        OnInteractServerRpc();
    }
    [ServerRpc(RequireOwnership = false)] private void OnInteractServerRpc() { OnInteractObserverRpc(); }
    protected abstract void OnInteractObserverRpc();
    
    // OnFocus
    [Header("These events trigger when the player focuses on this object.")]
    [FoldoutGroup("On Focus Events")] [SerializeField] private UnityEvent onFocusEvent;
    [FoldoutGroup("On Focus Events")] [SerializeField] private UnityEvent onFocusServerEvent;
    public override void OnFocus() {
        if (!IsReady) { return; }
        PauseManager.Instance.interactPopup.gameObject.SetActive(true);
        PauseManager.Instance.interactPopup.text = $"{popupText} [{PauseManager.Instance.InteractPromptLetter.ToLower()}]";
        OnFocusServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnFocusServerRpc() {
        onFocusServerEvent?.Invoke();
        OnFocusObserverRpc();
    }
    [ObserversRpc] private void OnFocusObserverRpc() { onFocusEvent?.Invoke(); }
    
    // OnLoseFocus
    [Header("These events trigger when the player loses focus on this object.")]
    [FoldoutGroup("On Lose Focus Events")] [SerializeField] private UnityEvent onLoseFocusEvent;
    [FoldoutGroup("On Lose Focus Events")] [SerializeField] private UnityEvent onLoseFocusServerEvent;
    public override void OnLoseFocus() {
        if (!IsReady) { return; }
        OnLoseFocusServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void OnLoseFocusServerRpc() {
        onLoseFocusServerEvent?.Invoke();
        OnLoseFocusObserverRpc();
    }
    [ObserversRpc] private void OnLoseFocusObserverRpc() { onLoseFocusEvent?.Invoke(); }
}
