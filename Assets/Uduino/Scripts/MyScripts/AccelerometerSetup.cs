using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AccelerometerSetup : MonoBehaviour
{
    float pitch;
    float roll;
    int tiltZone = 0;

    float frequency;
    private int lastTiltZone = 1;
    public int minFrequency = 60;
    public int maxFrequency = 355;
    private AudioSource audioSource;
    public Slider slider;

    float lastFreq;

    // NEW: Pre-generated AudioClip storage
    private Dictionary<int, AudioClip> frequencyClips = new Dictionary<int, AudioClip>();

    void Start()
    {
        UduinoManager.Instance.OnDataReceived += DataReceived;

        audioSource = GetComponent<AudioSource>();

        // Set up the AudioSource to loop and not play on awake
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // --- NEW: Pre-generate sine wave clips ---
        GenerateAllFrequencies();

        // Play initial silent clip
        audioSource.clip = frequencyClips[minFrequency];
        audioSource.Play();
    }

    void DataReceived(string data, UduinoDevice board)
    {
        string[] parts = data.Split(',');
        if (parts.Length == 2)
        {
            if (float.TryParse(parts[0], out float p))
                pitch = p;
            if (float.TryParse(parts[1], out float r))
                roll = r;
        }
    }

    void Update()
    {
        if (!UduinoManager.Instance.isConnected())
        {
            frequency = slider.value;
            
            if (lastFreq == frequency){
                Debug.Log("Returned because same freq");
                return;
            }
            lastFreq = frequency;
            int roundedFrequency = Mathf.FloorToInt(frequency);
            roundedFrequency = Mathf.Clamp(roundedFrequency, minFrequency, maxFrequency);

            // --- NEW: Switch to existing pre-generated clip ---
            if (frequencyClips.ContainsKey(roundedFrequency))
            {
                audioSource.clip = frequencyClips[roundedFrequency];
                audioSource.Play();
                Debug.Log(audioSource.clip.name);
            }

            return;
        }

        // Convert pitch + roll to direction in degrees (0-360)
        float angle = Mathf.Atan2(roll, pitch) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // Map to zones
        tiltZone = Mathf.FloorToInt(angle / 10f);
        if (tiltZone == lastTiltZone)
            return;
        else
            lastTiltZone = tiltZone;

        frequency = minFrequency + (tiltZone * 12.2f);
        frequency = Mathf.FloorToInt(frequency);

        // Clamp
        frequency = Mathf.Clamp(frequency, minFrequency, maxFrequency);

        // Adjust AudioSource clip
        int freqKey = Mathf.FloorToInt(frequency);

        if (frequencyClips.ContainsKey(freqKey))
        {
            audioSource.clip = frequencyClips[freqKey];
            audioSource.Play();
        }

        Debug.Log($"Zone: {tiltZone} | Angle: {angle:F1}° | Frequency: {frequency}Hz");
    }

    // --- NEW: Pre-generate all frequencies at startup ---
    void GenerateAllFrequencies()
    {
        int sampleRate = 44100;
        int sampleLength = sampleRate; // 1 second duration

        for (int freq = minFrequency; freq <= maxFrequency; freq++)
        {
            float[] samples = new float[sampleLength];
            for (int i = 0; i < sampleLength; i++)
            {
                samples[i] = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate);
            }

            AudioClip clip = AudioClip.Create($"Sine_{freq}Hz", sampleLength, 1, sampleRate, false);
            clip.SetData(samples, 0);

            frequencyClips[freq] = clip;
        }

        Debug.Log($"Pre-generated {frequencyClips.Count} frequency clips!");
    }
}
