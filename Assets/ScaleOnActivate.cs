using UnityEngine;

public class ScaleOnActivate : MonoBehaviour
{
    public float animationDuration = 1.0f;

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUp());
    }

    public void StartScaleDown()
    {
        StartCoroutine(ScaleDown());
    }

    private System.Collections.IEnumerator ScaleUp()
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private System.Collections.IEnumerator ScaleDown()
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }
}