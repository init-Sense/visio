using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotatableGrabbable : XRGrabInteractable
{
    public float rotationSpeedThreshold = 100f;
    public float upwardSpeed = 0.1f;
    public float elevatorStartDelay = 3f;
    public float diskMoveTime = 5f;
    public float elevatorMoveTime = 10f;
    public Transform elevator;
    public Transform target;

    private Vector3 initialPosition;
    private Quaternion lastRotation;
    private float currentAngularVelocity;
    private bool hasBeenGrabbed = false;

    void Start()
    {
        initialPosition = transform.position;
        movementType = MovementType.Kinematic;
        upwardSpeed = Vector3.Distance(elevator.position, target.position) / elevatorMoveTime;
    }

    void Update()
    {
        if (hasBeenGrabbed)
        {
            float angleDifference = Quaternion.Angle(lastRotation, transform.rotation);
            currentAngularVelocity = angleDifference / Time.deltaTime;

            if (currentAngularVelocity >= rotationSpeedThreshold)
            {
                hasBeenGrabbed = false;
                StartCoroutine(MoveDisk());
                StartCoroutine(MoveElevator());
            }

            lastRotation = transform.rotation;
        }

        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    IEnumerator MoveDisk()
    {
        Vector3 startPosition = transform.position;
        float startTime = Time.time;

        while (Time.time < startTime + diskMoveTime)
        {
            float t = (Time.time - startTime) / diskMoveTime;
            transform.position = Vector3.Lerp(startPosition, startPosition + Vector3.up * upwardSpeed * diskMoveTime, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    IEnumerator MoveElevator()
    {
        yield return new WaitForSeconds(elevatorStartDelay);

        Vector3 startPosition = elevator.position;
        float startTime = Time.time;

        while (Time.time < startTime + elevatorMoveTime)
        {
            float t = (Time.time - startTime) / elevatorMoveTime;
            elevator.position = Vector3.Lerp(startPosition, target.position, t);
            yield return null;
        }
    }

    protected override void OnSelectEntered(XRBaseInteractor interactor)
    {
        base.OnSelectEntered(interactor);
        hasBeenGrabbed = true;
        lastRotation = transform.rotation;
    }

    protected override void OnSelectExited(XRBaseInteractor interactor)
    {
        base.OnSelectExited(interactor);
    }
}
