using FishNet.Object;
using UnityEngine;

/// <summary>
/// This makes it so we only have to call InputManager.RestartEngine() once not 4 times per player init.
/// </summary>
[DefaultExecutionOrder(-1000)] // Ensure this runs before any other scripts that might use InputManager
public class InputReset : MonoBehaviour {
    public void Awake() { InputManager.RestartEngine(); }
}