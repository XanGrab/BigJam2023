using UnityEngine;

public class VFXAnim : MonoBehaviour
{
    public void OnAnimEnd()
    {
        Destroy(transform.parent.gameObject);
    }
}
