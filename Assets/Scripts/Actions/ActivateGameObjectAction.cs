using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Activate GameObject")]
public class ActivateGameObjectAction : ActionBase
{
    public override void ExecuteAction(GameObject targetObject, Ray incomingRay, float transparencyIncrement = 0f)
    {
        targetObject.SetActive(true);
    }

    public override void RevertAction(GameObject targetObject)
    {
        ScaleOnActivate scaleOnActivate = targetObject.GetComponent<ScaleOnActivate>();
        if (scaleOnActivate != null)
        {
            scaleOnActivate.StartScaleDown();
        }
        targetObject.SetActive(false);
    }
}