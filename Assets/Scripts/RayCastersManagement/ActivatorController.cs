using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ActivatorController : XRGrabInteractable
{
    public float rotationSpeed = 50f; // Rotation speed in degrees per second
    public float centeringSpeed = 2f; // Adjust as needed
    public Material activatorGlowMaterial; // The glowing material
    private Material _activatorOriginalMaterial; // To store the original material
    private MeshRenderer _activatorRenderer; // The renderer of the activator object
    private Rigidbody _rigidbody; // The rigidbody of the activator object
    private bool _isInsideTrigger = false; // To check if inside the trigger
    private Transform _targetTransform; // The target transform for floating

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
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _isInsideTrigger = false;
        _rigidbody.useGravity = true; // Enable gravity
        _targetTransform = null; // Reset the target transform
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Debug.Log("OnSelectExited called, enabling gravity");
        _rigidbody.useGravity = true; // Enable gravity
        _targetTransform = null; // Reset the target transform
    }
}