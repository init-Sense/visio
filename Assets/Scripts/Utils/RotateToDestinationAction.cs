using UnityEngine;

public class RotateToDestinationAction : MonoBehaviour
{
    [System.Serializable]
    public class RotationObject
    {
        public GameObject targetObject;
        public float rotationAmount = 90f;
        public float duration = 1.0f;
        public Transform targetPosition;

        // public AudioClip startMoveClip;
        // public AudioClip movingClip;
        // public AudioClip arriveMoveClip;
        //
        // public AudioClip startRotateClip;
        // public AudioClip rotatingClip;
        // public AudioClip stopRotateClip;
    }

    public RotationObject[] objectsToRotate;

    private Quaternion[] initialRotations;
    private Vector3[] initialPositions;
    private Quaternion[] targetRotations;
    private float[] elapsedTimes;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered the collider of: " + gameObject.name);
            ExecuteAction();
        }
    }

    public void ExecuteAction()
    {
        initialRotations = new Quaternion[objectsToRotate.Length];
        initialPositions = new Vector3[objectsToRotate.Length];
        targetRotations = new Quaternion[objectsToRotate.Length];
        elapsedTimes = new float[objectsToRotate.Length];

        for (int i = 0; i < objectsToRotate.Length; i++)
        {
            initialRotations[i] = objectsToRotate[i].targetObject.transform.rotation;
            initialPositions[i] = objectsToRotate[i].targetObject.transform.position;
            targetRotations[i] = Quaternion.Euler(objectsToRotate[i].targetObject.transform.eulerAngles + new Vector3(0, objectsToRotate[i].rotationAmount, 0));
            elapsedTimes[i] = 0;
        }

        StartCoroutine(Rotate());
    }

    public void RevertAction()
    {
        for (int i = 0; i < objectsToRotate.Length; i++)
        {
            objectsToRotate[i].targetObject.transform.rotation = initialRotations[i];
            objectsToRotate[i].targetObject.transform.position = initialPositions[i];
        }
    }

private System.Collections.IEnumerator Rotate()
{
    bool allRotationsCompleted = false;

    while (!allRotationsCompleted)
    {
        allRotationsCompleted = true;

        for (int i = 0; i < objectsToRotate.Length; i++)
        {
            if (elapsedTimes[i] < objectsToRotate[i].duration)
            {
                allRotationsCompleted = false;

                float t = elapsedTimes[i] / objectsToRotate[i].duration;
                t = t * t * (3f - 2f * t); // Smooth step interpolation
                
                objectsToRotate[i].targetObject.transform.rotation = Quaternion.Lerp(initialRotations[i], targetRotations[i], t);
                
                // Check if targetPosition is not null before using it
                if (objectsToRotate[i].targetPosition != null)
                {
                    objectsToRotate[i].targetObject.transform.position = Vector3.Lerp(initialPositions[i], objectsToRotate[i].targetPosition.position, t);
                }

                // Play the moving and rotating clips
                // if (elapsedTimes[i] == 0)
                // {
                //     AudioSource.PlayClipAtPoint(objectsToRotate[i].startMoveClip, objectsToRotate[i].targetObject.transform.position);
                //     AudioSource.PlayClipAtPoint(objectsToRotate[i].startRotateClip, objectsToRotate[i].targetObject.transform.position);
                // }
                // else
                // {
                //     AudioSource.PlayClipAtPoint(objectsToRotate[i].movingClip, objectsToRotate[i].targetObject.transform.position);
                //     AudioSource.PlayClipAtPoint(objectsToRotate[i].rotatingClip, objectsToRotate[i].targetObject.transform.position, 1.0f); // Increase volume for far away pivot point
                // }

                elapsedTimes[i] += Time.deltaTime;
            }
            else
            {
                objectsToRotate[i].targetObject.transform.rotation = targetRotations[i];
                
                // Check if targetPosition is not null before using it
                if (objectsToRotate[i].targetPosition != null)
                {
                    objectsToRotate[i].targetObject.transform.position = objectsToRotate[i].targetPosition.position;
                }

                // // Play the arrive and stop rotating clips
                // AudioSource.PlayClipAtPoint(objectsToRotate[i].arriveMoveClip, objectsToRotate[i].targetObject.transform.position);
                // AudioSource.PlayClipAtPoint(objectsToRotate[i].stopRotateClip, objectsToRotate[i].targetObject.transform.position);
            }
        }

        yield return null;
    }

    gameObject.SetActive(false);
}

}
