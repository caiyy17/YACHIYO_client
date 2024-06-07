using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public static class WavUtility
{
    public struct WavFormat
    {
        public float[] samples;
        public int samplesCount;
        public int channels;
        public int frequency;
    }

    public static WavFormat Load(AudioClip audio)
    {
        var samples = new float[audio.samples * audio.channels];
        audio.GetData(samples, 0);
        UnityEngine.Debug.Log(audio.frequency);
        return new WavFormat
        {
            samples = samples,
            samplesCount = audio.samples,
            channels = audio.channels,
            frequency = audio.frequency
        };
    }
    public static void Save(string filepath, WavFormat wav)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
        byte[] wavData = ConvertToWav(wav.samples, wav.samplesCount, wav.channels, wav.frequency);
        // 将字节数组写入文件
        File.WriteAllBytes(filepath, wavData);
    }

    public static byte[] ConvertToWav(float[] samples, int samplesCount, int channels, int frequency)
    {
        byte[] wav = new byte[samplesCount * 2 + 44]; // 2 bytes per sample + 44 byte header
        Buffer.BlockCopy(BitConverter.GetBytes(0x46464952), 0, wav, 0, 4); // "RIFF" in ASCII
        Buffer.BlockCopy(BitConverter.GetBytes(36 + samplesCount * 2), 0, wav, 4, 4); // file size
        Buffer.BlockCopy(BitConverter.GetBytes(0x45564157), 0, wav, 8, 4); // "WAVE" in ASCII
        Buffer.BlockCopy(BitConverter.GetBytes(0x20746D66), 0, wav, 12, 4); // "fmt " in ASCII
        Buffer.BlockCopy(BitConverter.GetBytes(16), 0, wav, 16, 4); // sub chunk size
        Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, wav, 20, 2); // PCM format
        Buffer.BlockCopy(BitConverter.GetBytes((short)channels), 0, wav, 22, 2); // number of channels
        Buffer.BlockCopy(BitConverter.GetBytes(frequency), 0, wav, 24, 4); // sample rate
        Buffer.BlockCopy(BitConverter.GetBytes(frequency * channels * 2), 0, wav, 28, 4); // byte rate
        Buffer.BlockCopy(BitConverter.GetBytes((short)(channels * 2)), 0, wav, 32, 2); // block align
        Buffer.BlockCopy(BitConverter.GetBytes((short)16), 0, wav, 34, 2); // bits per sample
        Buffer.BlockCopy(BitConverter.GetBytes(0x61746164), 0, wav, 36, 4); // "data" in ASCII
        Buffer.BlockCopy(BitConverter.GetBytes(samplesCount * 2), 0, wav, 40, 4); // sub chunk 2 size

        // Convert float array to 16 bit integer and write to byte array
        int offset = 44;
        for (int i = 0; i < samples.Length; i++)
        {
            short val = (short)(samples[i] * 32767);
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, wav, offset, 2);
            offset += 2;
        }

        return wav;
    }

    public static WavFormat TrimSilence(WavFormat wav, float min)
    {
        var samples = wav.samples;

        int i;
        for (i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > min)
            {
                break;
            }
        }

        int start = i;

        for (i = samples.Length - 1; i >= 0; i--)
        {
            if (Mathf.Abs(samples[i]) > min)
            {
                break;
            }
        }

        int end = i;

        var trimmedSamples = new float[end - start];
        Array.Copy(samples, start, trimmedSamples, 0, trimmedSamples.Length);

        var trimmedClip = new WavFormat
        {
            samples = trimmedSamples,
            samplesCount = trimmedSamples.Length,
            channels = wav.channels,
            frequency = wav.frequency
        };

        return trimmedClip;
    }

    public static IEnumerator ToAudioClip(byte[] wavBytes, Action<AudioClip> onComplete, string name = "audioClip")
    {
        // 使用内存流读取字节
        using (MemoryStream stream = new MemoryStream(wavBytes))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // 读取头部
                byte[] riff = reader.ReadBytes(4);
                int size = reader.ReadInt32();
                byte[] wave = reader.ReadBytes(4);
                byte[] fmt = reader.ReadBytes(4);
                int fmtSize = reader.ReadInt32();
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16();

                byte[] data = reader.ReadBytes(4);
                int dataSize = reader.ReadInt32();

                // 读取数据
                byte[] audioData = reader.ReadBytes(dataSize);

                // 创建 AudioClip
                AudioClip audioClip = AudioClip.Create(name, dataSize / (bitDepth / 8) / channels, channels, sampleRate, false);
                float[] audioFloats = new float[audioClip.samples * audioClip.channels];
                int sampleCount = 0;

                // 根据 bit depth 转换字节到 float
                for (int i = 0; i < dataSize; i += bitDepth / 8)
                {
                    float sample = 0;
                    switch (bitDepth)
                    {
                        case 16:
                            sample = BitConverter.ToInt16(audioData, i) / 32768f;
                            break;
                        case 8:
                            sample = (audioData[i] - 128) / 128f;
                            break;
                        default:
                            throw new Exception("Unsupported WAV bit depth: " + bitDepth);
                    }
                    audioFloats[sampleCount++] = sample;
                }
                audioClip.SetData(audioFloats, 0);

                // 模拟耗时操作，实际中可以去掉这一行
                yield return null;

                // 完成后通过回调返回 AudioClip
                onComplete?.Invoke(audioClip);
            }
        }
    }
    public static AudioClip ToAudioClip2(byte[] wavBytes, string name = "audioClip")
    {
        // 使用内存流读取字节
        using (MemoryStream stream = new MemoryStream(wavBytes))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // 读取头部
                byte[] riff = reader.ReadBytes(4);
                int size = reader.ReadInt32();
                byte[] wave = reader.ReadBytes(4);
                byte[] fmt = reader.ReadBytes(4);
                int fmtSize = reader.ReadInt32();
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16();

                byte[] data = reader.ReadBytes(4);
                int dataSize = reader.ReadInt32();

                // 读取数据
                byte[] audioData = reader.ReadBytes(dataSize);

                // 创建 AudioClip
                AudioClip audioClip = AudioClip.Create(name, dataSize / (bitDepth / 8) / channels, channels, sampleRate, false);
                float[] audioFloats = new float[audioClip.samples * audioClip.channels];
                int sampleCount = 0;

                // 根据 bit depth 转换字节到 float
                for (int i = 0; i < dataSize; i += bitDepth / 8)
                {
                    float sample = 0;
                    switch (bitDepth)
                    {
                        case 16:
                            sample = BitConverter.ToInt16(audioData, i) / 32768f;
                            break;
                        case 8:
                            sample = (audioData[i] - 128) / 128f;
                            break;
                        default:
                            throw new Exception("Unsupported WAV bit depth: " + bitDepth);
                    }
                    audioFloats[sampleCount++] = sample;
                }
                audioClip.SetData(audioFloats, 0);

                return audioClip;
            }
        }
    }
}
