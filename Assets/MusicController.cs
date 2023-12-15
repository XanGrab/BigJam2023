using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
public class MusicController : MonoBehaviour {
    private PlayerController playerCtrl;

    private int trackIndex = 3;
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
    }

    private void Start() {
        AudioManager.Play(tracks[trackIndex]); 
    }

    private void OnEnable() {
        playerCtrl.onModeChange += SwitchTrack;
    }
    private void OnDisable() {
        playerCtrl.onModeChange -= SwitchTrack;
    }

    private void SwitchTrack(int modeIndex){
        trackIndex = modeIndex - 5;
        float timestamp = AudioManager.GetTimestamp();
        AudioManager.Stop();
        Debug.Log("[MusicController] onSwitch " + modeIndex + ", Track Index " + trackIndex);
        AudioManager.Play(tracks[trackIndex], timestamp);
    }
}
