using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "StaffMeter")]
public class StaffMeter : ScriptableObject {
    private int spriteIndex = 0;
    [SerializeField] private List<Sprite> sprites;

    public Sprite GetModeSprite(int modeIndex) {
        // Debug.Log("[Staff] modeIndex" + modeIndex);
        spriteIndex = (modeIndex - 5) * 8;
        // Debug.Log("[Staff] spriteIndex" + spriteIndex);
        Sprite modeSprite = sprites[spriteIndex];
        return modeSprite;
    }

    public Sprite GetNoteSprite(int numNotes) {
        spriteIndex = spriteIndex - (spriteIndex % 8) + numNotes;
        Sprite modeSprite = sprites[(spriteIndex - (spriteIndex % 7)) + numNotes];
        return modeSprite;
    }
}