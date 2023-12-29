using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class MusicController : MonoBehaviour
{
    private PlayerController playerCtrl;

    private int trackIndex = 3;

    public Sound music;

    private string[] tracks = {
        "Stage1-Locrian",
        "Stage1-Phrygian",
        "Stage1-Mixolydian",
        "Stage1-Aeolian",
        "Stage1-Lydian",
        "Stage1-Dorian",
        "Stage1-Ionian",
    };

    // Start is called before the first frame update
    private void Awake() {
        playerCtrl = GetComponent<PlayerController>();
        AudioManager.LoadSounds( new Sound[] { music } );
    }

    private void Start() {
        AudioManager.Play(tracks[trackIndex]);
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
