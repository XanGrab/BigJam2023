using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerHUD : Singleton<PlayerHUD>
{
    [SerializeField] private UIDocument playerHUD;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController playerCtrl;

    private ProgressBar Healthbar;
    [SerializeField] private StaffMeter staffMeter;
    private VisualElement MeterUI;
    public VisualElement ModeImage { get; private set; }
    [SerializeField] private List<Sprite> modeSprites;
    public Label ModeLabel;

    private int numNotes;

    private string[] modeNames = {
        "Ionian Invoker",
        "Dorian Defender",
        "Phrygian Phists",
        "Lydian Lacerator",
        "Mixolydian Masherator",
        "Aeolian Adachi",
        "Locrian Longbow",
    };

    public void Init()
    {
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
        SetMode(5);
    }

    private void UpdateHealthBarValue()
    {
        Healthbar.value = playerStats.getCurrHp();
    }

    private void OnEnable()
    {
        playerStats.onHPChange += UpdateHealthBarValue;
        playerCtrl.OnModeChange += SetMode;
    }

    private void OnDisable()
    {
        playerStats.onHPChange -= UpdateHealthBarValue;
        playerCtrl.OnModeChange -= SetMode;
    }

    private void SetMode(int modeIndex)
    {
        Sprite modeSprite = modeSprites[modeIndex - 5];
        ModeImage.style.backgroundImage = new StyleBackground(modeSprite);
        ModeLabel.text = modeNames[modeIndex - 5];

        Sprite meterSprite = staffMeter.GetModeSprite(modeIndex);
        MeterUI.style.backgroundImage = new StyleBackground(meterSprite);

        numNotes = 0;
    }

    public void OnEnemyKilled()
    {
        numNotes++;

        if (numNotes > 7)
        {
            numNotes = 0;
            //TODO: BIG BOOM???
        }

        Sprite meterSprite = staffMeter.GetNoteSprite(numNotes);
        MeterUI.style.backgroundImage = new StyleBackground(meterSprite);
    }
}
