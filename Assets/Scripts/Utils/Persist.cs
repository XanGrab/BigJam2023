using UnityEngine;

public class Persist : MonoBehaviour
{
    private void Awake()
    {
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }
}
