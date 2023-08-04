using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    public float rotationSpeed = 20f;
    public Vector3 rotationAxis = Vector3.up;

    // Update is called once per frame
    private void Update()
    {
        // Rotate the object around the given axis by rotationSpeed degrees per second
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}