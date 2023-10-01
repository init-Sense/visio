using UnityEngine;

[CreateAssetMenu(fileName = "SetMaterialAction", menuName = "Actions/Set Material")]
public class SetMaterialAction : ActionBase
{
    public Material material;
    public Material defaultMaterial;

    public override void ExecuteAction(GameObject targetObject, Ray incomingRay)
    {
        targetObject.GetComponent<Renderer>().material = material;
    }

    public override void RevertAction(GameObject targetObject)
    {
        targetObject.GetComponent<Renderer>().material = defaultMaterial;
    }
}