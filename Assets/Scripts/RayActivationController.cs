using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayActivationController : MonoBehaviour
{
    public Transform raycastOrigin; // The point from where the ray should be cast.
    public Transform attachPoint; // The point where the sphere should attach.
    public LayerMask raycastMask; // The layers that the raycast can hit.

    public GameObject activatorObject; // Assign your activator object here from Inspector.

    public RayProcessingController rayProcessingController; // The controller that will perform actions when the ray hits.

    private XRGrabInteractable _grabInteractable;
    private Rigidbody _grabInteractableRb;

    private LineRenderer _lineRenderer;
    private bool _isActivated = false;

    void Start()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _lineRenderer = GetComponent<LineRenderer>();

        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = new Material(Shader.Find("Standard"));
            _lineRenderer.startColor = Color.red;
            _lineRenderer.endColor = Color.red;
            _lineRenderer.startWidth = 0.01f;
            _lineRenderer.endWidth = 0.01f;
        }

        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
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
            rayProcessingController.ResetRayHit(rayProcessingController.rayReceiver);
        }
    }

    void Update()
    {
        if (_isActivated)
        {
            Raycast();
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }

    void Raycast()
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask))
        {
            _lineRenderer.SetPosition(0, raycastOrigin.position);
            _lineRenderer.SetPosition(1, hit.point);

            if (hit.transform.gameObject == rayProcessingController.rayReceiver)
            {
                rayProcessingController.ProcessRayHit(hit.transform.gameObject);
            }
        }
        else
        {
            _lineRenderer.SetPosition(0, raycastOrigin.position);
            _lineRenderer.SetPosition(1, raycastOrigin.position + raycastOrigin.forward * 1000);
        }

        _lineRenderer.enabled = true;
    }
}
