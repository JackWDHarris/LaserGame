using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TargetTone : MonoBehaviour
{
    public float target; // The frequency the player must match
    public float timeMatched = 1f; // Time required to hold the frequency
    public float matchThreshold = 1f; // Allowed difference to count as matched
    private float matchTimer = 0f;
    private float timeBetween = 2;

    public ArduinoOut playerSource; // Reference to the player's script

    private float phase = 0f;
    private float sampleRate;
    private float phaseIncrement;
    private AudioSource audioSource;

    private bool isWaitingForNext = false;

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.05f;

        PickNewTarget();
    }

    void Update()
    {
        if (isWaitingForNext) return;

        float playerFreq = playerSource.frequency;

        if (Mathf.Abs(playerFreq - target) <= matchThreshold)
        {
            matchTimer += Time.deltaTime;

            if (matchTimer >= timeMatched)
            {
                Debug.Log("Match successful!");
                StartCoroutine(WaitAndPickNewTarget(timeBetween)); // 1 second pause
            }
        }
        else
        {
            matchTimer = 0f;
        }
    }

    void PickNewTarget()
    {
        int zone = Random.Range(0, 30); // 360 / 12.2 = ~30 zones
        target = playerSource.minFrequency + (zone * 12.2f);
        target = Mathf.FloorToInt(target);
        target = Mathf.Clamp(target, playerSource.minFrequency, playerSource.maxFrequency);

        Debug.Log($"New target frequency: {target} Hz");
        matchTimer = 0f;
    }

    IEnumerator WaitAndPickNewTarget(float delay)
    {
        audioSource.Stop();
        isWaitingForNext = true;
        yield return new WaitForSeconds(delay);
        isWaitingForNext = false;
        audioSource.Play();
        PickNewTarget();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        phaseIncrement = 2f * Mathf.PI * target / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += phaseIncrement;
            if (phase > 2f * Mathf.PI)
                phase -= 2f * Mathf.PI;
            float sample = Mathf.Sin(phase);

            for (int c = 0; c < channels; c++)
            {
                data[i + c] = sample;
            }
        }
    }
}
