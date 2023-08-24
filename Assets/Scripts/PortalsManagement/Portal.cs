using UnityEngine;

public class Portal : MonoBehaviour
{
    public PortalController portalController;

    private void Awake()
    {
        if (!portalController)
        {
            portalController = FindObjectOfType<PortalController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            portalController.TeleportPlayer(this, other.transform);
        }
    }
}