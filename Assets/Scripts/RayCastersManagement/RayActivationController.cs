using System.Collections.Generic;
using UnityEngine;

public class RayActivationController : MonoBehaviour
{
    public List<Transform> raycastOrigins;
    public LayerMask raycastMask;
    public List<RayProcessingController> rayProcessingControllers;
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    private bool _isRayActive = false; // Variable to track ray activation

    void Awake()
    {
        foreach (Transform raycastOrigin in raycastOrigins)
        {
            LineRenderer lineRenderer = raycastOrigin.gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Standard"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            _lineRenderers.Add(lineRenderer);
        }
    }

    // Public method to activate raycasting
    public void ActivateRaycasting()
    {
        _isRayActive = true;
    }

    // Public method to deactivate raycasting
    public void DeactivateRaycasting()
    {
        _isRayActive = false;
    }

    void Update()
    {
        if (_isRayActive)
        {
            for (int i = 0; i < raycastOrigins.Count; i++)
            {
                Raycast(raycastOrigins[i], _lineRenderers[i], i);
            }
        }
        else
        {
            foreach (LineRenderer lineRenderer in _lineRenderers)
            {
                lineRenderer.enabled = false;
            }
        }
    }

    void Raycast(Transform raycastOrigin, LineRenderer lineRenderer, int rayIndex)
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask))
        {
            lineRenderer.SetPosition(0, raycastOrigin.position);
            lineRenderer.SetPosition(1, hit.point);

            foreach (RayProcessingController controller in rayProcessingControllers)
            {
                controller.ProcessRayHit(hit.point, ray, hit.normal, rayIndex);
            }
        }
        else
        {
            lineRenderer.SetPosition(0, raycastOrigin.position);
            lineRenderer.SetPosition(1, raycastOrigin.position + raycastOrigin.forward * 1000);

            foreach (RayProcessingController controller in rayProcessingControllers)
            {
                controller.ResetAllRayHits(); // Call a method that resets all the RayReceivers inside the controller
            }
        }

        // Always enable the line renderer, even if no hit occurs
        lineRenderer.enabled = true;
    }
}