using UnityEngine;
using System.Collections;

public class RayceiverSphereController : MonoBehaviour
{
    [Tooltip("List of game objects to activate when hit by a ray.")]
    public GameObject[] gameObjectsToActivate;

    private MeshRenderer meshRenderer;
    private bool isActivated = false;
    private bool isReceiverHit = false;

    private RayActivationController rayActivationController;
    private RayProcessingController rayProcessingController;

    private Coroutine deactivationCoroutine;
    private bool activationInProgress = false;

    private float deactivationCooldown = 0.5f; // Cooldown time in seconds
    private float lastDeactivationTime;

    private enum ReceiverState
    {
        Inactive,
        Activating,
        Active,
        Deactivating
    }

    private ReceiverState currentState = ReceiverState.Inactive;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        rayActivationController = FindObjectOfType<RayActivationController>();
        rayProcessingController = FindObjectOfType<RayProcessingController>();
    }

    private void Update()
    {
        Ray rayActivation = new Ray(rayActivationController.raycastOrigin.position, rayActivationController.raycastOrigin.forward);
        RaycastHit hitActivation;
        bool hitActivationController = Physics.Raycast(rayActivation, out hitActivation, Mathf.Infinity, rayActivationController.raycastMask);

        Ray rayProcessing = new Ray(rayProcessingController.transform.position, rayProcessingController.transform.forward);
        RaycastHit hitProcessing;
        bool hitProcessingController = Physics.Raycast(rayProcessing, out hitProcessing, Mathf.Infinity, rayProcessingController.raycastMask);

        if (hitActivationController && hitActivation.collider.gameObject == gameObject)
        {
            isReceiverHit = true;
            if (currentState == ReceiverState.Inactive || currentState == ReceiverState.Deactivating)
            {
                Activate();
            }
        }
        else if (hitProcessingController && hitProcessing.collider.gameObject == gameObject)
        {
            isReceiverHit = true;
            if (currentState == ReceiverState.Inactive || currentState == ReceiverState.Deactivating)
            {
                Activate();
            }
        }
        else
        {
            isReceiverHit = false;
            if (!activationInProgress)
            {
                DeactivateAfterDelay();
            }
        }
    }

    public void Activate()
    {
        if (currentState == ReceiverState.Inactive)
        {
            Debug.Log("Activating game objects");
            currentState = ReceiverState.Activating;
            activationInProgress = true;
            foreach (GameObject go in gameObjectsToActivate)
            {
                go.SetActive(true);
            }
            currentState = ReceiverState.Active;
            isActivated = true;
            activationInProgress = false;
        }
        else if (currentState == ReceiverState.Deactivating)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
            currentState = ReceiverState.Active;
            isActivated = true;
        }
        else
        {
            Debug.Log("Game objects are already active");
        }
    }

    public void Deactivate()
    {
        if (currentState == ReceiverState.Active)
        {
            Debug.Log("Deactivating game objects");
            currentState = ReceiverState.Deactivating;
            deactivationCoroutine = StartCoroutine(DeactivationCoroutine());
        }
        else
        {
            Debug.Log("Game objects are already inactive");
        }
    }

    private void DeactivateAfterDelay()
    {
        if (isActivated && !isReceiverHit && Time.time - lastDeactivationTime >= deactivationCooldown)
        {
            lastDeactivationTime = Time.time;
            Deactivate();
        }
    }

    private IEnumerator DeactivationCoroutine()
    {
        yield return new WaitForSeconds(deactivationCooldown);
        foreach (GameObject go in gameObjectsToActivate)
        {
            go.SetActive(false);
        }
        currentState = ReceiverState.Inactive;
        isActivated = false;
        deactivationCoroutine = null;
    }
}
