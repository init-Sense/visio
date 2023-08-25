using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class controls the processing of ray hits.
/// Each ray receiver has a list of actionable objects and a flag to enable/disable the reflection of the ray.
/// The ray receivers are activated/deactivated by the RayActivationController.
/// </summary>
public class RayProcessingController : MonoBehaviour
{
    public enum ReceiverState
    {
        Idle,
        RayHitting,
        ActionExecuted
    }

    private Dictionary<GameObject, ReceiverState> receiverStates = new Dictionary<GameObject, ReceiverState>();

    [Tooltip("List of ray receivers.")]
    [System.Serializable]
    public class RayReceiver
    {
        public GameObject rayReceiver;
        public bool enableReflection;
        public List<ActionableObject> actionableObjects;
        public int reflectionLimit;
        public int requiredHits = 1; // default to 1 for backward compatibility
        [HideInInspector] public List<int> rayHits = new List<int>();
        [HideInInspector] public bool isActivated = false;
    }

    [Tooltip("List of actions to perform on hit.")]
    [System.Serializable]
    public class ActionableObject
    {
        public GameObject targetObject;
        public ActionBase actionBase;
        public float transparencyIncrement = 0.1f; // Default value
    }

    public List<RayReceiver> rayReceivers;
    public LineRenderer reflectionLineRendererPrefab;
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


    public void ProcessRayHit(Vector3 hitPoint, Ray incomingRay, Vector3 normal, int rayIndex, int uniqueRayId)
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
                        if (!receiver.rayHits.Contains(uniqueRayId))
                        {
                            receiver.rayHits.Add(uniqueRayId);
                            Debug.Log($"Ray {uniqueRayId} hit receiver {receiver.rayReceiver.name}. Total hits: {receiver.rayHits.Count}");
                        }

                        if (!receiverStates.ContainsKey(receiver.rayReceiver))
                        {
                            receiverStates[receiver.rayReceiver] = ReceiverState.RayHitting;
                        }

                        Debug.Log(
                            $"Ray {uniqueRayId} hit receiver {receiver.rayReceiver.name}. Total hits: {receiver.rayHits.Count}");

                        if (receiver.rayHits.Count >= receiver.requiredHits &&
                            receiverStates[receiver.rayReceiver] != ReceiverState.ActionExecuted)
                        {
                            Debug.Log($"Receiver {receiver.rayReceiver.name} achieved required hits.");

                            receiver.isActivated = true;

                            // Execute actions and maybe reset the rayHits list if needed
                            foreach (ActionableObject action in receiver.actionableObjects)
                            {
                                action.actionBase.ExecuteAction(action.targetObject, incomingRay,
                                    action.transparencyIncrement);
                            }

                            receiver.rayHits.Clear();

                            // Update the state
                            receiverStates[receiver.rayReceiver] = ReceiverState.ActionExecuted;
                        }

                        if (receiver.enableReflection)
                        {
                            ReflectRay(hitPoint, incomingRay.direction, normal, 0, uniqueRayId);
                        }
                    }
                }
            }
        }


        if (!rayHitReceiver)
        {
            foreach (RayReceiver receiver in rayReceivers)
            {
                if (receiver.isActivated)
                {
                    ResetRayHit(receiver.rayReceiver);
                    receiver.isActivated = false;
                }
            }
        }
    }


    private void ReflectRay(Vector3 origin, Vector3 direction, Vector3 normal, int reflectionCount, int uniqueRayId)
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
                    if (!receiver.rayHits.Contains(uniqueRayId))
                    {
                        // Reflect the ray if reflection is enabled
                        if (receiver.enableReflection)
                        {
                            ReflectRay(hit.point, ray.direction, hit.normal, reflectionCount + 1, uniqueRayId);
                        }

                        // Perform actions on the associated target objects
                        foreach (ActionableObject action in receiver.actionableObjects)
                        {
                            action.actionBase.ExecuteAction(action.targetObject, ray);
                        }
                    }
                }
            }
        }
    }


    public void ResetRayHit(GameObject rayReceiverGameObject)
    {
        Debug.Log($"Attempting to reset ray hits for receiver {rayReceiverGameObject.name}");

        if (receiverStates.ContainsKey(rayReceiverGameObject) &&
            receiverStates[rayReceiverGameObject] == ReceiverState.RayHitting)
        {
            Debug.Log($"Resetting ray hits for receiver {rayReceiverGameObject.name}");

            // Find the corresponding RayReceiver for this GameObject
            foreach (RayReceiver receiver in rayReceivers)
            {
                if (receiver.rayReceiver == rayReceiverGameObject)
                {
                    foreach (ActionableObject action in receiver.actionableObjects)
                    {
                        action.actionBase.RevertAction(action.targetObject);
                    }

                    // Reset state to Idle after actions have been reverted
                    receiverStates[rayReceiverGameObject] = ReceiverState.Idle;

                    // Add other reset logic here as needed
                    break;
                }
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

    private void OnEnable()
    {
        ResetAllRayHits();
    }
}