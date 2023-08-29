using UnityEngine;

public class AreaController : MonoBehaviour
{
    [Tooltip("Total number of levels in this area.")]
    public int totalLevels;

    [System.Serializable]
    public class ActionableGameObject
    {
        public GameObject targetObject;
        public ActionBase action;
    }

    [Tooltip("List of GameObjects and actions to perform when the area is completed.")]
    public ActionableGameObject[] areaCompletedActions;

    private int completedLevels = 0;

    private bool isAreaCompleted = false;

    public void OnLevelCompleted()
    {
        completedLevels++;
        Debug.Log("Level completed. Total completed levels: " + completedLevels);

        if (completedLevels >= totalLevels)
        {
            isAreaCompleted = true;
            Debug.Log("All levels completed. Executing area completed actions.");
            foreach (ActionableGameObject actionableGameObject in areaCompletedActions)
            {
                actionableGameObject.action.ExecuteAction(actionableGameObject.targetObject, new Ray());
            }
        }
    }

    public void OnLevelReverted()
    {
        if (completedLevels > 0)
        {
            completedLevels--;
        }
        Debug.Log("Level reverted. Total completed levels: " + completedLevels);

        if (completedLevels < totalLevels)
        {
            isAreaCompleted = false;
            Debug.Log("Reverting area completed actions.");
            foreach (ActionableGameObject actionableGameObject in areaCompletedActions)
            {
                actionableGameObject.action.RevertAction(actionableGameObject.targetObject);
            }
        }
    }

    
    public bool IsAreaCompleted()
    {
        return isAreaCompleted;
    }
}