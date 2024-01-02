using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using SoundSystem;

public class PlayerStats : Singleton<PlayerStats>{
    [SerializeField] private float maxHp;
    private float currHp;

    public event Action onHPChange;
    public UnityEvent onStatsReady;

    // Start is called before the first frame update
    void Start() {
        currHp = maxHp;
        onStatsReady.Invoke();
    }

    public float getMaxHP() => maxHp;
    public float getCurrHp() => currHp;

    public void TakeDamage(float _dmg) {
        currHp = Mathf.Max(0, currHp - _dmg);

        if (currHp <= 0)
            Die();
        onHPChange?.Invoke();
    }

    private void Die() {
        AudioManager.Play("GameOver");
        SceneManager.LoadScene("GameOver");
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

        onHPChange?.Invoke();
        return true;
    }
}
