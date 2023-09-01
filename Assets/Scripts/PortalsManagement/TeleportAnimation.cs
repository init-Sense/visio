using UnityEngine;

public class TeleportAnimation : MonoBehaviour
{
    public Transform waveStartPoint;
    public Transform waveEndPoint;
    public GameObject teleportWave;
    public float waveSpeed = 1.0f;

    void Start()
    {
        if (waveStartPoint == null || waveEndPoint == null || teleportWave == null)
        {
            Debug.LogError("Wave start point, end point, or teleport wave is not set");
        }
    }

    void Update()
    {
        AnimateTeleportWave();
    }

    private void AnimateTeleportWave()
    {
        float waveSpeed = this.waveSpeed;

        Vector3 startPosition = waveStartPoint.position;
        Vector3 endPosition = waveEndPoint.position;

        float distance = Vector3.Distance(startPosition, endPosition);
        float timeToReachEnd = distance / waveSpeed;

        float t = (Time.time % timeToReachEnd) / timeToReachEnd;
        teleportWave.transform.position = Vector3.Lerp(startPosition, endPosition, t);

        float scale = Mathf.Lerp(1, 0, t);
        teleportWave.transform.localScale = new Vector3(1, scale, 1);
    }
}