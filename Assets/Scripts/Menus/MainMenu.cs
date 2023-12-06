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
        creditsButton.clicked += CreditsButtonPress;
    }
    
    void StartButtonPress() => SceneManager.LoadScene("Gameplay");
    void CreditsButtonPress() => Debug.Log("TODO");
}
