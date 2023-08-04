using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateOnlyInteractable : XRBaseInteractable
{
    public enum RotationAxis
    {
        NoRotation,
        Horizontal,
        Vertical,
        Full360
    }

    public RotationAxis rotationAxis;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Quaternion initialInteractorRotation;

    protected override void Awake()
    {
        base.Awake();
        initialPosition = transform.position;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialInteractorRotation = args.interactor.transform.rotation;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        transform.position = initialPosition;
    }

    protected virtual void Update()
    {
        if (isSelected)
        {
            transform.position = initialPosition;

            Quaternion relativeRotation = Quaternion.Inverse(initialInteractorRotation) * selectingInteractor.transform.rotation;

            switch (rotationAxis)
            {
                case RotationAxis.NoRotation:
                    relativeRotation = Quaternion.Euler(0, 0, 0);
                    break;
                    
                case RotationAxis.Horizontal:
                    relativeRotation = Quaternion.Euler(0, relativeRotation.eulerAngles.y, 0);
                    break;

                case RotationAxis.Vertical:
                    relativeRotation = Quaternion.Euler(relativeRotation.eulerAngles.x, 0, 0);
                    break;

                case RotationAxis.Full360:
                    relativeRotation = Quaternion.Euler(relativeRotation.eulerAngles.x, relativeRotation.eulerAngles.y, relativeRotation.eulerAngles.z);
                    break;
            }

            transform.rotation = initialRotation * relativeRotation;
        }
    }
}
