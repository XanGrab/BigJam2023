using UnityEngine;
using SoundSystem;
using System;
using System.Diagnostics;

[RequireComponent(typeof(PlayerController))]
public class MusicController : MonoBehaviour {
    private PlayerController playerCtrl;

    private int trackIndex = 0;

    public Sound music;

    // Start is called before the first frame update
    private void Awake() {
        playerCtrl = GetComponent<PlayerController>();
    }

    private void Start() {
        music.SetClipIndex(trackIndex);
        music.Play(0f, 1f);
    }

    private void OnEnable() {
        playerCtrl.OnModeChange += SwitchTrack;
    }

    private void OnDisable() {
        playerCtrl.OnModeChange -= SwitchTrack;
    }

    private void SwitchTrack(int modeIndex) {
        trackIndex = modeIndex - 5;
        music.SetClipIndex(trackIndex);
        AudioManager.FadeTo(music, 0.1f, 0.5f);
    }
}
