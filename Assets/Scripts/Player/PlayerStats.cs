using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{
    [SerializeField] private float maxHp;
    private float currHp;

    // Start is called before the first frame update
    void Start()
    {
        //Healthbar.Instance.UpdateMaxHP(maxHp);
        currHp = maxHp;
    }

    public void TakeDamage(float _dmg)
    {
        currHp = Mathf.Max(0, currHp - _dmg);

        SetHPVisuals(currHp / maxHp);

        if (currHp <= 0)
            Die();
    }

    private void Die()
    {

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

        SetHPVisuals(currHp / maxHp);

        return true;
    }

    private void SetHPVisuals(float _healthPercent)
    {
        //Xander put your funny uwu code here or something idk
    }
}
