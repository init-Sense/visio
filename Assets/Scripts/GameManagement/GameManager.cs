using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ElevatorController elevatorController; // Elevator controller
    public List<LevelController> levels; // List of levels
    private int currentLevelIndex = 0; // Current level index

    void Start()
    {
        if (!elevatorController)
        {
            Debug.LogError("ElevatorController is not set for GameManager script. Please set it in the Unity Editor.");
        }
    }

    // Called when a game object is put in the collider
    public void OnObjectInserted(GameObject insertedObject, Transform elevatorTarget)
    {
        // Check if the inserted object is the correct one for the current level
        if (IsCorrectObjectForLevel(insertedObject, currentLevelIndex))
        {
            // Move the elevator to the next level
            elevatorController.MoveTo(elevatorTarget, insertedObject);


            // Complete the level and initiate level rotation and object destruction
            if (currentLevelIndex < levels.Count)
            {
                levels[currentLevelIndex].CompleteLevel();
            }

            // Increase the current level index
            currentLevelIndex++;
        }
        else
        {
            Debug.Log("Inserted object is not correct for the current level.");
        }
    }

    // Checks if the inserted object is the correct one for the current level
    private bool IsCorrectObjectForLevel(GameObject insertedObject, int levelIndex)
    {
        // Here you can implement your logic to check if the inserted object is the correct one for the current level.
        // This might involve checking the tag or name of the inserted object, comparing it to a stored reference, etc.
        // For now, let's just return true to assume any inserted object is correct.
        return true;
    }
}