using BeauRoutine;
using System.Collections;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float timeTillDestroy = -1f;
    [field: SerializeField] public bool DestroyOnHit { get; private set; }

    [field: SerializeField] public int Damage { get; private set; }

    private void Start()
    {
        if (timeTillDestroy > 0)
            Routine.Start(this, DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return timeTillDestroy;

        Destroy(gameObject);
    }
}
