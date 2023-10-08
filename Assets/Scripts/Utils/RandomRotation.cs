using UnityEngine;

/// <summary>
/// A simple rotation script, mainly for the energy ball or objects in the landscape that need to rotate.
/// </summary>
public class RandomRotation : MonoBehaviour
{
    public float rotationSpeed = 20f;
    public Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}