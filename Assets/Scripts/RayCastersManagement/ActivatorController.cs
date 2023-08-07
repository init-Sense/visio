using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class is used to control the activator object.
/// The activator object is used to activate ray casting groups, which are used to activate ray casting origins.
/// </summary>
public class ActivatorController : XRGrabInteractable
{
    [Tooltip("Assign the RayActivationController object here.")] [SerializeField]
    private RayActivationController rayActivationController;

    [Tooltip("Chooses the group index to activate when this activator is grabbed.")] [SerializeField]
    private int groupIndex = 0;

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

    protected override void Awake()
    {
        base.Awake();
        _activatorRenderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>(); // Get the rigidbody
        if (_activatorRenderer != null)
        {
            _activatorOriginalMaterial = _activatorRenderer.material; // Store the original material
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
            // Float and rotate the activator towards the target transform.
            transform.position = Vector3.Lerp(transform.position, _targetTransform.position,
                Time.deltaTime * centeringSpeed);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Activate ray if it's not already active
            if (!_isRayActive)
            {
                rayActivationController.ActivateRaycasting(groupIndex);
                _isRayActive = true;
            }
        }
        else if (_isRayActive) // Deactivate ray if previously active
        {
            rayActivationController.DeactivateRaycasting(groupIndex);
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
            _isInsideTrigger = true;
            _rigidbody.useGravity = false; // Disable gravity
            _rigidbody.velocity = Vector3.zero; // Zero velocity
            _rigidbody.angularVelocity = Vector3.zero; // Zero angular velocity
            _activatorRenderer.material = activatorGlowMaterial; // Set the glowing material

            // Find the child transform with the name "Hovering Point"
            _targetTransform = other.transform.Find("Hovering Point");
            if (_targetTransform == null)
            {
                Debug.LogError("No child transform named 'Hovering Point' found under ActivatorFloatingArea.");
            }
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ActivatorFloatingArea"))
        {
            Debug.Log("Exiting ActivatorFloatingArea, enabling gravity");
            _isInsideTrigger = false;
            _rigidbody.useGravity = true; // Enable gravity
            _activatorRenderer.material = _activatorOriginalMaterial; // Restore the original material
            _targetTransform = null; // Reset the target transform

            rayActivationController.DeactivateRaycasting(groupIndex);
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _isInsideTrigger = false;
        _rigidbody.useGravity = true; // Enable gravity
        _targetTransform = null; // Reset the target transform

        _isRayActive = false;

        rayActivationController.DeactivateRaycasting(groupIndex);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Debug.Log("OnSelectExited called, enabling gravity");
        _rigidbody.useGravity = true; // Enable gravity
        _targetTransform = null; // Reset the target transform

        _isRayActive = false;

        rayActivationController.DeactivateRaycasting(groupIndex);
    }
}