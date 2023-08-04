using System.Collections;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public GameManager gameManager;
    public Transform elevatorTarget;
    public GameObject correctObject;
    public GameObject objectToShrink;
    public Material newMaterial;
    public Vector3 targetScale = Vector3.one * 0.5f;
    public float rotationSpeed = 20f;
    public float shrinkSpeed = 0.1f;

    private GameObject insertedObject = null;
    private Rigidbody insertedObjectRb = null;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the correct one
        if (other.gameObject == correctObject)
        {
            // Store the object
            insertedObject = other.gameObject;

            // Get the object's Rigidbody
            insertedObjectRb = insertedObject.GetComponent<Rigidbody>();
            
            // Stop gravity and freeze rotation on the inserted object
            if (insertedObjectRb != null)
            {
                insertedObjectRb.useGravity = false;
                insertedObjectRb.freezeRotation = true;
            }

            // Call the OnObjectInserted method of the game manager
            gameManager.OnObjectInserted(other.gameObject, elevatorTarget);
        }
    }

    void Update()
    {
        // If an object has been inserted
        if (insertedObject != null)
        {
            // Move the object to the center of the trigger
            insertedObject.transform.position = Vector3.Lerp(insertedObject.transform.position, transform.position, 0.05f);

            // Slowly rotate the object
            insertedObject.transform.Rotate(Vector3.up, 20 * Time.deltaTime);
        }
    }

    // If the object leaves the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == insertedObject)
        {
            // Reset stored object
            insertedObject = null;

            // Enable gravity and unfreeze rotation on the inserted object
            if (insertedObjectRb != null)
            {
                insertedObjectRb.useGravity = true;
                insertedObjectRb.freezeRotation = false;
                insertedObjectRb = null;
            }

            // Start the RotateAndShrink coroutine
            if (objectToShrink != null)
            {
                StartCoroutine(RotateAndShrink(objectToShrink.transform));
            }
        }
    }

    // Coroutine to rotate and shrink the parent object
    private IEnumerator RotateAndShrink(Transform targetObject)
    {
        // Change the material of the object
        Renderer objectRenderer = targetObject.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.material = newMaterial;
        }

        // Rotate and shrink the object
        while (targetObject.localScale.sqrMagnitude > targetScale.sqrMagnitude)
        {
            targetObject.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            targetObject.localScale = Vector3.Lerp(targetObject.localScale, targetScale, shrinkSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
