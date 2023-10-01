using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Complete Level Action")]
public class CompleteLevelAction : ActionBase
{
    private bool isReverted = false;

    public override void ExecuteAction(GameObject targetObject, Ray incomingRay)
    {
        AreaController areaController = targetObject.GetComponent<AreaController>();
        if (areaController != null)
        {
            Debug.Log("Executing level completed action.");
            areaController.OnLevelCompleted();
        }
        else
        {
            Debug.LogError("AreaController not found on target object: " + targetObject.name);
        }

        isReverted = false;
    }

    public override void RevertAction(GameObject targetObject)
    {
        if (isReverted)
        {
            return;
        }

        AreaController areaController = targetObject.GetComponent<AreaController>();
        if (areaController != null)
        {
            Debug.Log("Executing level reverted action.");
            areaController.OnLevelReverted();
        }
        else
        {
            Debug.LogError("AreaController not found on target object: " + targetObject.name);
        }

        isReverted = true;
    }

}