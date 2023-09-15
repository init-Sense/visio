using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Tooltip("Assign a clip to play while the ray is active.")]
    public AudioClip rayActiveLoopClip;

    private AudioSource _audioSource;
    
    private bool isAudioPlaying = false;

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

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.spatialBlend = 1.0f; // Make the sound effects source positional
        _audioSource.transform.position = transform.position; // Set the position to the center of the activator

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

        if (_isRayActive && rayActiveLoopClip != null && !isAudioPlaying)
        {
            _audioSource.clip = rayActiveLoopClip;
            _audioSource.Play();
            isAudioPlaying = true;
        }
        else if (!_isRayActive && isAudioPlaying) // If the ray is not active, stop playing the clip
        {
            _audioSource.Stop();
            isAudioPlaying = false;
        }

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
        Debug.Log("OnTriggerEnter called with collider: " + other.name);

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
            
            Debug.Log("RayActivationController found: " + currentRayActivationController.name);
            
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

    Coroutine deactivateCoroutine;


    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit called with collider: " + other.name);
        
        // Ensure it's the correct trigger that is exited
        foreach (var pair in floatingAreaRayControllerPairs)
        {
            if (pair.floatingArea == other)
            {
                Debug.Log("Exiting ActivatorFloatingArea, enabling gravity");
                _isInsideTrigger = false;
                _rigidbody.useGravity = true;
                if (_energyBallRenderer != null)
                {
                    _energyBallRenderer.material = _activatorOriginalMaterial;
                }

                if (deactivateCoroutine != null)
                {
                    StopCoroutine(deactivateCoroutine);
                }

                deactivateCoroutine = StartCoroutine(DeactivateRayAfterDelay());
                break;
            }
        }
    }


    private IEnumerator DeactivateRayAfterDelay()
    {
        yield return new WaitForSeconds(rayDeactivationDelay);

        if (!_isInsideTrigger && _isRayActive)
        {
            if (currentRayActivationController != null)
            {
                currentRayActivationController.DeactivateRaycasting();
                currentRayActivationController.rayProcessingController.ResetRayHit(); // Explicitly reset ray hit
            }
            else
            {
                Debug.LogError("currentRayActivationController is null.");
            }
            _isRayActive = false;
        }

        if (_targetRenderer != null)
        {
            _targetRenderer.material = _targetOriginalMaterial; // Reset to the original material
            _targetRenderer = null;
            _targetOriginalMaterial = null;
        }

        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
            isAudioPlaying = false;
        }
    }



    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Debug.Log("OnSelectEntered called, disabling gravity");
        _rigidbody.useGravity = false;
        _targetTransform = args.interactable.transform;

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

        isSelectExitedCalled = true;
    }
}