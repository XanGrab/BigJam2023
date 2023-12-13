using System;
using UnityEngine;

public class FollowTarget : MonoBehaviour {
    /// <summary>
    /// The target transform to be followed
    /// </summary>
    [SerializeField] private Transform target;

    /// <summary>
    /// the minimum bound on the x-axis for this object
    /// </summary>
    [SerializeField] private float minX;
    /// <summary>
    /// the maximum bound on the x-axis for this object
    /// </summary>
    [SerializeField] private float maxX;

    /// <summary>
    /// The Vector3 offset from the target for the new position
    /// </summary>
    [SerializeField] private Vector3 offset;

    // private Vector3 velocity = Vector3.zero;
    // [SerializeField] private float deadZone = 2;
    // [SerializeField] private float dampenFactor = 0.1f;

    private void LateUpdate() {
        // if (Math.Abs(transform.position.x - target.position.x) < deadZone)     
        //     return;

        updatePostion();
    }

    private void updatePostion(){
        Vector3 targetPosition = offset;
        targetPosition.x = Mathf.Clamp(target.position.x, minX, maxX);

        transform.position = targetPosition;
        // transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dampenFactor);
    }
}
