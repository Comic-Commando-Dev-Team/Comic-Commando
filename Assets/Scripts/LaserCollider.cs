using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class LaserCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private BoxCollider boxCollider;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    void Update()
    {
        UpdateCollider();
    }

    void UpdateCollider()
    {
        Vector3 start = lineRenderer.GetPosition(0);
        Vector3 end = lineRenderer.GetPosition(1);
        Vector3 direction = end - start;

        boxCollider.size = new Vector3(direction.magnitude, 0.1f, 0.1f);
        boxCollider.center = start + direction / 2;
        boxCollider.transform.rotation = Quaternion.LookRotation(direction);
    }
}