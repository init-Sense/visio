using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class is used to control the disk object.
/// The disk object is used to render tangible objects intangible.
/// </summary>
public class DiskTangibleController : XRGrabInteractable
{
    private bool _isInsideTrigger = false;

    // Disk object reference.
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _rigidbody;

    [Tooltip("This is where the disk will actually float to.")] [SerializeField]
    public Transform hoverPoint;

    [Tooltip("This is the wall object that will be disabled when the disk enters its associated trigger.")]
    [SerializeField]
    public GameObject wallObject;

    [Tooltip("Assign a material for the wall object when the disk enters its associated trigger.")] [SerializeField]
    public Material transparentMaterial;

    // Wall object reference.
    private Material _originalMaterial;
    private MeshRenderer _wallMeshRenderer;
    private Collider _wallCollider;

    private void SetMaterial(Material material)
    {
        if (_wallMeshRenderer)
        {
            _wallMeshRenderer.material = material;
        }
        else
        {
            Debug.LogError("MeshRenderer for the wall object is not found.");
        }
    }

    private void SetColliderEnabled(bool isEnabled)
    {
        if (_wallCollider)
        {
            _wallCollider.enabled = isEnabled;
        }
        else
        {
            Debug.LogError("Collider for the wall object is not found.");
        }
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _startPosition = transform.position;

        if (wallObject)
        {
            _wallMeshRenderer = wallObject.GetComponent<MeshRenderer>();
            _wallCollider = wallObject.GetComponent<Collider>();

            _originalMaterial = _wallMeshRenderer.material;

            if (transparentMaterial == null)
            {
                transparentMaterial = new Material(Shader.Find("Standard"));
                transparentMaterial.color = new Color(1, 1, 1, 0.5f);
            }
        }
        else
        {
            Debug.LogError(
                "Wall object is not set for DiskTangibleController script. Please set it in the Unity Editor.");
        }
    }

    void FixedUpdate()
    {
        if (_isInsideTrigger)
        {
            transform.position = Vector3.Lerp(transform.position, hoverPoint.position, Time.deltaTime);
            transform.Rotate(Vector3.up, 50.0f * Time.deltaTime);
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

            SetMaterial(transparentMaterial);
            SetColliderEnabled(false);
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