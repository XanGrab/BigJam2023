using UnityEngine;

public class ShadowEntity : MonoBehaviour
{
    [SerializeField] private Transform shadowTransform;

    [SerializeField] private LayerMask groundLayer;

    private void Start()
    {
        shadowTransform.gameObject.name = "shadow_" + name;
        shadowTransform.parent = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        #region Set position and scale of "shadow" object
        float newScale = 0;
        if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), -transform.up, out RaycastHit _hit, 50, groundLayer))
        {
            shadowTransform.position = _hit.point;
            float distToGround = _hit.distance;

            //Shadow should be smaller the further away the character is from the ground
            newScale = Mathf.Max(1.5f - 0.1f * distToGround, 0);
        }
        shadowTransform.localScale = new Vector3(newScale, shadowTransform.localScale.y, newScale);
        #endregion
    }
}
