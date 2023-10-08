using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A centralized script that controls all the portals in the scene.
/// </summary>
public class PortalController : MonoBehaviour
{
    [System.Serializable]
    public class PortalEntry
    {
        public Portal portal;
        public Transform destination;
        public TeleportAnimation teleportAnimation;
    }

    public List<PortalEntry> portals = new List<PortalEntry>();

    public void TeleportPlayer(Portal portal, Transform playerTransform)
    {
        foreach (var portalEntry in portals)
        {
            if (portalEntry.portal == portal)
            {
                playerTransform.position = portalEntry.destination.position;
                playerTransform.rotation = portalEntry.destination.rotation;
                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                if (portalEntry.teleportAnimation != null)
                {
                    portalEntry.teleportAnimation.DeactivateTeleport();
                }

                return;
            }
        }
    }
}