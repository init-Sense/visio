using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RayProcessingController : MonoBehaviour
{
    [Serializable]
    public class ActionableObject
    {
        public GameObject targetObject;
        public ActionBase actionHandler;
        public bool canReflectRay;
    }
    
    public Dictionary<int, List<LineRenderer>> reflectedRayRenderers = new Dictionary<int, List<LineRenderer>>();
    public GameObject rayReceiver;
    public bool rayReceiverReflectsRays;
    public float reflectedRayLength = 1000f;

    public int maxReflectionCount = 5; // Defines the max depth for recursive reflections

    private bool _isProcessingRay = false; // Flag to indicate if a ray is currently being processed
    private bool _isRayHitting = false; // Flag to indicate if a ray is currently hitting the rayReceiver
    
    public List<ActionableObject> actionObjects = new List<ActionableObject>();

    
    private LineRenderer CreateNewLineRendererForRay(int rayIndex)
    {
        LineRenderer newRenderer = new GameObject("Reflected Ray Renderer " + rayIndex).AddComponent<LineRenderer>();
        newRenderer.transform.parent = this.transform;
        newRenderer.material = new Material(Shader.Find("Standard"));
        newRenderer.startColor = Color.blue;
        newRenderer.endColor = Color.blue;
        newRenderer.startWidth = 0.01f;
        newRenderer.endWidth = 0.01f;
        newRenderer.positionCount = 2;
        newRenderer.enabled = false;

        if (!reflectedRayRenderers.ContainsKey(rayIndex))
        {
            reflectedRayRenderers[rayIndex] = new List<LineRenderer>();
        }

        reflectedRayRenderers[rayIndex].Add(newRenderer);

        return newRenderer;
    }

    public void ProcessRayHit(Vector3 hitPoint, Ray incomingRay, Vector3 hitNormal, int rayIndex = 0)
    {
        _isRayHitting = true; // Set the flag to true

        // If a ray is currently being processed, return early
        if (_isProcessingRay) return;

        _isProcessingRay = true;

        foreach (ActionableObject actionObject in actionObjects)
        {
            if (_isRayHitting) // Only perform actions if the ray is currently hitting the rayReceiver
            {
                PerformAction(actionObject, incomingRay);
            }
        }

        // If rayReceiver should reflect rays, reflect the incoming ray
        if (rayReceiverReflectsRays && _isRayHitting) // Only reflect if the ray is currently hitting the rayReceiver
        {
            RecursiveRaycast(hitPoint, incomingRay, hitNormal, rayIndex, 0);
        }

        _isProcessingRay = false;
    }

    private void RecursiveRaycast(Vector3 hitPoint, Ray incomingRay, Vector3 hitNormal, int rayIndex,
        int currentReflectionCount)
    {
        Ray reflectedRay = new Ray(hitPoint, Vector3.Reflect(incomingRay.direction, hitNormal));

        RaycastHit reflectedHit;

        LineRenderer currentRayRenderer = CreateNewLineRendererForRay(rayIndex);

        if (Physics.Raycast(reflectedRay, out reflectedHit, reflectedRayLength))
        {
            // If reflected ray hits something
            currentRayRenderer.SetPosition(0, reflectedRay.origin);
            currentRayRenderer.SetPosition(1, reflectedHit.point);

            Debug.Log("Reflected ray hit " + reflectedHit.transform.gameObject.name);

            // Check if the hit object has a RayProcessingController component and allows reflections, and if we're under the maximum reflection count
            RayProcessingController hitObjectRPC =
                reflectedHit.transform.gameObject.GetComponent<RayProcessingController>();
            if (hitObjectRPC != null && hitObjectRPC.rayReceiverReflectsRays &&
                currentReflectionCount < maxReflectionCount)
            {
                // Call RecursiveRaycast again, but increment the current reflection count and ray index
                RecursiveRaycast(reflectedHit.point, reflectedRay, reflectedHit.normal, rayIndex + 1,
                    currentReflectionCount + 1);
            }
        }
        else
        {
            // If reflected ray doesn't hit anything, just draw in direction of reflection
            currentRayRenderer.SetPosition(0, reflectedRay.origin);
            currentRayRenderer.SetPosition(1, reflectedRay.origin + reflectedRay.direction * reflectedRayLength);

            Debug.Log("Reflected ray didn't hit anything");
        }

        currentRayRenderer.enabled = true;
    }


    private void PerformAction(ActionableObject actionObject, Ray incomingRay)
    {
        actionObject.actionHandler.ExecuteAction(actionObject.targetObject, incomingRay);

        // If the object can reflect the ray, do it here
        if (actionObject.canReflectRay)
        {
            // Reflect the ray.
            Ray reflectedRay = new Ray(actionObject.targetObject.transform.position, Vector3.Reflect(incomingRay.direction, Vector3.up));
        }
    }



    public void ResetRayHit(GameObject hitObject)
    {
        _isRayHitting = false;

        if (_isProcessingRay) return;

        _isProcessingRay = true;

        if (hitObject == rayReceiver)
        {
            foreach (ActionableObject actionObject in actionObjects)
            {
                actionObject.actionHandler.RevertAction(actionObject.targetObject);
            }

            // Disable and destroy all reflected ray LineRenderers
            foreach (var rayRenderers in reflectedRayRenderers)
            {
                foreach (LineRenderer lineRenderer in rayRenderers.Value)
                {
                    if (lineRenderer != null)
                    {
                        lineRenderer.enabled = false;
                        Destroy(lineRenderer.gameObject);
                    }
                }
            }

            // Clear the lists of LineRenderers.
            reflectedRayRenderers.Clear();
        }

        _isProcessingRay = false;
    }
}