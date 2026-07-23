using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CapTheSize : MonoBehaviour, ILayoutSelfController {
    public Vector2 maxSize = new Vector2(500, 500);
    private RectTransform rect;

    void Awake() => rect = GetComponent<RectTransform>();

    public void SetLayoutHorizontal() => LimitSize(0);
    public void SetLayoutVertical() => LimitSize(1);

    private void LimitSize(int axis) {
        if (rect.sizeDelta[axis] > maxSize[axis]) { rect.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, maxSize[axis]); }
    }
}