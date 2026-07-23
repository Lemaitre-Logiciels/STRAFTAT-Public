using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Crosshair : MonoBehaviour
{
    public RectTransform rectTransform;
    public static Crosshair Instance;
    public Image image;
    public Image scopeImage;
    public Sprite invisibleCrosshair;
    public Sprite nogunCrosshair;
    public Sprite standCrosshair;
    public Sprite sprintCrosshair;
    public Sprite aimCrosshair;
    [HideInInspector] public Sprite fixedCrosshair;
    [HideInInspector] public Vector2 fixedCrosshairSize;
    Vector2 defaultCrosshairSize = new Vector2(400, 400);
    [FormerlySerializedAs("FixedCrosshair")] public Sprite defaultFixedCrosshair;
    public bool canScopeAim;
    public AudioClip headshotHitClip;
    [SerializeField] private float aimCrosshairLimit = 18;
    public FirstPersonController player;

    private Settings settings;

    public GameObject hatObj;

    public bool  instantAimLens;
    // Start is called before the first frame update
    void Awake() {
        if (Instance == null) Instance = this;
        image = GetComponent<Image>();
    }

    void Start()
    {
        settings = Settings.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") {
            image.sprite = invisibleCrosshair;
            scopeImage.sprite = invisibleCrosshair;
        }
        
        image.enabled = !settings.disableCrosshair;
        
        if (!player) return; 
        
        var desiredCrosshair = (!player.playerPickupScript.hasObjectInHand && !player.playerPickupScript.hasObjectInLeftHand ? nogunCrosshair : 
                                canScopeAim && player.playerPickupScript.hasObjectInHand && aimCrosshair != null && player.isAiming && player.zoomFOV < settings.normalFovValue - aimCrosshairLimit ? invisibleCrosshair : player.isSprinting || !player.safeGrounded ? sprintCrosshair : standCrosshair);

        var ScopeDesiredCrosshair = player.playerPickupScript.hasObjectInHand && player.gameObject.activeSelf && canScopeAim && aimCrosshair != null && player.isAiming && ((player.playerCamera.fieldOfView < settings.normalFovValue - aimCrosshairLimit) || instantAimLens) ? aimCrosshair : invisibleCrosshair;

        if (settings.enableFixedCrosshair) {
            desiredCrosshair = (!player.playerPickupScript.hasObjectInHand && !player.playerPickupScript.hasObjectInLeftHand ? fixedCrosshair : canScopeAim && player.playerPickupScript.hasObjectInHand && aimCrosshair != null && player.isAiming && player.zoomFOV < settings.normalFovValue - aimCrosshairLimit ? invisibleCrosshair : player.isSprinting || !player.safeGrounded ? fixedCrosshair : fixedCrosshair);

            if (rectTransform.sizeDelta != fixedCrosshairSize) { rectTransform.sizeDelta = fixedCrosshairSize; }
        }
        else {
            if (rectTransform.sizeDelta != defaultCrosshairSize) { rectTransform.sizeDelta = defaultCrosshairSize; }
        }
        
        if (image.sprite != desiredCrosshair) image.sprite = desiredCrosshair;
        if (scopeImage.sprite != ScopeDesiredCrosshair) scopeImage.sprite = ScopeDesiredCrosshair;

        if (hatObj) hatObj.SetActive(true);

        if (Camera.main) {
            image.sprite = invisibleCrosshair;
            scopeImage.sprite = invisibleCrosshair;
        }
    }
}
