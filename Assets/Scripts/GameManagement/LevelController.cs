using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public List<GameObject> objectsToDestroyOnCompletion; // Objects to destroy upon level completion
    public float rotationSpeed = 20f; // Speed of level rotation
    private bool isLevelCompleted = false; // Flag to indicate level completion

    void Update()
    {
        if (isLevelCompleted)
        {
            // Rotate the level around the Y axis
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    public void CompleteLevel()
    {
        // Destroy objects marked for destruction
        foreach (GameObject obj in objectsToDestroyOnCompletion)
        {
            Destroy(obj);
        }

        isLevelCompleted = true;
    }
}