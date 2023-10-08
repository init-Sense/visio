using UnityEngine;

/// <summary>
/// This script is used to generate a ray and reflect it using the RayReflectionHandler script.
/// </summary>
public class RayActivationController : MonoBehaviour
{
    [Tooltip("Raycast origin.")] public Transform raycastOrigin;

    [Tooltip("Raycast mask. If unsure, set it to Puzzle Ray.")]
    public LayerMask raycastMask;

    private LineRenderer _lineRenderer;

    public Material lineMaterial;

    private RayReflectionHandler CurrentReflectionHandler { get; set; }

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
        _lineRenderer.enabled = true;
    }

    public void DeactivateRaycasting()
    {
        _lineRenderer.enabled = false;
    }

    void FixedUpdate()
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

            RayReflectionHandler hitReflectionHandler = hit.collider.gameObject.GetComponent<RayReflectionHandler>();
            if (hitReflectionHandler != null)
            {
                CurrentReflectionHandler = hitReflectionHandler;
                CurrentReflectionHandler.Initialize(hit.point, ray.direction, hit.normal);
            }
            else
            {
                if (CurrentReflectionHandler != null)
                {
                    CurrentReflectionHandler.DestroyReflectionLine();
                    CurrentReflectionHandler = null;
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(1, raycastOrigin.position + ray.direction * 15f);

            if (CurrentReflectionHandler != null)
            {
                CurrentReflectionHandler.DestroyReflectionLine();
                CurrentReflectionHandler = null;
            }
        }

        return hitSomething;
    }
}