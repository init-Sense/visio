using System;
using UnityEngine;

/// <summary>
/// A list of actions to perform on hit through the ActionBase class.
/// It's mainly used to perform events on raycast hits.
/// </summary>
public abstract class ActionBase : ScriptableObject
{
    public abstract void ExecuteAction(GameObject targetObject, Ray incomingRay);
    public abstract void RevertAction(GameObject targetObject);
}

[CreateAssetMenu(fileName = "SetMaterialAction", menuName = "Actions/Set Material")]
public class SetMaterialActionHandler : ActionBase
{
    public Color color = Color.red;

    public override void ExecuteAction(GameObject targetObject, Ray incomingRay)
    {
        targetObject.GetComponent<Renderer>().material.color = color;
    }

    public override void RevertAction(GameObject targetObject)
    {
        targetObject.GetComponent<Renderer>().material.color = Color.white;
    }
}

[CreateAssetMenu(fileName = "RemoveGameObjectAction", menuName = "Actions/Remove GameObject")]
public class RemoveGameObjectActionHandler : ActionBase
{
    public override void ExecuteAction(GameObject targetObject, Ray incomingRay)
    {
        targetObject.SetActive(false);
    }

    public override void RevertAction(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }
}