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

    private enum ActivatorState
    {
        NotInsideTrigger,
        InsideTrigger
    }

    private ActivatorState currentState = ActivatorState.NotInsideTrigger;

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
    private Transform _targetTransform;

    private bool _isRayActive = false;
    public AreaController areaController;

    private bool isSelectExitedCalled = false;

    private Material _targetOriginalMaterial;
    private MeshRenderer _targetRenderer;

    private RayActivationController currentRayActivationController;

    private Renderer _energyBallRenderer;

    private Coroutine deactivateCoroutine;
    private float rayDeactivationDelay = 1f; // 1 second delay, adjust as needed
    private Transform _energyRingTurretTransform;
    private Material _energyRingOriginalMaterial;
    private Vector3 _energyRingOriginalPosition;
    private Coroutine _energyRingAnimationCoroutine;
    [Tooltip("The amplitude of the up and down motion for the EnergyRing.")]
    public float energyRingFloatAmplitude = 0.5f; // default value
    [Tooltip("The speed of the up and down motion for the EnergyRing.")]
    public float energyRingFloatSpeed = 1.0f; // default value
    [Tooltip("The rotation speed for the EnergyRing.")]
    public float energyRingRotationSpeed = 60f; // default value in degrees per second


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

        _energyRingTurretTransform = transform.parent.Find("EnergyRing_Turret");

        GameObject dynamicTurret = GameObject.Find("Dynamic Turret");
        if (dynamicTurret != null)
        {
            _energyRingTurretTransform = dynamicTurret.transform.Find("EnergyRing_Turret");
            if (_energyRingTurretTransform == null)
            {
                Debug.LogError("Ring not found inside Dynamic Turret!");
            }
            else
            {
                _energyRingOriginalPosition = _energyRingTurretTransform.position;
                _energyRingOriginalMaterial = _energyRingTurretTransform.GetComponent<Renderer>().material;

            }
        }
        else
        {
            Debug.LogError("Dynamic Turret not found!");
        }

    }

    void FixedUpdate()
    {
        if (currentState == ActivatorState.InsideTrigger)
        {
            HandleInsideTriggerState();
        }
    }

    void HandleInsideTriggerState()
    {
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


        // Float and rotate the activator towards the target transform.
        transform.position = Vector3.MoveTowards(transform.position, _targetTransform.position, Time.deltaTime * centeringSpeed);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (_rigidbody.useGravity)
        {
            Debug.Log("Gravity is enabled");
        }
        else
        {
            Debug.Log("Gravity is disabled");
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ActivatorFloatingArea"))
        {
            // Search for the RayActivationController in the parent object
            currentRayActivationController = other.transform.parent.GetComponent<RayActivationController>();
            currentState = ActivatorState.InsideTrigger;

            if (currentRayActivationController != null)
            {
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

            if (_energyRingTurretTransform)
            {
                Renderer energyRingRenderer = _energyRingTurretTransform.GetComponent<Renderer>();
                if (energyRingRenderer)
                {
                    energyRingRenderer.material = activatorGlowMaterial;
                }
                if (_energyRingAnimationCoroutine != null)
                {
                    StopCoroutine(_energyRingAnimationCoroutine);
                }
                _energyRingAnimationCoroutine = StartCoroutine(EnergyRingAnimation());
            }
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ActivatorFloatingArea"))
        {
            Debug.Log("Exiting ActivatorFloatingArea, enabling gravity");

            currentState = ActivatorState.NotInsideTrigger;

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

            if (_energyRingTurretTransform)
            {
                Renderer energyRingRenderer = _energyRingTurretTransform.GetComponent<Renderer>();
                if (energyRingRenderer)
                {
                    energyRingRenderer.material = _energyRingOriginalMaterial;
                }
                if (_energyRingAnimationCoroutine != null)
                {
                    StopCoroutine(_energyRingAnimationCoroutine);
                }
                StartCoroutine(ReturnEnergyRingToOriginalPosition());

            }

        }
    }



    private IEnumerator DeactivateRayAfterDelay()
    {
        yield return new WaitForSeconds(rayDeactivationDelay);

        if (currentState == ActivatorState.NotInsideTrigger && _isRayActive)
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

    private IEnumerator EnergyRingAnimation()
    {
        float elapsedTime = 0;
        while (true)
        {
            // For up and down motion
            float yOffset = energyRingFloatAmplitude * Mathf.Sin(energyRingFloatSpeed * elapsedTime);
            Vector3 newPosition = _energyRingOriginalPosition + new Vector3(0, yOffset, 0);
            _energyRingTurretTransform.position = newPosition;

            // For rotation
            _energyRingTurretTransform.Rotate(0, energyRingRotationSpeed * Time.deltaTime, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ReturnEnergyRingToOriginalPosition()
    {
        float elapsedTime = 0;
        float returnDuration = 2.0f;  // Set the time you'd like for the return in seconds. Adjust as needed.
        Vector3 startingPosition = _energyRingTurretTransform.position;

        while (elapsedTime < returnDuration)
        {
            _energyRingTurretTransform.position = Vector3.Lerp(startingPosition, _energyRingOriginalPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _energyRingTurretTransform.position = _energyRingOriginalPosition;
    }

}