using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class is used to control the activator object.
/// The activator object is used to activate ray casting.
/// </summary>
public class ActivatorController : XRGrabInteractable
{
    [Tooltip("Assign the RayActivationController object here.")] [SerializeField]
    private RayActivationController rayActivationController;

    [Tooltip("Adjust the rotation speed of the activator object.")]
    public float rotationSpeed = 50f;

    [Tooltip("Adjust the centering speed of the activator object.")]
    public float centeringSpeed = 2f;

    [Tooltip("Assign a material for the activator when triggered. This is optional.")]
    public Material activatorGlowMaterial;

    private Material _activatorOriginalMaterial;
    private MeshRenderer _activatorRenderer;
    private Rigidbody _rigidbody;
    private bool _isInsideTrigger = false;
    private Transform _targetTransform;

    private bool _isRayActive = false;
    public AreaController areaController;

    private bool isSelectExitedCalled = false;

    protected override void Awake()
    {
        base.Awake();
        _activatorRenderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
        if (_activatorRenderer != null)
        {
            _activatorOriginalMaterial = _activatorRenderer.material;
        }
        else
        {
            Debug.LogError("No MeshRenderer found on the activator object.");
        }
    }

    void FixedUpdate()
    {
        if (_targetTransform == null || !_isInsideTrigger)
            return;

        if (_isInsideTrigger)
        {
            // Float and rotate the activator towards the target transform.
            transform.position = Vector3.Lerp(transform.position, _targetTransform.position,
                Time.deltaTime * centeringSpeed);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Activate ray if it's not already active
            if (!_isRayActive)
            {
                rayActivationController.ActivateRaycasting();
                _isRayActive = true;
            }
        }
        else if (_isRayActive) // Deactivate ray if previously active
        {
            rayActivationController.DeactivateRaycasting();
            _isRayActive = false;
        }

        if (_rigidbody.useGravity)
        {
            Debug.Log("Gravity is enabled");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ActivatorFloatingArea"))
        {
            if (other.transform.childCount > 0)
            {
                _isInsideTrigger = true;
                _rigidbody.useGravity = false;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _activatorRenderer.material = activatorGlowMaterial;

                _targetTransform = other.transform.Find("Hovering Point");
                if (_targetTransform == null)
                {
                    Debug.LogError("No child transform named 'Hovering Point' found under ActivatorFloatingArea.");
                }
            }
            else
            {
                Debug.LogError("No child transform found under ActivatorFloatingArea.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ActivatorFloatingArea"))
        {
            Debug.Log("Exiting ActivatorFloatingArea, enabling gravity");
            _isInsideTrigger = false;
            _rigidbody.useGravity = true;
            _activatorRenderer.material = _activatorOriginalMaterial;
            _targetTransform = null;

            rayActivationController.DeactivateRaycasting();
            rayActivationController.rayProcessingController.ResetRayHit(); // Explicitly reset ray hit
        }
    }


    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Debug.Log("OnSelectEntered called, disabling gravity");
        _rigidbody.useGravity = false;
        _targetTransform = args.interactable.transform;

        _isRayActive = true;

        isSelectExitedCalled = false;
    }

protected override void OnSelectExited(SelectExitEventArgs args)
{
    if (isSelectExitedCalled)
    {
        return;
    }

    base.OnSelectExited(args);
    Debug.Log("OnSelectExited called, enabling gravity");
    _rigidbody.useGravity = true;
    _targetTransform = null;

    _isRayActive = false;

    StartCoroutine(DeactivateRaycastingWithDelay());

    isSelectExitedCalled = true;
}


    private IEnumerator DeactivateRaycastingWithDelay()
    {
        yield return new WaitForSeconds(1f); // adjust the delay as needed
        rayActivationController.DeactivateRaycasting();
        _isRayActive = false;
    }
}