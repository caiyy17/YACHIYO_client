using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yachiyo
{
    public class Anim3D : MonoBehaviour
    {
        // --- Motion: multiple Animators, each with its own ActionMap ---
        [Serializable]
        public class MotionTarget
        {
            public Animator animator;
            public ActionMap actionMap;
            [NonSerialized] public List<int> triggerHashes;
        }

        [Header("Motion")]
        [SerializeField] private List<MotionTarget> motionTargets = new List<MotionTarget>();
        [SerializeField] private string idleAction = "idle";
        [SerializeField] private float idleTimeout = 10f;
        private float lastMotionTime;

        // --- Expression: multiple SkinnedMeshRenderers, each with its own ActionMap ---
        // ActionMap values = blendshape names on the mesh
        [Serializable]
        public class ExpressionTarget
        {
            public SkinnedMeshRenderer renderer;
            public ActionMap actionMap;
            public float transitionSpeed = 10f;
            // Runtime: same pattern as MotionTarget's triggerHashes
            [NonSerialized] public Dictionary<string, int> nameToIndex;
            [NonSerialized] public List<int> managedIndices;
            [NonSerialized] public int activeIndex = -1;
        }

        [Header("Expression")]
        [SerializeField] private List<ExpressionTarget> expressionTargets = new List<ExpressionTarget>();
        [SerializeField] private float expressionTimeout = 5f;
        private float lastExpressionTime;

        // --- Blink ---
        [Serializable]
        public class BlinkTarget
        {
            public SkinnedMeshRenderer renderer;
            public int blendShapeIndex;
            public float blinkDuration = 0.4f;
            public float interval = 3.0f;
            public float threshold = 0.3f;
            public float closeRatio = 100f;
            public float halfCloseRatio = 20f;
            [NonSerialized] public float blinkTimer;
            [NonSerialized] public float intervalTimer;
            [NonSerialized] public bool isBlinking;
        }

        [Header("Blink")]
        [SerializeField] private List<BlinkTarget> blinkTargets = new List<BlinkTarget>();

        // --- Mouth: multiple SkinnedMeshRenderers, each with its own thresholds ---
        [Serializable]
        public class MouthTarget
        {
            public SkinnedMeshRenderer renderer;
            public int blendShapeIndex;
            public float minVolume = 0.03f;
            public float maxVolume = 0.15f;
            public float smoothingFactor = 0.4f;
            [NonSerialized] public float ema;
        }

        [Header("Mouth")]
        [SerializeField] private AudioSource mouthAudioSource;
        [SerializeField] private List<MouthTarget> mouthTargets = new List<MouthTarget>();
        private float[] mouthSamples = new float[256];


        void Start()
        {
            lastMotionTime = Time.time;
            lastExpressionTime = Time.time;

            foreach (var target in motionTargets)
            {
                if (target.actionMap != null)
                    target.actionMap.Initialize();
                if (target.animator != null)
                    CacheTriggerHashes(target);
            }

            foreach (var target in expressionTargets)
            {
                if (target.actionMap != null)
                    target.actionMap.Initialize();
                if (target.renderer != null)
                    CacheBlendShapeIndices(target);
            }

            foreach (var target in blinkTargets)
            {
                target.intervalTimer = target.interval;
            }
        }

        void Update()
        {
            if (idleTimeout > 0 && Time.time - lastMotionTime > idleTimeout)
            {
                SetMotion(idleAction);
            }

            if (expressionTimeout > 0 && Time.time - lastExpressionTime > expressionTimeout)
            {
                SetExpression("neutral");
            }
        }

        void LateUpdate()
        {
            // Expression smooth transition (runs first)
            foreach (var target in expressionTargets)
            {
                if (target.renderer == null || target.managedIndices == null) continue;
                if (target.transitionSpeed <= 0) continue;

                // Skip blink blendshape when no expression is active (let blink control it)
                int blinkIdx = GetBlinkIndex(target.renderer);

                foreach (int idx in target.managedIndices)
                {
                    if (idx == blinkIdx && target.activeIndex < 0) continue;
                    float goal = (idx == target.activeIndex) ? 100f : 0f;
                    float current = target.renderer.GetBlendShapeWeight(idx);
                    if (Mathf.Abs(current - goal) < 0.1f)
                    {
                        target.renderer.SetBlendShapeWeight(idx, goal);
                        continue;
                    }
                    target.renderer.SetBlendShapeWeight(idx,
                        Mathf.Lerp(current, goal, Time.deltaTime * target.transitionSpeed));
                }
            }

            // Blink (runs after expression, paused when expression is active)
            foreach (var blink in blinkTargets)
            {
                if (blink.renderer == null) continue;

                // Pause blink when expression is active on the same renderer
                if (IsExpressionActive(blink.renderer))
                {
                    if (blink.isBlinking)
                    {
                        blink.renderer.SetBlendShapeWeight(blink.blendShapeIndex, 0f);
                        blink.isBlinking = false;
                        blink.blinkTimer = 0f;
                    }
                    continue;
                }

                // Interval countdown → roll dice to start blink
                blink.intervalTimer -= Time.deltaTime;
                if (blink.intervalTimer <= 0f)
                {
                    blink.intervalTimer = blink.interval;
                    if (!blink.isBlinking && UnityEngine.Random.value > blink.threshold)
                    {
                        blink.isBlinking = true;
                        blink.blinkTimer = blink.blinkDuration;
                    }
                }

                // Blink animation
                if (blink.isBlinking)
                {
                    blink.blinkTimer -= Time.deltaTime;
                    if (blink.blinkTimer <= 0f)
                    {
                        blink.renderer.SetBlendShapeWeight(blink.blendShapeIndex, 0f);
                        blink.isBlinking = false;
                    }
                    else if (blink.blinkTimer <= blink.blinkDuration * 0.3f)
                    {
                        blink.renderer.SetBlendShapeWeight(blink.blendShapeIndex, blink.halfCloseRatio);
                    }
                    else
                    {
                        blink.renderer.SetBlendShapeWeight(blink.blendShapeIndex, blink.closeRatio);
                    }
                }
            }

            // Mouth (runs last, overrides expression on mouth blendshape)
            if (mouthAudioSource != null && mouthTargets.Count > 0)
            {
                mouthAudioSource.GetOutputData(mouthSamples, 0);
                float rms = CalculateRMS(mouthSamples);

                foreach (var target in mouthTargets)
                {
                    if (target.renderer == null) continue;
                    target.ema = rms * target.smoothingFactor + target.ema * (1 - target.smoothingFactor);
                    float value = Mathf.Clamp(target.ema, target.minVolume, target.maxVolume);
                    value = (value - target.minVolume) / (target.maxVolume - target.minVolume);
                    target.renderer.SetBlendShapeWeight(target.blendShapeIndex, value * 100);
                }
            }
        }

        // --- Public API ---

        public void SetMotion(string action)
        {
            lastMotionTime = Time.time;

            foreach (var target in motionTargets)
            {
                if (target.actionMap == null || target.animator == null) continue;

                if (target.actionMap.TryGetEntry(action, out var entry))
                {
                    string motion = entry.values[UnityEngine.Random.Range(0, entry.values.Count)];
                    ResetAllTriggers(target);
                    target.animator.SetTrigger(motion);
                }
            }
            Debug.Log("SetMotion: " + action);
        }

        public void SetExpression(string action)
        {
            lastExpressionTime = Time.time;

            foreach (var target in expressionTargets)
            {
                if (target.actionMap == null || target.renderer == null) continue;

                // Unknown key = neutral (all blendshapes to 0)
                int newIndex = -1;
                if (target.actionMap.TryGetEntry(action, out var entry)
                    && entry.values != null && entry.values.Count > 0)
                {
                    string name = entry.values[UnityEngine.Random.Range(0, entry.values.Count)];
                    if (!target.nameToIndex.TryGetValue(name, out newIndex))
                        newIndex = -1;
                }

                // Instant apply if no transition, otherwise LateUpdate handles it
                if (target.transitionSpeed <= 0)
                {
                    ResetAllBlendShapes(target);
                    if (newIndex >= 0)
                        target.renderer.SetBlendShapeWeight(newIndex, 100f);
                }

                target.activeIndex = newIndex;
            }
            Debug.Log("SetExpression: " + action);
        }

        public void MouthControl(float value)
        {
            foreach (var target in mouthTargets)
            {
                if (target.renderer == null) continue;
                target.renderer.SetBlendShapeWeight(target.blendShapeIndex, value * 100);
            }
        }

        // --- Internal: Motion ---

        private void CacheTriggerHashes(MotionTarget target)
        {
            target.triggerHashes = new List<int>();
            foreach (var param in target.animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    target.triggerHashes.Add(Animator.StringToHash(param.name));
            }
        }

        private void ResetAllTriggers(MotionTarget target)
        {
            if (target.triggerHashes == null) return;
            foreach (int hash in target.triggerHashes)
                target.animator.ResetTrigger(hash);
        }

        // --- Internal: Expression ---

        private void CacheBlendShapeIndices(ExpressionTarget target)
        {
            target.nameToIndex = new Dictionary<string, int>();
            target.managedIndices = new List<int>();
            Mesh mesh = target.renderer.sharedMesh;

            if (target.actionMap == null) return;
            foreach (var entry in target.actionMap.entries)
            {
                if (entry.values == null) continue;
                foreach (string name in entry.values)
                {
                    if (target.nameToIndex.ContainsKey(name)) continue;
                    int idx = mesh.GetBlendShapeIndex(name);
                    if (idx >= 0)
                    {
                        target.nameToIndex[name] = idx;
                        target.managedIndices.Add(idx);
                    }
                    else
                    {
                        Debug.LogWarning($"Anim3D: BlendShape '{name}' not found on {target.renderer.name}");
                    }
                }
            }
        }

        private void ResetAllBlendShapes(ExpressionTarget target)
        {
            if (target.managedIndices == null) return;
            foreach (int idx in target.managedIndices)
                target.renderer.SetBlendShapeWeight(idx, 0f);
        }

        // --- Internal: Blink ---

        private bool IsExpressionActive(SkinnedMeshRenderer renderer)
        {
            foreach (var target in expressionTargets)
            {
                if (target.renderer == renderer && target.activeIndex >= 0)
                    return true;
            }
            return false;
        }

        private int GetBlinkIndex(SkinnedMeshRenderer renderer)
        {
            foreach (var blink in blinkTargets)
            {
                if (blink.renderer == renderer)
                    return blink.blendShapeIndex;
            }
            return -1;
        }

        // --- Internal: Shared ---

        private float CalculateRMS(float[] samples)
        {
            float sum = 0;
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i] * samples[i];
            return Mathf.Sqrt(sum / samples.Length);
        }
    }
}
