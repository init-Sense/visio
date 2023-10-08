using UnityEngine;

/// <summary>
/// This script is used to activate and deactivate GameObjects when hit by a ray.
/// </summary>
public class RayceiverSphereController : MonoBehaviour
{
    [Tooltip("List of GameObjects to activate when hit by the ray.")]
    public GameObject[] gameObjectsToActivate;

    [Tooltip("List of GameObjects to deactivate when hit by the ray.")]
    public GameObject[] gameObjectsToDeactivate;

    [Tooltip("Layer mask to specify which layers the raycast should hit.")]
    public LayerMask raycastLayerMask;
    
    private bool _isActivated = false;
    
   void Update()
    {
        // Check if this object is hit by the ray
        if (IsHitByRay())
        {
            ActivateGameObjects();
            DeactivateGameObjects();
        }
        else
        {
            ResetGameObjects();
        }
    }

   bool IsHitByRay()
   {
       LineRenderer[] lineRenderers = FindObjectsOfType<LineRenderer>();

       foreach (LineRenderer lineRenderer in lineRenderers)
       {
           if (lineRenderer.enabled && (lineRenderer.gameObject.name == "ReflectionLine" || lineRenderer.gameObject.name == "Ray Caster"))
           {
               Vector3 startPoint = lineRenderer.GetPosition(0);
               Vector3 direction = (lineRenderer.GetPosition(1) - startPoint).normalized;

               if (Physics.Raycast(startPoint, direction, out RaycastHit hit, Vector3.Distance(startPoint, lineRenderer.GetPosition(1)), raycastLayerMask))
               {
                   if (hit.collider.gameObject == this.gameObject)
                   {
                       return true;
                   }
               }
           }
       }

       return false;
   }



    void ActivateGameObjects()
    {
        if (!_isActivated)
        {
            foreach (GameObject go in gameObjectsToActivate)
            {
                go.SetActive(true);
            }

            _isActivated = true;
        }
    }

    void DeactivateGameObjects()
    {
        foreach (GameObject go in gameObjectsToDeactivate)
        {
            go.SetActive(false);
        }
    }

    void ResetGameObjects()
    {
        if (_isActivated)
        {
            foreach (GameObject go in gameObjectsToActivate)
            {
                go.SetActive(false);
            }

            foreach (GameObject go in gameObjectsToDeactivate)
            {
                go.SetActive(true);
            }

            _isActivated = false;
        }
    }
}
