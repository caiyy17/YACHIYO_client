using System;
using UnityEngine;
using Newtonsoft.Json;

namespace SmplhMotion
{
    /// <summary>
    /// Loads pre-baked idle_motion.json from Resources and initializes SmplhMotionPlayer.
    /// The JSON format matches the Motion API response (base64-encoded float32 arrays).
    /// </summary>
    public class IdleInitializer : MonoBehaviour
    {
        [Tooltip("SmplhMotionPlayer to initialize with idle data")]
        public SmplhMotionPlayer motionPlayer;

        [Tooltip("Resource name of idle motion JSON (without extension)")]
        public string idleResourceName = "idle_motion";

        void Start()
        {
            if (motionPlayer == null)
            {
                Debug.LogError("[IdleInitializer] motionPlayer reference not set");
                return;
            }

            var textAsset = Resources.Load<TextAsset>(idleResourceName);
            if (textAsset == null)
            {
                Debug.LogError($"[IdleInitializer] Resource not found: {idleResourceName}");
                return;
            }

            try
            {
                var data = ParseMotionJson(textAsset.text);
                motionPlayer.Initialize(data);
                Debug.Log($"[IdleInitializer] Idle motion loaded: {data.numFrames} frames, {data.framerate} fps");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IdleInitializer] Failed to parse idle motion: {e.Message}");
            }
        }

        /// <summary>
        /// Parse Motion API JSON response into SmplhMotionData.
        /// Same format as SmplhApiClient.ParseResponse but using Newtonsoft.
        /// </summary>
        public static SmplhMotionData ParseMotionJson(string json)
        {
            var raw = JsonConvert.DeserializeObject<MotionApiResponse>(json);

            if (!string.IsNullOrEmpty(raw.error))
                throw new Exception($"Motion API error: {raw.error}");

            return new SmplhMotionData
            {
                prompt = raw.prompt,
                numFrames = raw.num_frames,
                framerate = raw.framerate,
                posesShape = raw.poses_shape,
                transShape = raw.trans_shape,
                betasShape = raw.betas_shape,
                poses = DecodeBase64Float32(raw.poses),
                trans = DecodeBase64Float32(raw.trans),
                betas = DecodeBase64Float32(raw.betas),
            };
        }

        /// <summary>
        /// Decode a base64 string to a float32 array (little-endian).
        /// </summary>
        public static float[] DecodeBase64Float32(string b64)
        {
            byte[] bytes = Convert.FromBase64String(b64);
            int count = bytes.Length / 4;
            float[] result = new float[count];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }

        [Serializable]
        class MotionApiResponse
        {
            public string error;
            public string prompt;
            public int num_frames;
            public int framerate;
            public string poses;
            public int[] poses_shape;
            public string trans;
            public int[] trans_shape;
            public string betas;
            public int[] betas_shape;
        }
    }
}
