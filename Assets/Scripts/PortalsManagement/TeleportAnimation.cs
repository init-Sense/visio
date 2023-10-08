using UnityEngine;

public class TeleportAnimation : MonoBehaviour
{
    [Tooltip("Teleport renderer. This will change material when the teleport is active.")]
    public GameObject teleportWave;

    [Tooltip("The material which will be used when the teleport is active.")]
    public Material activeMaterial;

    [Tooltip("The material which will be used when the teleport is inactive.")]
    public Material inactiveMaterial;

    [Tooltip("You can also determine if a teleport starts as active or inactive.")]
    public bool isTeleportActive = false;

    private Renderer _teleportWaveRenderer;
    private Renderer _teleportDiskRenderer;

    void Start()
    {
        if (teleportWave == null)
        {
            Debug.LogError("Teleport wave is not set");
        }

        _teleportWaveRenderer = teleportWave.GetComponent<Renderer>();
        if (_teleportWaveRenderer == null)
        {
            Debug.LogError("No Renderer found on the teleport wave object.");
        }
    }

    void Update()
    {
        UpdateTeleportMaterials();
    }

    private void UpdateTeleportMaterials()
    {
        Material currentMaterial = isTeleportActive ? activeMaterial : inactiveMaterial;
        _teleportWaveRenderer.material = currentMaterial;

        if (_teleportDiskRenderer != null)
        {
            _teleportDiskRenderer.material = currentMaterial;
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