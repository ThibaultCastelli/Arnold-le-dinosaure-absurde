using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeMovement : MonoBehaviour
{
    [SerializeField] [Range(0, 100)] int shakeIterations = 10;
    [SerializeField] [Range(0, 10f)] float shakeStrength = 1f;
    [SerializeField][Range(0.01f, 1f)] float shakeIntervals = 0.05f;

    Coroutine _shakeCoroutine;

    private void Awake()
    {
        // Subscribe to events causing a shake
        Events.OnCactusHit += Shake;
    }

    public void Shake()
    {
        // Prevent from having two shakes at the same time
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        // Initialize variables
        Vector3 originalPos = transform.position;
        float shakeCount = 0;
        float xShake;
        float yShake;

        while (shakeCount < shakeIterations)
        {
            // Translate to a random position
            xShake = Random.Range(-shakeStrength, shakeStrength);
            yShake = Random.Range(-shakeStrength, shakeStrength);
            transform.Translate(new Vector3(xShake, yShake, transform.position.z));

            yield return new WaitForSeconds(shakeIntervals);

            // Reset position
            transform.position = originalPos;

            yield return new WaitForSeconds(shakeIntervals);

            shakeCount++;
            // Actualize originalPos
            originalPos = transform.position;
        }

        // Reset position
        transform.position = originalPos;
    }
}
