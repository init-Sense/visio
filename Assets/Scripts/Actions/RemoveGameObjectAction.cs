using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RemoveGameObjectAction", menuName = "Actions/Remove GameObject")]
public class RemoveGameObjectAction : ActionBase
{
    public override void ExecuteAction(GameObject targetObject, Ray incomingRay, float transparencyIncrement = 0f)
    {
        targetObject.SetActive(false);
    }

    public override void RevertAction(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }
}