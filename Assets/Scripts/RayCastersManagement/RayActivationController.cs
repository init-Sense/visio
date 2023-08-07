using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the activation of ray casting groups.
/// Each group has a list of raycast origins and a raycast mask.
/// The ray casting groups are activated/deactivated by the RayProcessingController.
/// </summary>

public class RayActivationController : MonoBehaviour
{
    [Tooltip("List of raycast groups.")]
    [System.Serializable]
    public class RayCasterGroup
    {
        public List<Transform> raycastOrigins;
        public LayerMask raycastMask;
        public bool isActive; // Individual activation for each group
    }

    [Tooltip("List of raycast groups.")]
    public List<RayCasterGroup> rayCasterGroups;
    
    [Tooltip("Reference to the ray processing controller.")]
    public RayProcessingController rayProcessingController;

    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    void Awake()
    {
        foreach (RayCasterGroup group in rayCasterGroups)
        {
            foreach (Transform raycastOrigin in group.raycastOrigins)
            {
                LineRenderer lineRenderer = raycastOrigin.gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Standard"));
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.enabled = false;

                _lineRenderers.Add(lineRenderer); // Add the line renderer to the list
            }
        }
    }

    public void ActivateRaycasting(int groupIndex)
    {
        if (groupIndex < rayCasterGroups.Count)
        {
            rayCasterGroups[groupIndex].isActive = true;
        }
    }

    public void DeactivateRaycasting(int groupIndex)
    {
        if (groupIndex < rayCasterGroups.Count)
        {
            rayCasterGroups[groupIndex].isActive = false;
        }
    }

    void Update()
    {
        int lineRendererIndex = 0;
        foreach (RayCasterGroup group in rayCasterGroups)
        {
            if (group.isActive)
            {
                for (int i = 0; i < group.raycastOrigins.Count; i++)
                {
                    Raycast(group.raycastOrigins[i], _lineRenderers[lineRendererIndex++], group.raycastMask);
                }
            }
            else
            {
                for (int i = 0; i < group.raycastOrigins.Count; i++)
                {
                    _lineRenderers[lineRendererIndex++].enabled = false;
                }
            }
        }
    }

    void Raycast(Transform raycastOrigin, LineRenderer lineRenderer, LayerMask raycastMask) // Correct parameter
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask)) // Corrected to raycastMask
        {
            lineRenderer.SetPosition(0, raycastOrigin.position);
            lineRenderer.SetPosition(1, hit.point);

            rayProcessingController.ProcessRayHit(hit.point, ray, hit.normal, 0);
        }
        else
        {
            lineRenderer.SetPosition(0, raycastOrigin.position);
            lineRenderer.SetPosition(1, raycastOrigin.position + raycastOrigin.forward * 1000);

            rayProcessingController
                .ResetAllRayHits(); // Call a method that resets all the RayReceivers inside the controller
        }

        lineRenderer.enabled = true;
    }
}