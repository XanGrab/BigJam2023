using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;

    private void OnTriggerEnter(Collider _col)
    {
        stats.TakeDamage(_col.GetComponent<Hitbox>().Damage);
    }
}
