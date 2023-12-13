using UnityEngine;

public class CamerMover : MonoBehaviour
{
    [SerializeField] private float minX;
    [SerializeField] private float maxX;

    [SerializeField] private Transform target;

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Clamp(target.position.x, minX, maxX);

        transform.position = newPos;
    }
}
