using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [SerializeField] private VisualElement root;

    private Button startButton;
    private Button creditsButton;
    
    void Start() {
        root = GetComponent<UIDocument>().rootVisualElement;
        startButton = root.Q<Button>("Start-Button");
        creditsButton = root.Q<Button>("Credits-Button");

        startButton.clicked += StartButtonPress;
        startButton.RegisterCallback<NavigationMoveEvent> (e => {
            switch(e.direction) {
                case NavigationMoveEvent.Direction.Up: creditsButton.Focus(); break;
                case NavigationMoveEvent.Direction.Down: creditsButton.Focus(); break;
            }
            e.PreventDefault();
        });

        creditsButton.clicked += CreditsButtonPress;
        creditsButton.RegisterCallback<NavigationMoveEvent> (e => {
            switch(e.direction) {
                case NavigationMoveEvent.Direction.Up: startButton.Focus(); break;
                case NavigationMoveEvent.Direction.Down: startButton.Focus(); break;
            }
            e.PreventDefault();
        });
    }
    
    void StartButtonPress() => SceneManager.LoadScene("Gameplay");
    void CreditsButtonPress() => Debug.Log("TODO");


}
