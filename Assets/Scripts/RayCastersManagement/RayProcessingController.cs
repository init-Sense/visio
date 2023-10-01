using System;
using UnityEngine;

public class RayProcessingController : MonoBehaviour
{
    public enum ReceiverState
    {
        Idle,
        ActionExecuted,
        PartialHit // Remove if not needed
    }

    [Tooltip("Line renderer prefab for reflections.")]
    public LineRenderer reflectionLineRendererPrefab;

    [Tooltip("Limit for recursive reflections.")]
    public int reflectionLimit = 5;

    [HideInInspector] public int rayHits = 0;
    [HideInInspector] public bool isActivated = false;

    private ReceiverState receiverState = ReceiverState.Idle;
    private LineRenderer currentReflectionLine;
    public Material reflectionMaterial;
    private RayActivationController rayActivationController;

    public LayerMask raycastMask;

    private void Start()
    {
        rayActivationController = FindObjectOfType<RayActivationController>();
    }

    private void FixedUpdate()
    {
        switch (receiverState)
        {
            case ReceiverState.Idle:
                CheckPrimaryRayHit();
                break;
            case ReceiverState.ActionExecuted:
                CheckRayExit();
                break;
            case ReceiverState.PartialHit:
                // Handle partial hit logic if needed
                break;
        }
    }

    private void CheckPrimaryRayHit()
    {
        Ray ray = new Ray(rayActivationController.raycastOrigin.position,
            rayActivationController.raycastOrigin.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, rayActivationController.raycastMask);

        if (hitSomething && hit.collider.gameObject == gameObject)
        {
            receiverState = ReceiverState.ActionExecuted;
            ProcessRayHit(hit.point, ray, hit.normal);
        }
    }

    private void CheckRayExit()
    {
        Ray ray = new Ray(rayActivationController.raycastOrigin.position,
            rayActivationController.raycastOrigin.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, rayActivationController.raycastMask);

        if (!hitSomething || hit.collider.gameObject != gameObject)
        {
            ResetRayHit();
        }
    }

    public void ProcessRayHit(Vector3 hitPoint, Ray incomingRay, Vector3 normal)
    {
        // Handle reflection after processing the hit.
        HandleReflection(hitPoint, incomingRay.direction, normal);
    }

    private void HandleReflection(Vector3 hitPoint, Vector3 incomingDirection, Vector3 normal)
    {
        if (currentReflectionLine == null)
        {
            currentReflectionLine = CreateReflectionLineRenderer(hitPoint, hitPoint, reflectionMaterial);
        }

        ReflectRay(hitPoint, incomingDirection, normal, 0);
    }

    private void ReflectRay(Vector3 origin, Vector3 direction, Vector3 normal, int reflectionCount)
    {
        if (reflectionCount >= reflectionLimit) return;

        Vector3 reflectedDirection = Vector3.Reflect(direction, normal);
        Ray ray = new Ray(origin, reflectedDirection);
        RaycastHit hit;

        Vector3 endPoint = origin + reflectedDirection * 1000; // default end point if no hit

        if (Physics.Raycast(ray, out hit))
        {
            endPoint = hit.point;

            // Check for RayProcessingController
            RayProcessingController hitReceiver = hit.collider.gameObject.GetComponent<RayProcessingController>();
            if (hitReceiver != null && hitReceiver.receiverState != ReceiverState.ActionExecuted)
            {
                hitReceiver.ProcessRayHit(hit.point, ray, hit.normal);
            }

            // Check for RayceiverSphereController
            RayceiverSphereController hitSphereReceiver = hit.collider.gameObject.GetComponent<RayceiverSphereController>();
            if (hitSphereReceiver != null)
            {
                hitSphereReceiver.Activate();
            }

            ReflectRay(hit.point, reflectedDirection, hit.normal, reflectionCount + 1);
        }

        if (currentReflectionLine != null)
        {
            currentReflectionLine.SetPosition(0, origin);
            currentReflectionLine.SetPosition(1, endPoint);
        }
    }


    public LineRenderer CreateReflectionLineRenderer(Vector3 start, Vector3 end, Material material = null)
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
            lineRenderer.material = material != null ? material : new Material(Shader.Find("Standard"));
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        return lineRenderer;
    }

    public void DestroyReflectionLine()
    {
        if (currentReflectionLine != null)
        {
            Destroy(currentReflectionLine.gameObject);
            currentReflectionLine = null;
        }
    }

    public void ResetRayHit()
    {
        Debug.Log($"Resetting ray hits for receiver {gameObject.name}");

        receiverState = ReceiverState.Idle;
        rayHits = 0;
        isActivated = false;

        DestroyReflectionLine();
    }

    private void OnEnable()
    {
        ResetRayHit();
    }
}
