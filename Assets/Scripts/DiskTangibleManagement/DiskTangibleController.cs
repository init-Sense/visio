using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class DiskTangibleController : XRGrabInteractable
{
    private bool _isInsideTrigger = false;

    private Vector3 _startPosition;
    private Rigidbody _rigidbody;

    [Tooltip("This is where the disk will actually float to.")]
    [SerializeField]
    public Transform hoverPoint;

    [Tooltip("This is the object that will be disabled when the disk enters its associated trigger.")]
    [SerializeField]
    public GameObject activatedObject;

    [Tooltip("Assign a material for the activated object when the disk enters its associated trigger.")]
    [SerializeField]
    public Material transparentMaterial;

    [Tooltip("Material to apply when the disk enters the trigger.")]
    [SerializeField]
    public Material glowOn;

    [Tooltip("Material to apply when the disk exits the trigger.")]
    [SerializeField]
    public Material glowOff;

    [Tooltip("GameObject to apply the glow materials to.")]
    [SerializeField]
    public GameObject glowObject;

    private Material _originalMaterial;
    private MeshRenderer _activatedObjectMeshRenderer;
    private Collider _floatingAreaCollider;

    private void SetMaterial(Material material)
    {
        if (_activatedObjectMeshRenderer)
        {
            _activatedObjectMeshRenderer.material = material;
        }
        else
        {
            Debug.LogError("MeshRenderer for the activated object is not found.");
        }
    }

    private void SetColliderEnabled(bool isEnabled)
    {
        if (_floatingAreaCollider)
        {
            _floatingAreaCollider.enabled = isEnabled;
        }
        else
        {
            Debug.LogError("Collider for the floating area is not found.");
        }
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _startPosition = transform.position;

        if (activatedObject)
        {
            _activatedObjectMeshRenderer = activatedObject.GetComponent<MeshRenderer>();
            _floatingAreaCollider = activatedObject.GetComponent<Collider>();

            _originalMaterial = _activatedObjectMeshRenderer.material;

            if (transparentMaterial == null)
            {
                transparentMaterial = new Material(Shader.Find("Standard"));
                transparentMaterial.color = new Color(1, 1, 1, 0.5f);
            }
        }
        else
        {
            Debug.LogError("Activated object is not set for DiskTangibleController script. Please set it in the Unity Editor.");
        }
    }

    void FixedUpdate()
    {
        if (_isInsideTrigger)
        {
            transform.position = Vector3.Lerp(transform.position, hoverPoint.position, Time.deltaTime);
        
            // Make the disk's X direction face the Hover Point's X direction by aligning the disk's Z-axis with the Hover Point's Y-axis.
            Quaternion targetRotation = Quaternion.LookRotation(hoverPoint.up, hoverPoint.right);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DisktangibleFloatingArea"))
        {
            _isInsideTrigger = true;
            _rigidbody.useGravity = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            // Search for a Hovering Point within the collided object.
            Transform foundHoverPoint = other.transform.Find("Hovering Point");
            if (foundHoverPoint)
            {
                hoverPoint = foundHoverPoint;
            }
            else
            {
                Debug.LogWarning("No Hovering Point found in the collided object. Using the default hover point.");
            }

            SetMaterial(transparentMaterial);
            SetColliderEnabled(false);

            if (glowObject)
            {
                glowObject.GetComponent<MeshRenderer>().material = glowOn;
            }
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("DisktangibleFloatingArea"))
        {
            _isInsideTrigger = false;
            _rigidbody.useGravity = true;

            SetMaterial(_originalMaterial);
            SetColliderEnabled(true);

            if (glowObject)
            {
                glowObject.GetComponent<MeshRenderer>().material = glowOff;
            }
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        _isInsideTrigger = false;
        _rigidbody.useGravity = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        _rigidbody.useGravity = true;
    }
}
