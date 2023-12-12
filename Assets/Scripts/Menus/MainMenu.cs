using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour {
    private UIDocument menu;

    private Button startButton;
    private Button creditsButton;

    private Button[] menuNav = new Button[2];
    private int navIndex = 0;
    
    void Start() {
        menu = GetComponent<UIDocument>(); 
        var root = menu.rootVisualElement;
        startButton = root.Q<Button>("Start-Button");
        creditsButton = root.Q<Button>("Credits-Button");
        menuNav = new Button[] { startButton, creditsButton };

        // register navigation logic
        startButton.RegisterCallback<NavigationMoveEvent> (e => {
            e.PreventDefault();
            switch(e.direction) {
                case NavigationMoveEvent.Direction.Up: 
                    navIndex = (navIndex + 1) % menuNav.Length;
                    menuNav[navIndex].Focus(); 
                    Debug.Log("Debug [MainMenu] navIndex " + navIndex);
                    break;
                case NavigationMoveEvent.Direction.Down: 
                    navIndex = Math.Abs(navIndex - 1) % menuNav.Length;
                    menuNav[navIndex].Focus(); 
                    Debug.Log("Debug [MainMenu] navIndex " + navIndex);
                    break;
            }
        });

        // register button logic
        startButton.clicked += StartButtonPress;
        creditsButton.clicked += CreditsButtonPress;
    }
    
    void StartButtonPress() => SceneManager.LoadScene("Gameplay");
    void CreditsButtonPress() { 
        Debug.Log("TODO");
    }


}
