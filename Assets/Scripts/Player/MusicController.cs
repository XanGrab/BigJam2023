using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class MusicController : MonoBehaviour
{
    private PlayerController playerCtrl;

    private int trackIndex = 0;

    public Sound music;

    // Start is called before the first frame update
    private void Awake() {
        playerCtrl = GetComponent<PlayerController>();
        AudioManager.LoadSounds( new Sound[] { music } );
    }

    private void Start() {
        music.setClipIndex(trackIndex);
        AudioManager.Play(music.name);
    }

    private void OnEnable() {
        playerCtrl.OnModeChange += SwitchTrack;
    }
    private void OnDisable() {
        playerCtrl.OnModeChange -= SwitchTrack;
    }

    private void SwitchTrack(int modeIndex) {
        trackIndex = modeIndex - 5;
        float timestamp = AudioManager.GetTimestamp();
        music.setClipIndex(trackIndex);
        AudioManager.Play(music.name, timestamp);
    }
}
