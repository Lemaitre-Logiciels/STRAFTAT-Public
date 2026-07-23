using UnityEngine;
using UnityEngine.UI;

public class OutlineHandler : MonoBehaviour {
    // 0.5803921 0.6666667 1 1
    public static Color TeamColor = new Color(0.5803921f, 0.6666667f, 1f, 1f);
    public static Color EnemyColor = Color.red;
    public static float OutlineWidthMultiplier = 1f;
    
    
    [SerializeField] private Slider widthMultiplierSlider;
    public void WidthMultiplierSlider(float value) {
        OutlineWidthMultiplier = value;
        
        PlayerPrefs.SetFloat("OutlineWidthMultiplier", OutlineWidthMultiplier);
        PlayerPrefs.Save();
    }
    
    [SerializeField] private ColorInputSetting teamOutline;
    public void SaveTeamOutline(Color color) {
        TeamColor = color;
        
        string hexColor = ColorUtility.ToHtmlStringRGBA(TeamColor);
        PlayerPrefs.SetString("TeamOutlineColor", hexColor);
        PlayerPrefs.Save();
    }
    
    [SerializeField] private ColorInputSetting enemyOutline;
    public void SaveEnemyOutline(Color color) {
        EnemyColor = color;
        
        string hexColor = ColorUtility.ToHtmlStringRGBA(EnemyColor);
        PlayerPrefs.SetString("EnemyOutlineColor", hexColor);
        PlayerPrefs.Save();
    }

    public void Awake() {
        if (PlayerPrefs.HasKey("TeamOutlineColor")) {
            string hexColor = PlayerPrefs.GetString("TeamOutlineColor");
            if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color)) {
                TeamColor = color;
            }
        }
        if (PlayerPrefs.HasKey("EnemyOutlineColor")) {
            string hexColor = PlayerPrefs.GetString("EnemyOutlineColor");
            if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color)) { 
                EnemyColor = color;
            }
        }
        if (PlayerPrefs.HasKey("OutlineWidthMultiplier")) {
            OutlineWidthMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat("OutlineWidthMultiplier"), 0, 1f);
        }
        
        teamOutline.color = TeamColor;
        enemyOutline.color = EnemyColor;
        widthMultiplierSlider.SetValueWithoutNotify(OutlineWidthMultiplier);
    }
}
