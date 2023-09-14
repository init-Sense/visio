using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [System.Serializable]
    public class FloorInfo
    {
        public Collider floorCollider;
        public Transform destinationTransform;
        public GameObject ringToActivate;
        public List<GameObject> objectsToActivate;
        public AudioClip floorSoundtrack;
    }

    public GameObject elevator;
    public List<FloorInfo> floorsInfo;
    public GameObject energyPole;
    public Material energyPoleActivatedMaterial;
    public float duration = 5f;

    public AudioClip startSound;
    public AudioClip movingSound;
    public AudioClip endSound;

    private Renderer energyPoleRenderer;
    private Material energyPoleOriginalMaterial;
    private bool isMoving = false;

    private AudioSource audioSource;
    private AudioSource backgroundMusicSource;
    private Transform playerTransform;

    void Start()
    {
        if (energyPole != null)
        {
            energyPoleRenderer = energyPole.GetComponent<Renderer>();
            energyPoleOriginalMaterial = energyPoleRenderer.material;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;  // Make the sound effects source positional
        audioSource.transform.position = elevator.transform.position;  // Set the position to the center of the elevator
    
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.loop = true;
    }

    public void FloorColliderTriggered(FloorColliderScript floorColliderScript, Transform playerTransform)
    {
        if (isMoving)
        {
            return;
        }

        foreach (var floorInfo in floorsInfo)
        {
            if (floorColliderScript.gameObject == floorInfo.floorCollider.gameObject)
            {
                Debug.Log("Valid collision detected, initiating move.");
                isMoving = true;
                StartCoroutine(SmoothChangeMaterial(energyPoleRenderer, energyPoleOriginalMaterial,
                    energyPoleActivatedMaterial, 1f));
                StartCoroutine(MoveToPosition(elevator.transform.position, floorInfo.destinationTransform.position,
                    floorInfo, playerTransform));
                break;
            }
        }
    }


    IEnumerator SmoothChangeMaterial(Renderer renderer, Material originalMaterial, Material targetMaterial,
        float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            renderer.material.Lerp(originalMaterial, targetMaterial, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        renderer.material = targetMaterial;
    }

    IEnumerator MoveToPosition(Vector3 initialPosition, Vector3 destinationPosition, FloorInfo floorInfo, Transform playerTransform)
    {
        float elapsedTime = 0f;

        // Play start sound
        audioSource.PlayOneShot(startSound);
        // Set moving sound clip before entering the loop
        audioSource.clip = movingSound;
    
        bool movingSoundPlayed = false;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            elevator.transform.position = Vector3.Lerp(initialPosition, destinationPosition, t);

            // Update the position of the audio source to match the elevator's position
            audioSource.transform.position = elevator.transform.position;

            // Set the player to be a child of the elevator temporarily to move with the elevator
            playerTransform.SetParent(elevator.transform);

            elapsedTime += Time.deltaTime;

            // Start playing moving sound once the elevator starts moving and play it in a loop
            if (!movingSoundPlayed && elapsedTime > 0.5f) // Adjust 0.5f to a suitable value that works for you
            {
                audioSource.Play();
                movingSoundPlayed = true;
            }

            yield return null;
        }

        elevator.transform.position = destinationPosition;

        // Stop the moving sound before playing the end sound
        audioSource.Stop();

        // Play end sound
        audioSource.PlayOneShot(endSound);

        // Play the floor's background soundtrack
        if (floorInfo.floorSoundtrack != null)
        {
            backgroundMusicSource.clip = floorInfo.floorSoundtrack;
            backgroundMusicSource.Play();
        }

        if (floorInfo.ringToActivate != null)
        {
            floorInfo.ringToActivate.SetActive(true);
        }
        
        foreach (var obj in floorInfo.objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        floorInfo.floorCollider.enabled = false;
        StartCoroutine(SmoothChangeMaterial(energyPoleRenderer, energyPoleActivatedMaterial, energyPoleOriginalMaterial, 1f));

        // After reaching the destination, reset the player's parent to null
        playerTransform.SetParent(null);

        // After reaching the destination, reset the player's parent to null
        playerTransform.SetParent(null);

        isMoving = false;
    }

}