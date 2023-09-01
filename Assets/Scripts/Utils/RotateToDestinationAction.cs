using UnityEngine;

public class RotateToDestinationAction : MonoBehaviour
{
    [System.Serializable]
    public class RotationObject
    {
        public GameObject targetObject;
        public float rotationAmount = 90f;
    }

    public RotationObject[] objectsToRotate;
    public float duration = 1.0f;

    private Quaternion[] initialRotations;
    private Quaternion[] targetRotations;
    private float elapsedTime = 0;

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
        targetRotations = new Quaternion[objectsToRotate.Length];

        for (int i = 0; i < objectsToRotate.Length; i++)
        {
            initialRotations[i] = objectsToRotate[i].targetObject.transform.rotation;
            targetRotations[i] = Quaternion.Euler(objectsToRotate[i].targetObject.transform.eulerAngles + new Vector3(0, objectsToRotate[i].rotationAmount, 0));
        }

        elapsedTime = 0;

        StartCoroutine(Rotate());
    }

    public void RevertAction()
    {
        for (int i = 0; i < objectsToRotate.Length; i++)
        {
            objectsToRotate[i].targetObject.transform.rotation = initialRotations[i];
        }
    }

    private System.Collections.IEnumerator Rotate()
    {
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation

            for (int i = 0; i < objectsToRotate.Length; i++)
            {
                objectsToRotate[i].targetObject.transform.rotation = Quaternion.Lerp(initialRotations[i], targetRotations[i], t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < objectsToRotate.Length; i++)
        {
            objectsToRotate[i].targetObject.transform.rotation = targetRotations[i];
        }

        gameObject.SetActive(false);
    }

}
