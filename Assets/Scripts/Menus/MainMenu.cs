using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour
{
    private UIDocument menu;

    [SerializeField] private VisualTreeAsset[] Menus;

    private Button startButton;
    private Button creditsButton;
    private Button creditsBackButton;

    private Button[] menuNav = new Button[2];
    private int navIndex = 0;
    
    void Start() {
        SetUpMainMenu();
    }

    void SetUpMainMenu() {
        menu = GetComponent<UIDocument>(); 
        var root = menu.rootVisualElement;
        startButton = root.Q<Button>("Start-Button");
        creditsButton = root.Q<Button>("Credits-Button");

        menuNav = new Button[] { startButton, creditsButton };

        // register navigation logic
        root.RegisterCallback<NavigationMoveEvent>(e =>
        {
            e.PreventDefault();
            switch (e.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    navIndex = (navIndex + 1) % menuNav.Length;
                    menuNav[navIndex].Focus();
                    // Debug.Log("Debug [MainMenu] navIndex " + navIndex);
                    break;
                case NavigationMoveEvent.Direction.Down:
                    navIndex--;
                    if (navIndex < 0) navIndex += menuNav.Length;
                    menuNav[navIndex].Focus();
                    // Debug.Log("Debug [MainMenu] navIndex " + navIndex);
                    break;
            }
        });

        // register button logic
        startButton.clicked += StartButtonPress;
        creditsButton.clicked += CreditsButtonPress;
    }

    void SetUpCredits() {
        menu = GetComponent<UIDocument>(); 
        var root = menu.rootVisualElement;

        creditsBackButton = root.Q<Button>("Back-Button");

        // register button logic
        creditsBackButton.clicked += CreditsBackButtonPress;
    }
    
    void StartButtonPress() => SceneManager.LoadScene("Gameplay");
    void CreditsButtonPress() {
        menu.visualTreeAsset = Menus[1];
        menu.visualTreeAsset.Instantiate();
        SetUpCredits();
    }

    void CreditsBackButtonPress() {
        menu.visualTreeAsset = Menus[0];
        menu.visualTreeAsset.Instantiate();
        SetUpMainMenu();
    }
}
