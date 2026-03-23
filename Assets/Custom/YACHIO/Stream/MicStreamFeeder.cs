using UnityEngine;

/// <summary>
/// Feeds MicrophoneManager audio into a streaming AudioClip for WebRTC AudioStreamTrack.
///
/// Reads from MicrophoneManager's circular buffer in Update() (main thread),
/// writes to a ring buffer. Audio thread reads via PCMReaderCallback.
/// Pre-fills ring buffer on init so OnPCMRead never starves.
/// </summary>
public class MicStreamFeeder : MonoBehaviour
{
    private const int PREFILL_MS = 200;

    private float[] ringBuffer;
    private volatile int writeHead;
    private volatile int readHead;
    private int capacity;

    private int lastMicSample;
    private int micBufferSize;
    private bool ready;

    public AudioClip CreateStreamingClip()
    {
        var mic = MicrophoneManager.Instance;
        if (mic == null)
        {
            Debug.LogError("[MicStreamFeeder] MicrophoneManager not available");
            return null;
        }

        int sampleRate = mic.sampleRate;
        micBufferSize = sampleRate * 60;

        capacity = sampleRate * 2;
        ringBuffer = new float[capacity];

        // Pre-fill: read last PREFILL_MS of mic data into ring buffer
        int prefillSamples = sampleRate * PREFILL_MS / 1000;
        int currentPos = mic.GetCurrentSamplePosition();
        int startPos = (currentPos - prefillSamples + micBufferSize) % micBufferSize;

        float[] prefillData = mic.GetAudioData(startPos, currentPos);
        if (prefillData != null && prefillData.Length > 0)
        {
            for (int i = 0; i < prefillData.Length; i++)
                ringBuffer[i] = prefillData[i];
            writeHead = prefillData.Length;
        }
        else
        {
            writeHead = 0;
        }
        readHead = 0;
        lastMicSample = currentPos;
        ready = true;

        AudioClip clip = AudioClip.Create(
            "MicStream", sampleRate, 1, sampleRate, true, OnPCMRead
        );

        Debug.Log($"[MicStreamFeeder] Init: rate={sampleRate}Hz, prefill={prefillData?.Length ?? 0} samples ({PREFILL_MS}ms)");
        return clip;
    }

    void Update()
    {
        if (!ready) return;
        var mic = MicrophoneManager.Instance;
        if (mic == null) return;

        int currentPos = mic.GetCurrentSamplePosition();
        if (currentPos == lastMicSample) return;

        float[] micData = mic.GetAudioData(lastMicSample, currentPos);
        lastMicSample = currentPos;

        if (micData == null || micData.Length == 0) return;

        int w = writeHead;
        for (int i = 0; i < micData.Length; i++)
        {
            ringBuffer[w] = micData[i];
            w = (w + 1) % capacity;
        }
        writeHead = w;
    }

    private void OnPCMRead(float[] data)
    {
        if (!ready || ringBuffer == null)
        {
            System.Array.Clear(data, 0, data.Length);
            return;
        }

        int r = readHead;
        int w = writeHead;
        for (int i = 0; i < data.Length; i++)
        {
            if (r != w)
            {
                data[i] = ringBuffer[r];
                r = (r + 1) % capacity;
            }
            else
            {
                data[i] = 0f;
            }
        }
        readHead = r;
    }
}
