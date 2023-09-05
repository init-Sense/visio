using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class FloatingAreaRayControllerPair
{
    public Collider floatingArea;
    public RayActivationController rayActivationController;
    public GameObject targetObject;
    public Material targetGlowMaterial;
}

public class ActivatorController : XRGrabInteractable
{
    [Tooltip("Assign the floating areas and their associated RayActivationController objects here.")] [SerializeField]
    private List<FloatingAreaRayControllerPair> floatingAreaRayControllerPairs;

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

    private Material _targetOriginalMaterial;
    private MeshRenderer _targetRenderer;

    private RayActivationController currentRayActivationController;

    private Renderer _energyBallRenderer;

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

        Transform energyBallTransform = transform.Find("EnergyBall");
        if (energyBallTransform != null)
        {
            _energyBallRenderer = energyBallTransform.GetComponent<Renderer>();
            if (_energyBallRenderer == null)
            {
                Debug.LogError("No Renderer found on the EnergyBall object.");
            }
        }
        else
        {
            Debug.LogError("No child transform named 'EnergyBall' found under Activator.");
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
            if (!_isRayActive && currentRayActivationController != null)
            {
                currentRayActivationController.ActivateRaycasting();
                _isRayActive = true;
            }
        }
        else if (_isRayActive && currentRayActivationController != null) // Deactivate ray if previously active
        {
            currentRayActivationController.DeactivateRaycasting();
            _isRayActive = false;
        }

        if (_rigidbody.useGravity)
        {
            Debug.Log("Gravity is enabled");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Find the correct RayActivationController based on the floating area
        foreach (var pair in floatingAreaRayControllerPairs)
        {
            if (pair.floatingArea == other)
            {
                currentRayActivationController = pair.rayActivationController;
                _targetRenderer = pair.targetObject.GetComponent<MeshRenderer>();
                _targetOriginalMaterial = _targetRenderer.material;
                _targetRenderer.material = pair.targetGlowMaterial;
                break;
            }
        }

        if (currentRayActivationController != null)
        {
            _isInsideTrigger = true;
            _rigidbody.useGravity = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            if (_energyBallRenderer != null)
            {
                _energyBallRenderer.material = activatorGlowMaterial;
            }

            _targetTransform = other.transform.Find("Hovering Point");
            if (_targetTransform == null)
            {
                Debug.LogError("No child transform named 'Hovering Point' found under ActivatorFloatingArea.");
            }

            currentRayActivationController.ActivateRaycasting();
            _isRayActive = true;
        }
        else
        {
            Debug.LogError("No RayActivationController found for the floating area.");
        }
    }

    private float rayDeactivationDelay = 1f; // 1 second delay, adjust as needed

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ActivatorFloatingArea") && _isRayActive)
        {
            Debug.Log("Exiting ActivatorFloatingArea, enabling gravity");
            _isInsideTrigger = false;
            _rigidbody.useGravity = true;
            if (_energyBallRenderer != null)
            {
                _energyBallRenderer.material = _activatorOriginalMaterial;
            }

            _targetRenderer.material = _targetOriginalMaterial;
            _targetTransform = null;

            StartCoroutine(DeactivateRayAfterDelay());
        }
    }

    private IEnumerator DeactivateRayAfterDelay()
    {
        yield return new WaitForSeconds(rayDeactivationDelay);
        if (!_isInsideTrigger && _isRayActive) // Double-check to ensure the activator is still outside the trigger
        {
            currentRayActivationController.DeactivateRaycasting();
            currentRayActivationController.rayProcessingController.ResetRayHit(); // Explicitly reset ray hit
            _isRayActive = false;
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
        if (currentRayActivationController != null)
        {
            currentRayActivationController.DeactivateRaycasting();
            _isRayActive = false;
        }
    }
}