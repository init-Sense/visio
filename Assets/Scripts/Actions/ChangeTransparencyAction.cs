using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ChangeTransparencyAction", menuName = "Actions/Change Transparency")]
public class ChangeTransparencyAction : ActionBase
{
    private bool hasExecuted = false; // Flag to check if the action has been executed
    private float lastTransparencyIncrement; // Store the last transparency increment applied

    public override void ExecuteAction(GameObject targetObject, Ray incomingRay, float transparencyIncrement = 0f)
    {
        if (hasExecuted) return; // If the action has already been executed, return

        Debug.Log($"ExecuteAction called for {targetObject.name}");

        Renderer renderer = targetObject.GetComponent<Renderer>();
        Color baseMapColor = renderer.material.GetColor("_BaseColor"); // Get the Base Map color

        Debug.Log($"Initial Transparency of {targetObject.name}: {baseMapColor.a}");
        Debug.Log($"Transparency Increment: {transparencyIncrement}");

        baseMapColor.a += transparencyIncrement; 
        baseMapColor.a = Mathf.Clamp(baseMapColor.a, 0f, 1f); 

        renderer.material.SetColor("_BaseColor", baseMapColor); // Set the modified Base Map color

        Debug.Log($"Updated Transparency of {targetObject.name}: {baseMapColor.a}");

        // Disable the collider if the object is still transparent
        if (baseMapColor.a < 0.99f)
        {
            targetObject.GetComponent<Collider>().enabled = false;
        }
        else
        {
            targetObject.GetComponent<Collider>().enabled = true;
        }

        lastTransparencyIncrement = transparencyIncrement; // Store the last transparency increment applied
        hasExecuted = true; // Set the flag to true
    }

    public override void RevertAction(GameObject targetObject)
    {
        Renderer renderer = targetObject.GetComponent<Renderer>();
        Color baseMapColor = renderer.material.GetColor("_BaseColor");
        baseMapColor.a -= lastTransparencyIncrement; // Subtract the last transparency increment
        baseMapColor.a = Mathf.Clamp(baseMapColor.a, 0f, 1f); // Ensure alpha is between 0 and 1
        renderer.material.SetColor("_BaseColor", baseMapColor); // Set the modified Base Map color
        hasExecuted = false; // Reset the flag
    }
}
