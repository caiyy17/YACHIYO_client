using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(EmotionManager))]
[RequireComponent(typeof(ContentLoader))]
public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;
    Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    Queue<string> emotionQueue = new Queue<string>();
    Queue<string> textQueue = new Queue<string>();
    EmotionManager emotionManager;
    ContentLoader contentLoader;
    public bool isAudioLoadingOrPlaying = false;

    public bool StopPlayingFlag = false;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        emotionManager = gameObject.GetComponent<EmotionManager>();
        contentLoader = gameObject.GetComponent<ContentLoader>();
    }

    public void QueueAudio(int index, string text, string emotion, string audio_base64)
    {
        if(!StopPlayingFlag)
        {
            // 将base64字符串转换为byte数组
            byte[] audioData = System.Convert.FromBase64String(audio_base64);
            // 将byte的wav存入clip
            // 使用coroutine保存clip
            StartCoroutine(WavUtility.ToAudioClip(audioData, (clip) => {
                audioQueue.Enqueue(clip);
                emotionQueue.Enqueue(emotion);
                textQueue.Enqueue(text);
            }));
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying && audioQueue.Count > 0)
        {
            AudioClip clip = audioQueue.Dequeue();
            string emotion = emotionQueue.Dequeue();
            string text = textQueue.Dequeue();
            LoadAndPlay(clip, emotion, text);
        }
        
        if (audioSource.isPlaying || audioQueue.Count > 0)
        {
            isAudioLoadingOrPlaying = true;
        }
        else
        {
            isAudioLoadingOrPlaying = false;
        }
    }

    void LateUpdate()
    {
        if (StopPlayingFlag)
        {
            ResetAll();
            StopPlayingFlag = false;
            return;
        }
    }

    void LoadAndPlay(AudioClip clip, string emotion, string text)
    {
        audioSource.clip = clip;
        audioSource.Play();
        emotionManager.SetMotionAndExpression(emotion);
        contentLoader.AddText(text);
    }

    public void ResetAll()
    {
        audioQueue.Clear();
        emotionQueue.Clear();
        textQueue.Clear();
        audioSource.Stop();
        contentLoader.ClearImage();
        contentLoader.ClearText();
        isAudioLoadingOrPlaying = false;
    }
    
}