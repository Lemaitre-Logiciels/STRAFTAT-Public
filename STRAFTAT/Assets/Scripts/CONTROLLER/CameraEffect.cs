using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraEffect : MonoBehaviour
{
    private static readonly int DamageID = Shader.PropertyToID("_Damage");
    private static readonly int SuppressionID = Shader.PropertyToID("_Suppression");
    
    public static float Intensity = 1f;
    
    public Material material;
    private float dmg;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float suppSpeed = 10f;
    [SerializeField] private float start = 1f;
    [SerializeField] private float suppStart = 3f;
    [Space]
    [SerializeField] private AudioClip[] selfBodyHitClips;

    void Awake()
    {
        material.SetFloat(DamageID, 0);
        material.SetFloat(SuppressionID, 0);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat(DamageID, material.GetFloat(DamageID)-Time.deltaTime*speed);
        material.SetFloat(DamageID, Mathf.Clamp(material.GetFloat(DamageID), 0, start * Intensity));

        material.SetFloat(SuppressionID, material.GetFloat(SuppressionID)-Time.deltaTime*suppSpeed);
        material.SetFloat(SuppressionID, Mathf.Clamp(material.GetFloat(SuppressionID), 0, suppStart * Intensity));

        Graphics.Blit(source, destination, material);
    }

    [ContextMenu ("TakeHit")]
    public void TakeHit()
    {
        Intensity = Settings.Instance.damageIntensity;
        material.SetFloat(DamageID, start * Intensity);
        SoundManager.Instance.PlaySoundWithPitch(selfBodyHitClips[(int)Mathf.RoundToInt(Random.Range(0, selfBodyHitClips.Length))], Random.Range(0.95f, 1.05f));
    }

    [ContextMenu ("supp")]
    public void TrigSup()
    {
        material.SetFloat(SuppressionID, suppStart * Intensity);
    }
}
