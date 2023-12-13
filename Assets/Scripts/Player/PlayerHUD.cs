using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerHUD : MonoBehaviour {
    [SerializeField] private UIDocument playerHUD;
    [SerializeField] private PlayerStats playerStats;

    private ProgressBar Healthbar;

    // Start is called before the first frame update
    public void Init() {
        if(!playerHUD) return;

        var root = playerHUD.rootVisualElement;
        Healthbar = root.Q<ProgressBar>("HealthBar");

        Healthbar.highValue = playerStats.getMaxHP();
        Healthbar.lowValue = 0;

        Healthbar.value = playerStats.getCurrHp();
    }

    private void UpdateHealthBarValue(){
        Healthbar.value = playerStats.getCurrHp();
    }

    private void OnEnable() {
        playerStats.onHPChange += UpdateHealthBarValue;
    }
    private void OnDisable() {
        playerStats.onHPChange -= UpdateHealthBarValue;
    }
}
