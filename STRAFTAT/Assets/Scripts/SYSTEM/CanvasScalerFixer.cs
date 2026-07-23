using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasScalerFixer : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;
    void Update() { TrySetMatchWidthOrHeight(Screen.width, Screen.height); }

    private void TrySetMatchWidthOrHeight(int width, int height) {
        // check if is 16:9
        float numberOfScreenThing = (float)width/height;
        // 16:9 is 1.7777777 so if it is less than or equal to that, match width, otherwise match height
        // def a hack BUT it works so everybody can be happy
        float targetNumberOfScreenThing = numberOfScreenThing <= 1.7777777f ? 0f : 1f;
        if (canvasScaler.matchWidthOrHeight != targetNumberOfScreenThing) { canvasScaler.matchWidthOrHeight = targetNumberOfScreenThing; }
    }
}