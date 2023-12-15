using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private int cost;
    [SerializeField] private float maxHp;
    private float currHp;

    public int getCost() => cost;

    // Start is called before the first frame update
    void Start()
    {
        maxHp += EnemySpawner.Instance.MonsterPoints;

        currHp = maxHp;
    }

    public void TakeDamage(float _dmg)
    {
        currHp = Mathf.Max(0, currHp - _dmg);

        GameObject vfx = Instantiate(HitVFX.Instance.HitVFXPrefab, null);
        vfx.transform.position = transform.position;

        //Healthbar.Instance.UpdateHPBar(currHp / maxHp);

        if (currHp <= 0)
            Die();
    }

    private void Die()
    {
        GetComponent<EnemyController>().OnDeath();
    }

    /// <summary>
    /// Attemps to heal the player the specified amount.
    /// </summary>
    /// <param name="_heal"> amount of health to heal </param>
    /// <returns> true if the healing was successful, false if the player is already at full hp.</returns>
    public bool Heal(float _heal)
    {
        if (currHp == maxHp)
            return false;

        currHp = Mathf.Min(maxHp, currHp + _heal);

        //Healthbar.Instance.UpdateHPBar(currHp / maxHp);

        return true;
    }
}
