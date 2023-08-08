using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class controls the processing of ray hits.
/// Each ray receiver has a list of actionable objects and a flag to enable/disable the reflection of the ray.
/// The ray receivers are activated/deactivated by the RayActivationController.
/// </summary>
public class RayProcessingController : MonoBehaviour
{
    [Tooltip("List of ray receivers.")]
    [System.Serializable]
    public class RayReceiver
    {
        public GameObject rayReceiver; // The GameObject affected by the ray
        public bool enableReflection; // Flag to enable reflection of the ray
        public List<ActionableObject> actionableObjects; // List of actions to perform on hit
        public int reflectionLimit; // Limit for recursive reflections
    }

    [Tooltip("List of actions to perform on hit.")]
    [System.Serializable]
    public class ActionableObject
    {
        public GameObject targetObject; // The GameObject affected by the action
        public ActionBase actionHandler; // The action to perform
    }

    [Tooltip("List of ray receivers.")] public List<RayReceiver> rayReceivers;

    [Tooltip("Prefab for line renderer to draw reflections.")]
    public LineRenderer reflectionLineRendererPrefab; // Prefab for line renderer to draw reflections

    [Tooltip("Limit for recursive reflections.")]
    public int reflectionLimit = 5; // Limit for recursive reflections

    private LineRenderer CreateLineRenderer(Vector3 start, Vector3 end)
    {
        LineRenderer lineRenderer;
        if (reflectionLineRendererPrefab != null)
        {
            lineRenderer = Instantiate(reflectionLineRendererPrefab, transform);
        }
        else
        {
            GameObject reflectionLineObject = new GameObject("ReflectionLine");
            reflectionLineObject.transform.SetParent(transform);
            lineRenderer = reflectionLineObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Standard"));
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
        }

        // Set positions for the line renderer
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        return lineRenderer;
    }


    public void ProcessRayHit(Vector3 hitPoint, Ray incomingRay, Vector3 normal, int rayIndex)
    {
        // Destroy all existing lines first
        foreach (Transform child in transform)
        {
            if (child.GetComponent<LineRenderer>())
            {
                Destroy(child.gameObject);
            }
        }

        bool rayHitReceiver = false;
        RaycastHit hit;
        if (Physics.Raycast(incomingRay, out hit))
        {
            foreach (RayReceiver receiver in rayReceivers)
            {
                if (receiver.rayReceiver != null)
                {
                    // Check if the ray hit the intended receiver (interceptor)
                    if (hit.collider.gameObject == receiver.rayReceiver)
                    {
                        rayHitReceiver = true;
                        if (receiver.enableReflection)
                        {
                            ReflectRay(hitPoint, incomingRay.direction, normal, 0);
                        }

                        // Perform actions on the associated target objects
                        foreach (ActionableObject action in receiver.actionableObjects)
                        {
                            action.actionHandler.ExecuteAction(action.targetObject, incomingRay);
                        }
                    }
                }
            }
        }

        if (!rayHitReceiver)
        {
            // Reset reflection or any other logic when the ray did not hit a receiver
            ResetAllRayHits();
        }
    }



    private void ReflectRay(Vector3 origin, Vector3 direction, Vector3 normal, int reflectionCount)
    {
        if (reflectionCount >= reflectionLimit) return; // Terminate if the reflection limit is reached

        Ray ray = new Ray(origin, Vector3.Reflect(direction, normal));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Create or instantiate the line renderer for the reflection
            LineRenderer lineRenderer = CreateLineRenderer(origin, hit.point);

            // Check if the ray hit a receiver that has reflection enabled
            foreach (RayReceiver receiver in rayReceivers)
            {
                if (hit.transform.gameObject == receiver.rayReceiver)
                {
                    // Reflect the ray if reflection is enabled
                    if (receiver.enableReflection)
                    {
                        ReflectRay(hit.point, ray.direction, hit.normal, reflectionCount + 1);
                    }

                    // Perform actions on the associated target objects
                    foreach (ActionableObject action in receiver.actionableObjects)
                    {
                        action.actionHandler.ExecuteAction(action.targetObject, ray);
                    }
                }
            }
        }
    }


    public void ResetRayHit(GameObject rayReceiverGameObject)
    {
        // Find the corresponding RayReceiver for this GameObject
        foreach (RayReceiver receiver in rayReceivers)
        {
            if (receiver.rayReceiver == rayReceiverGameObject)
            {
                foreach (ActionableObject action in receiver.actionableObjects)
                {
                    action.actionHandler.RevertAction(action.targetObject);
                }

                // Add other reset logic here as needed
                break;
            }
        }
    }

    public void ResetAllRayHits()
    {
        foreach (RayReceiver receiver in rayReceivers)
        {
            if (receiver.rayReceiver != null)
            {
                ResetRayHit(receiver.rayReceiver);
            }
        }
    }
}