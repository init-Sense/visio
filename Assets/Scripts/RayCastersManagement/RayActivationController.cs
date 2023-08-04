using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayActivationController : XRGrabInteractable
{
    public List<Transform> raycastOrigins;
    public Collider activationTrigger;
    public LayerMask raycastMask;
    public GameObject activatorObject;
    public List<RayProcessingController> rayProcessingControllers;
    private Rigidbody _activatorRb;
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    private bool _isInsideTrigger = false;

    public float rotationSpeed = 50f; // in degrees per second
    public Material activatorGlowMaterial; // the glowing material
    private Material _activatorOriginalMaterial; // to store the original material
    private MeshRenderer _activatorRenderer; // the renderer of the activator object

    public float centeringSpeed = 2f; // adjust as needed


    protected override void Awake()
    {
        base.Awake();
        _activatorRb = activatorObject.GetComponent<Rigidbody>();

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

        _activatorRenderer = activatorObject.GetComponent<MeshRenderer>();
        if (_activatorRenderer != null)
        {
            _activatorOriginalMaterial = _activatorRenderer.material; // store the original material
        }
        else
        {
            Debug.LogError("No MeshRenderer found on the activator object.");
        }
    }

    void FixedUpdate()
    {
        if (_isInsideTrigger)
        {
            float step = centeringSpeed * Time.deltaTime; // calculate distance to move
            activatorObject.transform.position = Vector3.MoveTowards(activatorObject.transform.position,
                activationTrigger.bounds.center, step);
            activatorObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == activatorObject)
        {
            _isInsideTrigger = true;
            _activatorRb.useGravity = false;  // here gravity is deactivated
            _activatorRb.velocity = Vector3.zero;
            _activatorRb.angularVelocity = Vector3.zero;
            if (_activatorRenderer != null)
            {
                _activatorRenderer.material = activatorGlowMaterial; // set the glowing material
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == activatorObject)
        {
            _isInsideTrigger = false;
            _activatorRb.useGravity = true;  // here gravity is activated again
            ResetAllRays();
            if (_activatorRenderer != null)
            {
                _activatorRenderer.material = _activatorOriginalMaterial; // restore the original material
            }
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _isInsideTrigger = false;
        _activatorRb.useGravity = true;  // gravity is also activated if the user grabs the object
    }

    void Update()
    {
        if (_isInsideTrigger)
        {
            for (int i = 0; i < raycastOrigins.Count; i++)
            {
                Raycast(raycastOrigins[i], _lineRenderers[i], i);
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

    void ResetAllRays()
    {
        foreach (RayProcessingController controller in rayProcessingControllers)
        {
            if (controller.rayReceiver != null)
            {
                controller.ResetRayHit(controller.rayReceiver);
            }
        }
    }
}