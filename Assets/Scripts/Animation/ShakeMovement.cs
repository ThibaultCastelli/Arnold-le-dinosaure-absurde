using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeMovement : MonoBehaviour
{
    [SerializeField] [Range(0, 100)] int shakeIterations = 10;
    [SerializeField] [Range(0, 10f)] float shakeStrength = 1f;
    [SerializeField][Range(0.01f, 1f)] float shakeIntervals = 0.05f;

    Coroutine _shakeCoroutine;

    private void OnEnable()
    {
        // Subscribe to events causing a shake
        Events.OnCactusHit += Shake;
        Events.OnCamShake += Shake;
    }

    private void OnDisable()
    {
        // Unsubscribe to events causing a shake
        Events.OnCactusHit -= Shake;
        Events.OnCamShake -= Shake;
    }

    public void Shake()
    {
        // Prevent from having two shakes at the same time
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeIterations, shakeStrength, shakeIntervals));
    }

    public void Shake(int shakeIterations, float shakeStrength, float shakeIntervals)
    {
        // Prevent from having two shakes at the same time
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeIterations, shakeStrength, shakeIntervals));
    }

    private IEnumerator ShakeCoroutine(int shakeIterations, float shakeStrength, float shakeIntervals)
    {
        // Initialize variables
        Vector3 originalPos = transform.position;
        float shakeCount = 0;
        float xShake;
        float yShake;
        int rndSign;

        while (shakeCount < shakeIterations)
        {
            // Wait a frame on original position
            yield return null;

            // Translate to a random position
            rndSign = Random.Range(0, 1) == 1 ? 1 : -1;
            xShake = Random.Range(shakeStrength / 2, shakeStrength) * rndSign;
            rndSign = Random.Range(0, 1) == 1 ? 1 : -1;
            yShake = Random.Range(shakeStrength / 2, shakeStrength) * rndSign;

            transform.Translate(new Vector3(xShake, yShake, 0));

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
