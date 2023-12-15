using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;

    private void OnTriggerEnter(Collider _col)
    {
        Hitbox hitbox = _col.GetComponent<Hitbox>();

        stats.TakeDamage(hitbox.Damage);

        if (hitbox.DestroyOnHit)
            Destroy(hitbox.gameObject);
    }
}
