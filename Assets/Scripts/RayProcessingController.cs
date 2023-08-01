using UnityEngine;
using System.Collections.Generic;

public class RayProcessingController : MonoBehaviour
{
    [System.Serializable]
    public class ActionableObject
    {
        public GameObject TargetObject;
        public Action ActionToPerform;
        public bool CanReflectRay;  // New field
    }

    public enum Action { SetMaterial, RemoveGameObject }

    public List<ActionableObject> actionObjects = new List<ActionableObject>();
    public GameObject rayReceiver; // The GameObject that can receive rays.
    public bool rayReceiverReflectsRays; // New field indicating if the rayReceiver should reflect rays.

    public float reflectedRayLength = 1000f;
    private LineRenderer reflectedRayRenderer;  // LineRenderer to visualize the reflected ray.

    void Start()
    {
        // Initialize LineRenderer
        reflectedRayRenderer = gameObject.AddComponent<LineRenderer>();
        reflectedRayRenderer.material = new Material(Shader.Find("Standard"));
        reflectedRayRenderer.startColor = Color.blue;
        reflectedRayRenderer.endColor = Color.blue;
        reflectedRayRenderer.startWidth = 0.01f;
        reflectedRayRenderer.endWidth = 0.01f;
        reflectedRayRenderer.positionCount = 2;
        reflectedRayRenderer.enabled = false;
    }

    public void ProcessRayHit(Vector3 hitPoint, Ray incomingRay, Vector3 hitNormal)  // takes a hitPoint parameter
    {
        foreach (ActionableObject actionObject in actionObjects)
        {
            PerformAction(actionObject, incomingRay);
        }

        // If rayReceiver should reflect rays, reflect the incoming ray.
        if (rayReceiverReflectsRays)
        {
            Ray reflectedRay = new Ray(hitPoint, Vector3.Reflect(incomingRay.direction, hitNormal));  // use the hit point
            
            RaycastHit reflectedHit;
            if (Physics.Raycast(reflectedRay, out reflectedHit, reflectedRayLength))
            {
                // If reflected ray hits something
                reflectedRayRenderer.SetPosition(0, reflectedRay.origin);
                reflectedRayRenderer.SetPosition(1, reflectedHit.point);
                
                Debug.Log("Reflected ray hit " + reflectedHit.transform.gameObject.name);
            }
            else
            {
                // If reflected ray doesn't hit anything, just draw in direction of reflection
                reflectedRayRenderer.SetPosition(0, reflectedRay.origin);
                reflectedRayRenderer.SetPosition(1, reflectedRay.origin + reflectedRay.direction * reflectedRayLength);
                
                Debug.Log("Reflected ray didn't hit anything");
            }
            reflectedRayRenderer.enabled = true;
        }
        else
        {
            reflectedRayRenderer.enabled = false;
            
            Debug.Log("Ray receiver doesn't reflect rays");
        }
    }
    
    private void PerformAction(ActionableObject actionObject, Ray incomingRay)  // Updated
    {
        switch (actionObject.ActionToPerform)
        {
            case Action.SetMaterial:
                actionObject.TargetObject.GetComponent<Renderer>().material.color = Color.red;
                break;

            case Action.RemoveGameObject:
                Destroy(actionObject.TargetObject);
                break;
        }

        // If the object can reflect the ray, do it here
        if (actionObject.CanReflectRay)
        {
            // Reflect the ray. 
            // Note that this assumes the rayReceiver is a flat surface facing upwards.
            Ray reflectedRay = new Ray(actionObject.TargetObject.transform.position, Vector3.Reflect(incomingRay.direction, Vector3.up));
            
            // Here you can use the reflectedRay for other processing
        }
    }

    public void ResetRayHit(GameObject hitObject)
    {
        // Check if the hit object is the ray receiver.
        if (hitObject == rayReceiver)
        {
            foreach (ActionableObject actionObject in actionObjects)
            {
                if (actionObject.ActionToPerform == Action.SetMaterial)
                {
                    actionObject.TargetObject.GetComponent<Renderer>().material.color = Color.white;
                }
            }
        
            // Check if LineRenderer is not null before trying to access it.
            if (reflectedRayRenderer != null)
            {
                reflectedRayRenderer.enabled = false; // disable the reflected ray LineRenderer
            }
        }
    }


}