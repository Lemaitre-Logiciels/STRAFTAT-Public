using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ColorInputSetting : MonoBehaviour {
    [FormerlySerializedAs("outlineColor")] [HideInInspector] public Color color = Color.black;
    public UnityEvent<Color> onColorChanged;

    public void Start() {
        redText.SetTextWithoutNotify(((int)(color.r * 255)).ToString());
        greenText.SetTextWithoutNotify(((int)(color.g * 255)).ToString());
        blueText.SetTextWithoutNotify(((int)(color.b * 255)).ToString());
        alphaText.SetTextWithoutNotify(((int)(color.a * 255)).ToString());
        previewImage.color = color;
    }

    [SerializeField] private TMP_InputField redText;
    public void Red(string redInput) {
        int redValue = int.Parse(redInput);
        if (redValue < 0 || redValue > 255) {
            redValue = Mathf.Clamp(redValue, 0, 255);
            redText.text = redValue.ToString();
        }
        color.r = redValue / 255f;
        OnColorChanged();
    }

    [SerializeField] private TMP_InputField greenText;
    public void Green(string greenInput) {
        int greenValue = int.Parse(greenInput);
        if (greenValue < 0 || greenValue > 255) {
            greenValue = Mathf.Clamp(greenValue, 0, 255);
            greenText.text = greenValue.ToString();
        }
        color.g = greenValue / 255f;
        OnColorChanged();
    }
    
    [SerializeField] private TMP_InputField blueText;
    public void Blue(string blueInput) {
        int blueValue = int.Parse(blueInput);
        if (blueValue < 0 || blueValue > 255) {
            blueValue = Mathf.Clamp(blueValue, 0, 255);
            blueText.text = blueValue.ToString();
        }
        color.b = blueValue / 255f;
        OnColorChanged();
    }
    
    [SerializeField] private TMP_InputField alphaText;
    public void Alpha(string alphaInput) {
        int alphaValue = int.Parse(alphaInput);
        if (alphaValue < 0 || alphaValue > 255) {
            alphaValue = Mathf.Clamp(alphaValue, 0, 255);
            alphaText.text = alphaValue.ToString();
        }
        color.a = alphaValue / 255f;
        OnColorChanged();
    }
    
    public void OnColorChanged() {
        previewImage.color = color;
        onColorChanged.Invoke(color);
    }
    
    [SerializeField] private Image previewImage;
}
