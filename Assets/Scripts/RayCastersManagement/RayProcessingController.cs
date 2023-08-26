using System;
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
        ActionExecuted,
        PartialHit
    }

    private Dictionary<GameObject, ReceiverState> receiverStates = new Dictionary<GameObject, ReceiverState>();

    [Tooltip("List of ray receivers.")]
    [System.Serializable]
    public class RayReceiver
    {
        public GameObject rayReceiver;
        public bool enableReflection;
        public List<ActionableObject> actionableObjects;
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

    private bool isProcessingHit = false;

    // Receiver initialization
    private void Start()
    {
        InitializeReceiverStates();
    }

    private void InitializeReceiverStates()
    {
        foreach (RayReceiver receiver in rayReceivers)
        {
            if (receiver.rayReceiver != null)
            {
                receiverStates[receiver.rayReceiver] = ReceiverState.Idle;
            }
        }
    }


    // Standard ray hit

    public void ProcessRayHit(Vector3 hitPoint, Ray incomingRay, Vector3 normal, int rayIndex, int uniqueRayId)
    {
        if (isProcessingHit)
        {
            return;
        }

        isProcessingHit = true;

        DestroyExistingLines();

        bool rayHitReceiver = ProcessRayHitOnReceiver(hitPoint, incomingRay, normal, uniqueRayId);

        if (!rayHitReceiver)
        {
            ResetActivatedReceivers();
        }

        isProcessingHit = false;
    }

    private bool ProcessRayHitOnReceiver(Vector3 hitPoint, Ray incomingRay, Vector3 normal, int uniqueRayId)
    {
        bool rayHitReceiver = false;
        RaycastHit hit;
        if (Physics.Raycast(incomingRay, out hit))
        {
            foreach (RayReceiver receiver in rayReceivers)
            {
                if (receiver.rayReceiver != null && hit.collider.gameObject == receiver.rayReceiver)
                {
                    rayHitReceiver = true;
                    HandleRayHit(receiver, incomingRay, normal, uniqueRayId, hitPoint);
                }
            }
        }

        return rayHitReceiver;
    }

    private void HandleRayHit(RayReceiver receiver, Ray incomingRay, Vector3 normal, int uniqueRayId, Vector3 hitPoint)
    {
        if (!receiver.rayHits.Contains(uniqueRayId))
        {
            receiver.rayHits.Add(uniqueRayId);
            Debug.Log(
                $"Ray {uniqueRayId} hit receiver {receiver.rayReceiver.name}. Total hits: {receiver.rayHits.Count}, Required hits: {receiver.requiredHits}");

            ReceiverState currentState;
            if (receiverStates.TryGetValue(receiver.rayReceiver, out currentState))
            {
                if (receiver.rayHits.Count >= receiver.requiredHits && currentState != ReceiverState.ActionExecuted)
                {
                    ExecuteActionsAndResetRayHits(receiver, incomingRay);
                    receiverStates[receiver.rayReceiver] = ReceiverState.ActionExecuted;
                }
                else if (receiver.rayHits.Count < receiver.requiredHits)
                {
                    RevertActions(receiver);
                    receiverStates[receiver.rayReceiver] = ReceiverState.PartialHit;
                }

                if (receiver.enableReflection)
                {
                    ReflectRay(hitPoint, incomingRay.direction, normal, 0, uniqueRayId);
                }
            }
        }
    }


    private void ExecuteActionsAndResetRayHits(RayReceiver receiver, Ray incomingRay)
    {
        Debug.Log($"Receiver {receiver.rayReceiver.name} achieved required hits.");

        // Update the state first
        receiverStates[receiver.rayReceiver] = ReceiverState.ActionExecuted;

        // Set the activation flag
        receiver.isActivated = true;

        // Execute the actions
        try
        {
            foreach (ActionableObject action in receiver.actionableObjects)
            {
                action.actionBase.ExecuteAction(action.targetObject, incomingRay, action.transparencyIncrement);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error executing actions for receiver {receiver.rayReceiver.name}: {e.Message}");
            // Handle the error, e.g., revert the state or actions
            receiverStates[receiver.rayReceiver] = ReceiverState.PartialHit;
            RevertActions(receiver);
            return;
        }

        // Reset the hit count if you want to count subsequent hits from the same ray
        receiver.rayHits.Clear();
    }

    // Reflections

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

    // Resetters

    public void ResetRayHit(GameObject rayReceiverGameObject)
    {
        Debug.Log($"Attempting to reset ray hits for receiver {rayReceiverGameObject.name}");

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
                receiver.rayHits.Clear();

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

    private void DestroyExistingLines()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<LineRenderer>())
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void RevertActions(RayReceiver receiver)
    {
        foreach (ActionableObject action in receiver.actionableObjects)
        {
            action.actionBase.RevertAction(action.targetObject);
        }
    }

    private void ResetActivatedReceivers()
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

    private void OnEnable()
    {
        ResetAllRayHits();
    }
}