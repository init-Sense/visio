using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;
    public Material materialAfterInsertion;

    private Transform targetTransform;
    private GameObject insertedObject = null;
    private bool isMoving = false;

    void Update()
    {
        if (isMoving && targetTransform != null)
        {
            // Move towards the target position
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, step);

            // Check if the elevator has reached the target position
            if (Vector3.Distance(transform.position, targetTransform.position) < 0.001f)
            {
                // Stop moving
                StopMoving();
            }
        }
    }

    public void MoveTo(Transform target, GameObject insertedObject)
    {
        targetTransform = target;
        this.insertedObject = insertedObject; // Assign the inserted object
        isMoving = true;

        // Set player as a child of the elevator
        player.SetParent(transform);
    }

    public void StopMoving()
    {
        isMoving = false;

        // Remove player from being a child of the elevator
        player.SetParent(null);

        // Change the parent's material
        if (insertedObject != null && insertedObject.transform.parent != null)
        {
            Renderer parentRenderer = insertedObject.transform.parent.GetComponent<Renderer>();
            if (parentRenderer != null)
            {
                parentRenderer.material = materialAfterInsertion;
            }
        }

        // Destroy the inserted object
        if (insertedObject != null)
        {
            Destroy(insertedObject);
        }
    }
}
