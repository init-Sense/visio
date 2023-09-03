using UnityEngine;

public class RotateToDestinationAction : MonoBehaviour
{
    [System.Serializable]
    public class RotationObject
    {
        public GameObject targetObject;
        public float rotationAmount = 90f;
        public float duration = 1.0f;
        public Vector3 targetPosition;
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
                    objectsToRotate[i].targetObject.transform.position = Vector3.Lerp(initialPositions[i], objectsToRotate[i].targetPosition, t);

                    elapsedTimes[i] += Time.deltaTime;
                }
                else
                {
                    objectsToRotate[i].targetObject.transform.rotation = targetRotations[i];
                    objectsToRotate[i].targetObject.transform.position = objectsToRotate[i].targetPosition;
                }
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
