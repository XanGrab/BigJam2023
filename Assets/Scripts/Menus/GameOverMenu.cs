using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SoundSystem;

[RequireComponent(typeof(UIDocument))]
public class GameOverMenu : MonoBehaviour {
    private UIDocument menu;

    private Button continueButton;
    private Button mainMenuButton;

    private Button[] menuNav = new Button[2];
    private int navIndex = 0;

    void Start() {
       menu = GetComponent<UIDocument>(); 
        var root = menu.rootVisualElement;
        continueButton = root.Q<Button>("Continue-Button");
        mainMenuButton = root.Q<Button>("Main-Menu-Button");
        menuNav = new Button[] { continueButton, mainMenuButton };

        root.RegisterCallback<NavigationMoveEvent> (e => {
            switch(e.direction) {
                case NavigationMoveEvent.Direction.Left:
                    navIndex = (navIndex + 1) % menuNav.Length;
                    menuNav[navIndex].Focus(); 
                    // Debug.Log("Debug [GameOver] navIndex " + navIndex);
                    break;
                case NavigationMoveEvent.Direction.Right:
                    navIndex--;
                    if (navIndex < 0) navIndex += menuNav.Length;
                    menuNav[navIndex].Focus(); 
                    // Debug.Log("Debug [GameOver] navIndex " + navIndex);
                    break;
            }
            e.PreventDefault();
        });

        continueButton.clicked += ContinueButtonPress;
        mainMenuButton.clicked += MainMenuButtonPress;
    }
    
    void ContinueButtonPress() {
        AudioManager.PlayOnce("Button");
        SceneManager.LoadScene("Gameplay");
    }

    void MainMenuButtonPress() {
        AudioManager.PlayOnce("Button");
        SceneManager.LoadScene("MainMenu");
    }

}
