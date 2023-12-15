using UnityEngine;

public class PlayerAnim : MonoBehaviour {
    [SerializeField] private PlayerController player;

    public void SpawnHitbox() {
        player.SpawnHitbox();
        player.Scoot();
    }

    public void OnAttackEnd() {
        player.OnAttackEnd();
    }
}
