using UnityEngine;

public class TransparencyController : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // The object to be made transparent.
    [SerializeField] private float transparencyLevel = 0.5f; // Set the transparency level.
    private Material _material;
    private Color _originalColor;

    void Start()
    {
        _material = targetObject.GetComponent<Renderer>().material;
        _originalColor = _material.color;
    }

    public void SetTransparent(bool isTransparent)
    {
        if (isTransparent)
        {
            // If we want it transparent, set the alpha value to the transparency level.
            _material.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, transparencyLevel);
        }
        else
        {
            // If we want it opaque, restore the original color.
            _material.color = _originalColor;
        }
    }
}