using UnityEngine;
using UnityEngine.Serialization;

public class TeleportAnimation : MonoBehaviour
{
    public Transform waveStartPoint;
    public Transform waveEndPoint;
    public GameObject teleportWave;
    public GameObject teleportDisk;
    public Material activeMaterial;
    public Material inactiveMaterial;
    public float waveSpeed = 1.0f;
    public bool isTeleportActive = false;

    private Renderer teleportWaveRenderer;
    private Renderer teleportDiskRenderer;

    void Start()
    {
        if (waveStartPoint == null || waveEndPoint == null || teleportWave == null)
        {
            Debug.LogError("Wave start point, end point, or teleport wave is not set");
        }

        teleportWaveRenderer = teleportWave.GetComponent<Renderer>();
        if (teleportWaveRenderer == null)
        {
            Debug.LogError("No Renderer found on the teleport wave object.");
        }

        if (teleportDisk != null) // Add this block
        {
            teleportDiskRenderer = teleportDisk.GetComponent<Renderer>();
            if (teleportDiskRenderer == null)
            {
                Debug.LogError("No Renderer found on the other game object.");
            }
        }
    }

    void Update()
    {
        AnimateTeleportWave();
    }

    private void AnimateTeleportWave()
    {
        float currentWaveSpeed = isTeleportActive ? waveSpeed : waveSpeed / 2;
        Material currentMaterial = isTeleportActive ? activeMaterial : inactiveMaterial;
        teleportWaveRenderer.material = currentMaterial;

        if (teleportDiskRenderer != null) // Add this line
        {
            teleportDiskRenderer.material = currentMaterial;
        }

        Vector3 startPosition = waveStartPoint.position;
        Vector3 endPosition = waveEndPoint.position;

        float distance = Vector3.Distance(startPosition, endPosition);
        float timeToReachEnd = distance / currentWaveSpeed;

        float t = (Time.time % timeToReachEnd) / timeToReachEnd;
        teleportWave.transform.position = Vector3.Lerp(startPosition, endPosition, t);

        float scale = Mathf.Lerp(1, 0, t);
        teleportWave.transform.localScale = new Vector3(1, scale, 1);
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
