using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SetMaterialAction", menuName = "Actions/Set Material")]
public class SetMaterialAction : ActionBase
{
    public Color color = Color.red;

    public override void ExecuteAction(GameObject targetObject, Ray incomingRay, float transparencyIncrement = 0f)
    {
        targetObject.GetComponent<Renderer>().material.color = color;
    }

    public override void RevertAction(GameObject targetObject)
    {
        targetObject.GetComponent<Renderer>().material.color = Color.white;
    }
}