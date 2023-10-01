using UnityEngine;
using System.Collections;

public class RayceiverSphereController : MonoBehaviour
{
    [Tooltip("List of game objects to activate when hit by a ray.")]
    public GameObject[] gameObjectsToActivate;

    private MeshRenderer meshRenderer;
    private bool isActivated = false;

    // Declare the rayActivationController variable
    private RayActivationController rayActivationController;
    // Declare the rayProcessingController variable
    private RayProcessingController rayProcessingController;

    private Coroutine deactivationCoroutine;
    private bool activationInProgress = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Initialize the rayActivationController variable
        rayActivationController = FindObjectOfType<RayActivationController>();
        // Initialize the rayProcessingController variable
        rayProcessingController = FindObjectOfType<RayProcessingController>();
    }

    private void Update()
    {
        // Check ray from RayActivationController
        Ray rayActivation = new Ray(rayActivationController.raycastOrigin.position, rayActivationController.raycastOrigin.forward);
        RaycastHit hitActivation;
        bool hitActivationController = Physics.Raycast(rayActivation, out hitActivation, Mathf.Infinity, rayActivationController.raycastMask);

        // Check ray from RayProcessingController
        Ray rayProcessing = new Ray(rayProcessingController.transform.position, rayProcessingController.transform.forward);
        RaycastHit hitProcessing;
        bool hitProcessingController = Physics.Raycast(rayProcessing, out hitProcessing, Mathf.Infinity, rayProcessingController.raycastMask);

        if (hitActivationController && hitActivation.collider.gameObject == gameObject)
        {
            Activate();
        }
        else if (hitProcessingController && hitProcessing.collider.gameObject == gameObject)
        {
            Activate();
        }
        else if (!activationInProgress)
        {
            DeactivateAfterDelay();
        }
    }

    public void Activate()
    {
        if (!isActivated)
        {
            Debug.Log("Activating game objects");
            activationInProgress = true;
            foreach (GameObject go in gameObjectsToActivate)
            {
                go.SetActive(true);
            }
            isActivated = true;
            activationInProgress = false;
        }
        else
        {
            Debug.Log("Game objects are already active");
        }
    }

    public void Deactivate()
    {
        if (isActivated)
        {
            Debug.Log("Deactivating game objects");
            foreach (GameObject go in gameObjectsToActivate)
            {
                go.SetActive(false);
            }
            isActivated = false;
        }
        else
        {
            Debug.Log("Game objects are already inactive");
        }
    }

    private void DeactivateAfterDelay()
    {
        if (isActivated && deactivationCoroutine == null)
        {
            deactivationCoroutine = StartCoroutine(DeactivationCoroutine());
        }
    }

    private IEnumerator DeactivationCoroutine()
    {
        yield return new WaitForSeconds(0.1f); // Adjust the delay duration as needed

        // Check if the ray is still hitting the sphere
        Ray rayActivation = new Ray(rayActivationController.raycastOrigin.position, rayActivationController.raycastOrigin.forward);
        RaycastHit hitActivation;
        bool hitActivationController = Physics.Raycast(rayActivation, out hitActivation, Mathf.Infinity, rayActivationController.raycastMask);

        Ray rayProcessing = new Ray(rayProcessingController.transform.position, rayProcessingController.transform.forward);
        RaycastHit hitProcessing;
        bool hitProcessingController = Physics.Raycast(rayProcessing, out hitProcessing, Mathf.Infinity, rayProcessingController.raycastMask);

        if (!hitActivationController && !hitProcessingController)
        {
            Deactivate();
        }

        deactivationCoroutine = null;
    }
}
