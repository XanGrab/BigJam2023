using UnityEngine;

public class HitVFX : Singleton<HitVFX>
{
    [field: SerializeField] public GameObject HitVFXPrefab { get; private set; }
}
