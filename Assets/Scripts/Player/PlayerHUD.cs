using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class PlayerHUD : Singleton<PlayerHUD> {
    [SerializeField] private UIDocument playerHUD;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController playerCtrl;

    private ProgressBar Healthbar;
    [SerializeField] private StaffMeter staffMeter;
    private VisualElement MeterUI;
    public VisualElement ModeImage { get; private set; }
    [SerializeField] private List<Sprite> modeSprites;
    public Label ModeLabel;

    private string[] modeNames = {
        "Locrian Longbow",
        "Phrygian Phists",
        "Mixolydian Masherator",
        "Aeolian Adachi",
        "Lydian Lacerator",
        "Dorian Defender",
        "Ionian Invoker",
    };

    public void Init() {
        if (!playerHUD) return;

        var root = playerHUD.rootVisualElement;
        Healthbar = root.Q<ProgressBar>("HealthBar");
        MeterUI = root.Q<VisualElement>("MusicStaff");
        ModeImage = root.Q<VisualElement>("ModeImage");
        ModeLabel = root.Q<Label>("ModeLabel");

        Healthbar.highValue = playerStats.getMaxHP();
        Healthbar.lowValue = 0;

        Healthbar.value = playerStats.getCurrHp();
        // Hardcoded starting value :(
        SetMode(8);
    }

    private void UpdateHealthBarValue() {
        Healthbar.value = playerStats.getCurrHp();
    }

    private void OnEnable() {
        playerStats.onHPChange += UpdateHealthBarValue;
        playerCtrl.OnModeChange += SetMode;
    }

    private void OnDisable() {
        playerStats.onHPChange -= UpdateHealthBarValue;
        playerCtrl.OnModeChange -= SetMode;
    }

    private void SetMode(int modeIndex) {
        Sprite modeSprite = modeSprites[modeIndex - 5];
        ModeImage.style.backgroundImage = new StyleBackground(modeSprite);
        ModeLabel.text = modeNames[modeIndex - 5];

        Sprite meterSprite = staffMeter.GetModeSprite(modeIndex);
        MeterUI.style.backgroundImage = new StyleBackground(meterSprite);
    }
}
