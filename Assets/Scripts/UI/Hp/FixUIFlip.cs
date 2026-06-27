using UnityEngine;

public class FixUIFlip : MonoBehaviour
{
    private Transform parentTransform;
    private Vector3 initialScale;

    void Start()
    {
        parentTransform = transform.parent;
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (parentTransform == null) return;
        transform.rotation = Quaternion.identity;
        Vector3 newScale = initialScale;
        if (parentTransform.localScale.x < 0)
        {
            newScale.x = -initialScale.x; 
        }
        else
        {
            newScale.x = initialScale.x;
        }
        transform.localScale = newScale;
    }
}