using UnityEngine;
using System.Collections.Generic;

public class RayProcessingController : MonoBehaviour
{
    [System.Serializable]
    public class ActionableObject
    {
        public GameObject TargetObject;
        public Action ActionToPerform;
    }

    public enum Action { SetMaterial, RemoveGameObject }

    public List<ActionableObject> actionObjects = new List<ActionableObject>();
    public GameObject rayReceiver; // The GameObject that can receive rays.

    public void ProcessRayHit(GameObject hitObject)
    {
        // Check if the hit object is the ray receiver.
        if (hitObject == rayReceiver)
        {
            foreach (ActionableObject actionObject in actionObjects)
            {
                PerformAction(actionObject);
            }
        }
    }

    private void PerformAction(ActionableObject actionObject)
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
        }
    }
}