using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ArduinoOut : MonoBehaviour
{
    float pitch;
    float roll;
    int tiltZone = 0;

    public float frequency;  // Current desired frequency
    private float phase = 0f;
    private float sampleRate;
    private float lastFreq;
    private float phaseIncrement;

    public int minFrequency = 60;
    public int maxFrequency = 355;
    private AudioSource audioSource;
    public Slider slider;

    private bool isConnected = false;

    void Start()
    {
        UduinoManager.Instance.OnDataReceived += DataReceived;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.1f; // prevent it from being too loud
        
        sampleRate = AudioSettings.outputSampleRate;
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
        isConnected = UduinoManager.Instance.isConnected();

        if (!isConnected)
        {
            Debug.Log("Using slider");
            frequency = slider.value;
            return;
        }
        // Debug.Log("roll: " + roll + ", Pitch: " + pitch);
        float angle = Mathf.Atan2(roll, pitch) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        tiltZone = Mathf.FloorToInt(angle / 10f);

        // if (tiltZone != lastTiltZone)
        // {
        //     lastTiltZone = tiltZone;

            
        // }
        frequency = minFrequency + (tiltZone * 12.2f);
        frequency = Mathf.FloorToInt(frequency);
        frequency = Mathf.Clamp(frequency, minFrequency, maxFrequency);

        Debug.Log($"Zone: {tiltZone} | Angle: {angle:F1}° | Frequency: {frequency}Hz");
    }

    // --- MOST IMPORTANT PART ---
    // This runs automatically to generate audio samples
    void OnAudioFilterRead(float[] data, int channels)
    {
        phaseIncrement = 2f * Mathf.PI * frequency / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += phaseIncrement;
            if (phase > 2f * Mathf.PI)
                phase -= 2f * Mathf.PI;                
            float sample = Mathf.Sin(phase);

            // Write the sample to all channels
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = sample;
            }
        }
    }
}
