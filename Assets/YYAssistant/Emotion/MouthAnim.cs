using UnityEngine;

public class MouthAnim : MonoBehaviour
{
    public AudioSource audioSource;
    private float[] samples = new float[256];
    
    private float[] spectrumData = new float[256];  // 设置为需要的采样大小，应为2的幂
    public float min_volume = 0;
    public float max_volume = 0.01f;
    public float smoothingFactor = 0.1f;  // 平滑因子，介于0和1之间
    private float ema = 0;  // 初始化为0或第一个数据点的值

    public FloatEvent mouthEvent;

    public void LateUpdate()
    {
        audioSource.GetOutputData(samples, 0);
        float volume = CalculateRMS(samples);
        
        // float volume = 0;
        // if (audioSource.timeSamples > 64 && audioSource.timeSamples < audioSource.clip.samples - 64)
        // {
        //     audioSource.clip.GetData(samples, audioSource.timeSamples - 64);
        //     volume = CalculateRMS(samples);
        // }
        // else
        // {
        //     volume = 0;
        // }
        ema = volume * smoothingFactor + ema * (1 - smoothingFactor);
        
        // audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        // float bandEnergy = CalculateBandEnergy(200, 700, spectrumData, AudioSettings.outputSampleRate);
        // // Debug.Log(bandEnergy);
        // ema = bandEnergy * smoothingFactor + ema * (1 - smoothingFactor);
        MouthControl(ema, min_volume, max_volume);
    }

    float CalculateRMS(float[] samples)
    {
        float sum = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i]; // 平方和
        }
        return Mathf.Sqrt(sum / samples.Length); // 求RMS
    }
    
    float CalculateBandEnergy(int minFreq, int maxFreq, float[] spectrum, int sampleRate)
    {
        // 计算频率索引范围
        int n = spectrum.Length;
        int start = Mathf.FloorToInt(minFreq * n / sampleRate);
        int end = Mathf.CeilToInt(maxFreq * n / sampleRate);

        float sum = 0;
        for (int i = start; i <= end && i < spectrum.Length; i++)
        {
            sum += spectrum[i];
        }

        return sum / (end - start + 1);
    }

    void MouthControl(float volume, float min_volume, float max_volume)
    {
        // float time = Time.time;
        // float value = Mathf.Sin(time * 2.0f) * 0.5f + 0.5f;
        float value = Mathf.Clamp(volume, min_volume, max_volume);
        value = (value - min_volume) / (max_volume - min_volume);
        
        mouthEvent.Invoke(value);
    }
}