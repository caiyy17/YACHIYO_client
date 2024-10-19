using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Threading;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SignalManager))]
public class TalkingManager : MonoBehaviour
{
    [System.Serializable]
    class TalkingDataEntry
    {
        public AudioClip audioClip;
        public string emotion;
        public string text;
        public string emotion_hint;
        public bool eos;

        // 构造函数，方便创建对象
        public TalkingDataEntry(AudioClip audioClip, string emotion, string text, string emotion_hint)
        {
            this.audioClip = audioClip;
            this.emotion = emotion;
            this.text = text;
            this.emotion_hint = emotion_hint;
            this.eos = false;
        }
        public TalkingDataEntry(bool eos=false)
        {
            this.audioClip = WavUtility.emptyClip;
            this.emotion = "";
            this.text = "";
            this.emotion_hint = "";
            this.eos = eos;
        }
    }
    AudioSource audioSource; 
    SignalManager signalManager;
    Queue<TalkingDataEntry> audioQueue = new Queue<TalkingDataEntry>();

    bool isTalking = false;

    public StringEvent emotionEvent, textEvent, startEvent, stopEvent;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        signalManager = GetComponent<SignalManager>();
    }

    class TalkingData
    {
        public string emotion = "";
        public string emotion_hint = "";
        public string text = "";
        public string audio_data = "";
    }

    public void QueueAudio(string message)
    {
        TalkingData data = JsonUtility.FromJson<TalkingData>(message);
        string audio_base64 = data.audio_data;
        AudioClip clip = string.IsNullOrEmpty(audio_base64) 
            ? WavUtility.emptyClip 
            : WavUtility.ToAudioClip(System.Convert.FromBase64String(audio_base64));
        audioQueue.Enqueue(new TalkingDataEntry(clip, data.emotion, data.text, data.emotion_hint));
    }

    public void QueueEoS(string message){
        audioQueue.Enqueue(new TalkingDataEntry(true));
    }

    void Update()
    {
        if (!audioSource.isPlaying && audioQueue.Count > 0)
        {
            TalkingDataEntry entry = audioQueue.Dequeue();
            if(entry.eos){
                signalManager.SendSignal("answer_end", "finished");
                ResetAll();
            }
            LoadAndPlay(entry.audioClip, entry.emotion, entry.emotion_hint, entry.text);
        }
    }

    void LoadAndPlay(AudioClip clip, string emotion, string emotion_hint, string text)
    {
        audioSource.clip = clip;
        audioSource.Play();
        if(emotion != ""){
            emotionEvent.Invoke(emotion);
            textEvent.Invoke("[" + emotion_hint + "] " + text);
        }
        else{
            textEvent.Invoke(text);
        }
    }

    public void ResetAll(string message="")
    {
        isTalking = false;
        audioQueue.Clear();
        audioSource.Stop();
    }

    public void StateCheck(string message)
    {
        if (message == "answering"){
            startEvent.Invoke("");
            ResetAll();
            isTalking = true;
        }
    }
}