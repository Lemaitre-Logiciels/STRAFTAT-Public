using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurningGlow : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;
    private float glowTimer;
    public float glowSpeed;
    public float glowAmount;
    private Color c;
    
    // Start is called before the first frame update
    void Awake()
    {
        c = textLabel.color;
    }

    // Update is called once per frame
    void Update()
    {
        glowTimer += Time.deltaTime * glowSpeed;

        textLabel.fontMaterial.SetFloat(ShaderUtilities.ID_LightAngle, 3f + Mathf.Sin(glowTimer) * glowAmount);

    }
}
