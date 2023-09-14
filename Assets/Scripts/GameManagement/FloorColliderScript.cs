using UnityEngine;

public class FloorColliderScript : MonoBehaviour
{
    public ElevatorController elevatorController;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            elevatorController.FloorColliderTriggered(this, other.transform);
        }
    }
}