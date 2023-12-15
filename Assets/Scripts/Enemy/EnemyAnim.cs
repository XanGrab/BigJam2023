using UnityEngine;

public class EnemyAnim : MonoBehaviour
{
    [SerializeField] private EnemyController enemy;

    public void SpawnHitbox()
    {
        enemy.SpawnHitbox();
        enemy.Scoot();
    }

    public void OnAttackEnd()
    {
        enemy.OnAttackEnd();
    }

    public void DeathAnimEnd()
    {
        Destroy(enemy.gameObject);
    }
}
