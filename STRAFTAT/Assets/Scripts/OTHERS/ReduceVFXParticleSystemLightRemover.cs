using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
#endif

public class ReduceVFXParticleSystemLightRemover : MonoBehaviour {
    [SerializeField] private ParticleSystem[] particles;
    
    private void Awake() {
        if (!Settings.Instance.reduceVFX) { return; }
        
        foreach (ParticleSystem ps in particles) {
            // Dw tyou don't need to asign back https://docs.unity3d.com/ScriptReference/ParticleSystem-main.html unity is scuffed 
            ParticleSystem.LightsModule lights = ps.lights;
            lights.enabled = false;
        }
    }
    
    #if UNITY_EDITOR
    [Button]
    private void PopulateParticles() {
        ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>(true);
        List<ParticleSystem> particleList = new List<ParticleSystem>();
        foreach (ParticleSystem ps in allParticles) {
            if (ps.lights.enabled) { particleList.Add(ps); }
        }
        particles = particleList.ToArray();
    }
    #endif
}