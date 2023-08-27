﻿using UnityEngine;
using System.Collections.Generic;

public class PortalController : MonoBehaviour
{
    [System.Serializable]
    public class PortalEntry
    {
        public Portal portal;
        public Transform destination;
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
                return;
            }
        }
    }
}