using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using SmplhMotion;

/// <summary>
/// Pipeline module that receives SMPL-H motion data from ContentModule
/// and plays it on the character via SmplhMotionPlayer.
///
/// Replaces ActionModule when using SMPL config.
/// Pipeline: ContentModule → SmplhActionModule
/// </summary>
public class SmplhActionModule : ProcessingModuleSynchronous
{
    [Header("SMPL-H References")]
    [Tooltip("SmplhMotionPlayer on the character")]
    public SmplhMotionPlayer motionPlayer;

    [Tooltip("SmplhConverter on the character")]
    public SmplhConverter converter;

    SignalManager signalManager;

    void Awake()
    {
        moduleName = "SmplhActionModule";
        captuedSignals = new List<string> { "SoS", "EoS" };
        signalManager = FindObjectOfType<SignalManager>();
    }

    void Start()
    {
        if (signalManager != null)
        {
            signalManager.AddSignal("action_set", OnActionSet);
        }
    }

    void OnDisable()
    {
        if (signalManager != null)
        {
            signalManager.RemoveSignal("action_set", OnActionSet);
        }
    }

    protected override void ProcessMessage(string message)
    {
        YYMessage baseMessage = JsonUtility.FromJson<YYMessage>(message);

        if (baseMessage.signal == "SoS")
        {
            outputQueue.Add(message);
            return;
        }

        if (baseMessage.signal == "EoS")
        {
            if (motionPlayer != null && motionPlayer.IsPlaying)
            {
                motionPlayer.ReturnToIdle();
            }
            outputQueue.Add(message);
            return;
        }

        // Action data from ContentModule (signal == "")
        if (string.IsNullOrEmpty(baseMessage.content)) return;

        // Consume action field, forward the rest
        var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseMessage.content);
        if (jsonDict != null && jsonDict.ContainsKey("action"))
        {
            ProcessSmplAction(baseMessage.content);
            jsonDict.Remove("action");
        }

        if (jsonDict != null && jsonDict.Count > 0)
        {
            baseMessage.content = JsonConvert.SerializeObject(jsonDict);
            outputQueue.Add(JsonUtility.ToJson(baseMessage));
        }
    }

    void OnActionSet(string message)
    {
        ProcessSmplAction(message);
    }

    void ProcessSmplAction(string content)
    {
        if (motionPlayer == null || converter == null)
        {
            LogInfo("motionPlayer or converter not assigned, skipping");
            return;
        }

        if (!motionPlayer.IsPlaying)
        {
            LogInfo("motionPlayer not initialized, skipping");
            return;
        }

        try
        {
            // Extract action string from {"action": "..."}
            var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            if (jsonDict == null || !jsonDict.ContainsKey("action")) return;

            string actionStr = jsonDict["action"].ToString();
            if (string.IsNullOrEmpty(actionStr)) return;

            // Parse SMPL motion JSON
            SmplhMotionData motionData = IdleInitializer.ParseMotionJson(actionStr);

            if (motionData.numFrames <= 30)
            {
                LogInfo($"Motion too short ({motionData.numFrames} frames), skipping");
                return;
            }

            // Convert and play
            SmplhPoseFrame[] frames = converter.ConvertAll(motionData);
            motionPlayer.SetCurrentMotion(frames);
            LogInfo($"Playing SMPL motion: {motionData.numFrames} frames, prompt={motionData.prompt}");
        }
        catch (Exception e)
        {
            LogError($"Failed to process SMPL action: {e.Message}");
        }
    }
}
