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