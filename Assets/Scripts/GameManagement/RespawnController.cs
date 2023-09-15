using UnityEngine;
using System.Collections.Generic;

public class RespawnController : MonoBehaviour
{
    public string[] tagsToRespawn; // Tags of objects to respawn (e.g., "Player", "Activator")
    public List<Transform> playerCheckpoints; // List of player checkpoints

    private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
    private Vector3 playerRespawnPosition;

    void Start()
    {
        foreach (string tag in tagsToRespawn)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objectsWithTag)
            {
                Debug.Log("Registered object: " + obj.name + " with initial position: " + initialPositions[obj]);

                if (tag != "Player")
                {
                    initialPositions[obj] = obj.transform.position;
                }
                
            }
        }

        if (playerCheckpoints.Count > 0)
        {
            playerRespawnPosition = playerCheckpoints[0].position; // Set the first checkpoint as the initial respawn position
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object entered respawn area: " + other.name);

        foreach (string tag in tagsToRespawn)
        {
            if (other.CompareTag(tag))
            {
                if (tag == "Player")
                {
                    other.transform.position = playerRespawnPosition;
                }
                else
                {
                    other.transform.position = initialPositions[other.gameObject];
                }
                break;
            }
        }
    }

    public void UpdatePlayerRespawnPosition(Vector3 newRespawnPosition)
    {
        playerRespawnPosition = newRespawnPosition;
    }
}