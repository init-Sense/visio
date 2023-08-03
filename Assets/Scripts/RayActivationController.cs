using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayActivationController : MonoBehaviour
{
    public List<Transform> raycastOrigins; // Replaced raycastOrigin with a list
    public Transform attachPoint;
    public LayerMask raycastMask;

    public GameObject activatorObject;

    public List<RayProcessingController> rayProcessingControllers;

    private XRGrabInteractable _grabInteractable;
    private Rigidbody _grabInteractableRb;

    private List<LineRenderer> _lineRenderers = new List<LineRenderer>(); // List of LineRenderers for each ray
    private bool _isActivated = false;

    void Start()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();

        // Initialize a LineRenderer for each raycast origin.
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == activatorObject)
        {
            _isActivated = true;
            _grabInteractableRb = other.gameObject.GetComponent<Rigidbody>();
            _grabInteractableRb.isKinematic = true;
            other.gameObject.transform.position = attachPoint.position;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == activatorObject)
        {
            _isActivated = false;
            _grabInteractableRb.isKinematic = false;

            foreach (RayProcessingController controller in rayProcessingControllers)
            {
                controller.ResetRayHit(controller.rayReceiver);
            }
        }
    }


    void Update()
    {
        if (_isActivated)
        {
            for (int i = 0; i < raycastOrigins.Count; i++)
            {
                Raycast(raycastOrigins[i], _lineRenderers[i], i); // Pass the index
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


    void Raycast(Transform raycastOrigin, LineRenderer lineRenderer, int rayIndex) // rayIndex is added as parameter
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask))
        {
            lineRenderer.SetPosition(0, raycastOrigin.position);
            lineRenderer.SetPosition(1, hit.point);

            foreach (RayProcessingController controller in rayProcessingControllers)
            {
                if (hit.transform.gameObject == controller.rayReceiver)
                {
                    controller.ProcessRayHit(hit.point, ray, hit.normal, rayIndex); // rayIndex is passed
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(0, raycastOrigin.position);
            lineRenderer.SetPosition(1, raycastOrigin.position + raycastOrigin.forward * 1000);

            foreach (RayProcessingController controller in rayProcessingControllers)
            {
                controller.ResetRayHit(controller.rayReceiver);
            }
        }

        lineRenderer.enabled = true;
    }

}