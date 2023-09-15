using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public RespawnController respawnScript; // Reference to the RespawnObjects script

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            respawnScript.UpdatePlayerRespawnPosition(transform.position);
        }
    }
}