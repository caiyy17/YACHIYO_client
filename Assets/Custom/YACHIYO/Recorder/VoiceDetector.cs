using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System;

namespace Yachiyo
{
    public class VoiceDetector : MonoBehaviour
    {
        private MicrophoneManager microphoneManager;
        private int sampleRate;
        private float loudnessHoldTimer = 0f;
        private float silenceHoldTimer = 0f;
        public bool useVAD = false;
        private bool _useVAD = false;

        public float speakingThreshold = 0.01f;
        public float silenceThreshold = 0.001f;
        public float loudnessHoldTime = 0.3f;
        public float silenceHoldTime = 0.1f;
        public float timeWindow = 0.3f;
        public bool isSpeaking = false;
        public float currentLoudness = 0;

        public InputAction recordButton;

        void Awake()
        {
            useVAD = false;
            _useVAD = true;
        }
        void Start()
        {
            microphoneManager = MicrophoneManager.Instance;
            sampleRate = microphoneManager.sampleRate;
            silenceThreshold = PlayerPrefs.GetFloat("silenceThreshold", silenceThreshold);
            speakingThreshold = PlayerPrefs.GetFloat("speakingThreshold", speakingThreshold);
            recordButton.Enable();
        }

        void OnDisable()
        {
            recordButton.Disable();
        }

        void Update()
        {
            if (!isSpeaking)
            {
                if (recordButton.WasPerformedThisFrame())
                {
                    isSpeaking = true;
                    _useVAD = false;
                }
                else if (useVAD && _useVAD)
                {
                    currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
                    if (currentLoudness > speakingThreshold)
                    {
                        loudnessHoldTimer += Time.deltaTime;
                        if (loudnessHoldTimer >= loudnessHoldTime)
                        {
                            isSpeaking = true;
                            loudnessHoldTimer = 0f; // 重置计时器
                        }
                    }
                    else
                    {
                        loudnessHoldTimer = 0f; // 未达到阈值时重置计时器
                    }
                }
                else
                {
                    loudnessHoldTimer = 0f; // 未达到阈值时重置计时器
                }
            }
            else
            {
                if (recordButton.WasReleasedThisFrame())
                {
                    isSpeaking = false;
                    _useVAD = true;
                }
                else if (useVAD && _useVAD)
                {
                    currentLoudness = microphoneManager.GetCurrentLoudness(timeWindow);
                    if (currentLoudness < silenceThreshold)
                    {
                        silenceHoldTimer += Time.deltaTime;
                        if (silenceHoldTimer >= silenceHoldTime)
                        {
                            isSpeaking = false;
                            silenceHoldTimer = 0f; // 重置计时器
                        }
                    }
                    else
                    {
                        silenceHoldTimer = 0f; // 未达到阈值时重置计时器
                    }
                }
                else if (!useVAD && _useVAD)
                { //中途切换到非VAD模式
                    silenceHoldTimer = 0f; // 未达到阈值时重置计时器
                    isSpeaking = false;
                }
            }
        }
        public void SetVAD(bool value)
        {
            useVAD = value;
        }
    }
}
