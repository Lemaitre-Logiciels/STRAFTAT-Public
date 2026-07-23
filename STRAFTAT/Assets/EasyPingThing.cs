using FishNet;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class EasyPingThing : MonoBehaviour {
    [SerializeField] private string prefix = "Ping: ";
    [SerializeField] private string suffix = " ms";
    private TextMeshProUGUI textMesh;
    void Awake() { textMesh = GetComponent<TextMeshProUGUI>(); }
    void Update() { textMesh.text = prefix + InstanceFinder.TimeManager.RoundTripTime + suffix; }
}
