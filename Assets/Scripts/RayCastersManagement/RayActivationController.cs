using UnityEngine;

public class RayActivationController : MonoBehaviour
{
    [Tooltip("Raycast origin.")]
    public Transform raycastOrigin;
    [Tooltip("Raycast mask.")]
    public LayerMask raycastMask;
    [Tooltip("Reference to the ray processing controller.")]
    public RayProcessingController rayProcessingController;

    private LineRenderer _lineRenderer;

    public Material lineMaterial;

    void Awake()
    {
        _lineRenderer = raycastOrigin.gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Standard"));
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.startWidth = 0.01f;
        _lineRenderer.endWidth = 0.01f;
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
    }

    public void ActivateRaycasting()
    {
        Debug.Log("ActivateRaycasting called");
        _lineRenderer.enabled = true;
    }

    public void DeactivateRaycasting()
    {
        Debug.Log("DeactivateRaycasting called");
        _lineRenderer.enabled = false;
        rayProcessingController.ResetRayHit();


    }

    void FixedUpdate()
    {
        if (_lineRenderer.enabled)
        {
            bool hitSomething = Raycast(raycastOrigin, _lineRenderer, raycastMask);
            if (!hitSomething)
            {
                rayProcessingController.DestroyReflectionLine();
            }
        }
    }


    private float resetCooldown = 0.5f; // Time in seconds before resetting the ray hit
    private float lastHitTime;

    private bool reflectionDestroyedThisFrame = false;

    bool Raycast(Transform raycastOrigin, LineRenderer lineRenderer, LayerMask raycastMask)
    {
        Debug.Log("Raycast called");
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask);

        lineRenderer.SetPosition(0, raycastOrigin.position);

        if (hitSomething)
        {
            lineRenderer.SetPosition(1, hit.point);

            RayProcessingController hitReceiver = hit.collider.gameObject.GetComponent<RayProcessingController>();
            if (hitReceiver != null)
            {
                hitReceiver.ProcessRayHit(hit.point, ray, hit.normal);
            }

            // If the ray hits an object with the "Exit" tag or exits the reflective surface
            if (hit.collider.CompareTag("Exit") || hitReceiver == null)
            {
                rayProcessingController.DestroyReflectionLine();
            }
        }
        else
        {
            lineRenderer.SetPosition(1, raycastOrigin.position + raycastOrigin.forward * 1000);
            rayProcessingController.DestroyReflectionLine();
        }

        return hitSomething;
    }





    void LateUpdate()
    {
        reflectionDestroyedThisFrame = false;
    }


}
