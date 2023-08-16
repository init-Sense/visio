using UnityEngine;

public class PlatformMotionController : MonoBehaviour
{
    public Transform startPoint; // Starting point of the platform
    public Transform endPoint; // Ending point of the platform
    public float speed = 1.0f; // Speed of the platform's motion
    public AnimationCurve motionCurve; // Curve to control the motion

    private bool isMoving = false; // Flag to check if the platform is moving
    private float journeyLength; // Total distance between the start and end points
    private float startTime; // Time when the platform starts moving

    private void Start()
    {
        transform.position = startPoint.position; // Initialize the platform's position
        journeyLength = Vector3.Distance(startPoint.position, endPoint.position);
    }

    private void Update()
    {
        if (isMoving)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            float curvedFraction = motionCurve.Evaluate(fractionOfJourney); // Get the value from the curve

            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, curvedFraction);

            if (fractionOfJourney >= 1.0f)
            {
                isMoving = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isMoving)
        {
            startTime = Time.time;
            isMoving = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isMoving)
        {
            // Make the platform vanish or become non-interactable
            gameObject.SetActive(false);
        }
    }
}