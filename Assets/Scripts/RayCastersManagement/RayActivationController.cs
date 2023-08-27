using UnityEngine;

/// <summary>
/// This class controls the activation of ray casting.
/// The ray casting is activated/deactivated by the RayProcessingController.
/// </summary>
public class RayActivationController : MonoBehaviour
{
    [Tooltip("Raycast origin.")]
    public Transform raycastOrigin;
    [Tooltip("Raycast mask.")]
    public LayerMask raycastMask;
    [Tooltip("Reference to the ray processing controller.")]
    public RayProcessingController rayProcessingController;

    private LineRenderer _lineRenderer;

    void Awake()
    {
        _lineRenderer = raycastOrigin.gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Standard"));
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.startWidth = 0.01f;
        _lineRenderer.endWidth = 0.01f;
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
    }

    public void ActivateRaycasting()
    {
        _lineRenderer.enabled = true;
    }

    public void DeactivateRaycasting()
    {
        _lineRenderer.enabled = false;
    }

    void Update()
    {
        if (_lineRenderer.enabled)
        {
            Raycast(raycastOrigin, _lineRenderer, raycastMask);
        }
    }

    bool Raycast(Transform raycastOrigin, LineRenderer lineRenderer, LayerMask raycastMask)
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask);

        lineRenderer.SetPosition(0, raycastOrigin.position);

        if (hitSomething)
        {
            lineRenderer.SetPosition(1, hit.point);

            if (rayProcessingController.enableReflection)
            {
                // Create or update the reflection line
                if (rayProcessingController.currentReflectionLine == null)
                {
                    rayProcessingController.currentReflectionLine = rayProcessingController.CreateReflectionLineRenderer(hit.point, hit.point);
                }

                Vector3 reflectedDirection = Vector3.Reflect(ray.direction, hit.normal);
                Vector3 endPoint = hit.point + reflectedDirection * 1000; // default end point if no hit
                rayProcessingController.currentReflectionLine.SetPosition(0, hit.point);
                rayProcessingController.currentReflectionLine.SetPosition(1, endPoint);
            }

            rayProcessingController.ProcessRayHit(hit.point, ray, hit.normal);
        }
        else
        {
            lineRenderer.SetPosition(1, raycastOrigin.position + raycastOrigin.forward * 1000);
            rayProcessingController.DestroyReflectionLine();
        }

        return hitSomething;
    }
    
}