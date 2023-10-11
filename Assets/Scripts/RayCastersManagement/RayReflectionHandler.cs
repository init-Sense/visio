using UnityEngine;

/// <summary>
/// This script is solely responsible for reflecting a ray. Give this script to any object that should have reflective properties.
/// </summary>
public class RayReflectionHandler : MonoBehaviour
{
    [Tooltip("Material to use for the reflection line.")]
    public Material reflectionMaterial;

    [Tooltip("Layer mask to specify which layers the raycast should hit.")]
    public LayerMask raycastMask;

    [Tooltip("Maximum number of reflections.")]
    public int reflectionLimit = 5;

    private LineRenderer _currentReflectionLine;

    private void Awake()
    {
        _currentReflectionLine = CreateReflectionLineRenderer(transform.position, transform.position);
        _currentReflectionLine.enabled = false;
    }


    public void Initialize(Vector3 hitPoint, Vector3 incomingDirection, Vector3 normal)
    {
        ReflectRay(hitPoint, incomingDirection, normal, 0);
    }

    private void ReflectRay(Vector3 origin, Vector3 direction, Vector3 normal, int reflectionCount)
    {
        if (reflectionCount >= reflectionLimit) return;

        Vector3 reflectedDirection = Vector3.Reflect(direction, normal);
        Ray ray = new Ray(origin, reflectedDirection);
        RaycastHit hit;

        Vector3 endPoint = origin + reflectedDirection * 20;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask))
        {
            endPoint = hit.point;

            RayReflectionHandler reflectionHandler = hit.collider.gameObject.GetComponent<RayReflectionHandler>();
            if (reflectionHandler != null)
            {
                reflectionHandler.Initialize(hit.point, reflectedDirection, hit.normal); 
            }

            Renderer hitRenderer = hit.collider.gameObject.GetComponent<Renderer>();
            if (hitRenderer != null && hitRenderer.sharedMaterial == reflectionMaterial)
            {
                ReflectRay(hit.point, reflectedDirection, hit.normal, reflectionCount + 1);
            }
        }



        if (_currentReflectionLine != null)
        {
            _currentReflectionLine.enabled = true;
            _currentReflectionLine.SetPosition(0, origin);
            _currentReflectionLine.SetPosition(1, endPoint);
        }
    }

    private LineRenderer CreateReflectionLineRenderer(Vector3 start, Vector3 end)
    {
        LineRenderer lineRenderer;

        GameObject reflectionLineObject = new GameObject("ReflectionLine");
        reflectionLineObject.transform.SetParent(transform);
        lineRenderer = reflectionLineObject.AddComponent<LineRenderer>();
        lineRenderer.material = reflectionMaterial;
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        reflectionLineObject.layer = gameObject.layer;

        return lineRenderer;
    }

    public void DestroyReflectionLine()
    {
        if (_currentReflectionLine != null)
        {
            _currentReflectionLine.enabled = false;
        }
    }
}