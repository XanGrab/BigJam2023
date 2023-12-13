using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider _col)
    {
        PlayerStats.Instance.TakeDamage(_col.GetComponent<Hitbox>().Damage);
    }
}
