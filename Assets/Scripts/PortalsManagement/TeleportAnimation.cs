using UnityEngine;

public class TeleportAnimation : MonoBehaviour
{
    public GameObject teleportWave;
    public GameObject teleportDisk;
    public Material activeMaterial;
    public Material inactiveMaterial;
    public bool isTeleportActive = false;

    private Renderer teleportWaveRenderer;
    private Renderer teleportDiskRenderer;

    void Start()
    {
        if (teleportWave == null)
        {
            Debug.LogError("Teleport wave is not set");
        }

        teleportWaveRenderer = teleportWave.GetComponent<Renderer>();
        if (teleportWaveRenderer == null)
        {
            Debug.LogError("No Renderer found on the teleport wave object.");
        }

        if (teleportDisk != null)
        {
            teleportDiskRenderer = teleportDisk.GetComponent<Renderer>();
            if (teleportDiskRenderer == null)
            {
                Debug.LogError("No Renderer found on the teleport disk object.");
            }
        }
    }

    void Update()
    {
        UpdateTeleportMaterials();
    }

    private void UpdateTeleportMaterials()
    {
        Material currentMaterial = isTeleportActive ? activeMaterial : inactiveMaterial;
        teleportWaveRenderer.material = currentMaterial;

        if (teleportDiskRenderer != null)
        {
            teleportDiskRenderer.material = currentMaterial;
        }
    }

    public void ActivateTeleport()
    {
        isTeleportActive = true;
    }

    public void DeactivateTeleport()
    {
        isTeleportActive = false;
    }
}