using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += new Vector3(speed * Time.deltaTime, 0, 0);
    }
}
