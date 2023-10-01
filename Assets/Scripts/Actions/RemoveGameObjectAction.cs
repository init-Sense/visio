using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RemoveGameObjectAction", menuName = "Actions/Remove GameObject")]
public class RemoveGameObjectAction : ActionBase
{
    public override void ExecuteAction(GameObject targetObject, Ray incomingRay)
    {
        Debug.Log("Executing: Removing GameObject " + targetObject.name);
        targetObject.SetActive(false);
    }

    public override void RevertAction(GameObject targetObject)
    {
        Debug.Log("Reverting: Setting GameObject active " + targetObject.name);
        targetObject.SetActive(true);
    }

}