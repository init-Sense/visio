using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public RespawnController respawnScript;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            respawnScript.UpdatePlayerRespawnPosition(transform.position);
        }
    }
}