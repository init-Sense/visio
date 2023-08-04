using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiskTangibleController : XRGrabInteractable
{
    private bool _isInsideTrigger = false;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _rigidbody;
    public Transform boxCenter;
    public GameObject wallObject;
    public Material transparentMaterial;
    private Material _originalMaterial;
    private MeshRenderer _wallMeshRenderer;
    private Collider _wallCollider;

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
            // Float and rotate the disk.
            transform.position = Vector3.Lerp(transform.position, boxCenter.position, Time.deltaTime);
            transform.Rotate(Vector3.up, 50.0f * Time.deltaTime);
        }
        else
        {
            // Return disk to initial position if it is not inside the trigger.
            transform.position = Vector3.Lerp(transform.position, _startPosition, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _startRotation, Time.deltaTime);
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

            // Change the wall material to the transparent one and disable collider
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

            // Change the wall material back to the original one and enable collider
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
}