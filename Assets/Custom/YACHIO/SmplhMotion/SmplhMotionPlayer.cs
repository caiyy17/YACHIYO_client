using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmplhMotion
{
    /// <summary>
    /// Frame-buffer-based SMPL-H motion player.
    ///
    /// Maintains a continuous frame buffer that is consumed for playback.
    /// Idle motion auto-refills when buffer runs low; action motions can be
    /// injected at any time with smooth crossfade blending.
    ///
    /// Rotation formula (localRotation for retarget offset support):
    ///   targetWorld = rootRot * D[j] * B[j]
    ///   bone.localRotation = Inv(parent.rotation) * targetWorld
    ///
    /// Mathematically equivalent to Inv(B[p]) * smplLocal * B[j] when
    /// Unity parent == SMPL-H parent. Using Inv(parent.rotation) is more
    /// robust as it naturally handles intermediate bones (e.g. MMD models).
    /// Retarget offsets (two-pass):
    ///   Pass 1: bone.localRotation = Inv(parent.rotation) * rootRot * D[j] * B[j]
    ///   Pass 2: bone.localRotation *= Inv(B[j]) * offset * B[j]
    ///   Inserts offset in SMPL-H deformation frame, propagates to children via hierarchy.
    ///
    /// Root XZ:   game object position += rootRot * rootStepXZ
    /// Root Y:    hips bone Y = initialHipsY + (frameRootY - frame0RootY)
    /// </summary>
    [System.Serializable]
    public class RetargetOffset
    {
        public HumanBodyBones bone;
        public Vector3 euler;
    }

    [RequireComponent(typeof(SmplhConverter))]
    public class SmplhMotionPlayer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Animator on the character model (required)")]
        public Animator targetAnimator;

        [Header("Retarget")]
        [Tooltip("Per-joint rotation offsets applied in local space (euler degrees)")]
        public List<RetargetOffset> retargetOffsets = new List<RetargetOffset>();

        [Header("Playback")]
        [Tooltip("Playback speed multiplier")]
        public float playbackSpeed = 1f;

        [Tooltip("Target framerate for playback")]
        public int framerate = 30;

        [Tooltip("Number of frames used for crossfade blending")]
        public int blendLength = 10;

        [Tooltip("When remaining frames <= this, auto-refill with idle")]
        public int refillThreshold = 15;

        [Tooltip("When true, auto-refill loops the current action instead of idle")]
        public bool loopMotion = false;

        const int CompactThreshold = 200;

        // ── Public state ──
        public bool IsPlaying { get; private set; }
        public int BufferRemaining => _buffer.Count - _consumeIndex;

        // ── Frame buffer ──
        readonly List<SmplhPoseFrame> _buffer = new List<SmplhPoseFrame>();
        int _consumeIndex;
        SmplhPoseFrame[] _idleFramesCache;
        SmplhPoseFrame[] _currentMotionCache;

        // ── Internal state ──
        SmplhConverter _converter;
        Transform _characterRoot; // the character's transform (for root XZ movement)
        Coroutine _playCoroutine;

        Transform[] _boneTransforms;
        Quaternion[] _bindModel;     // B[j]: bind pose rotations in model space
        Quaternion[] _retargetOffsetByJoint; // per SMPL-H joint offset (identity if none)
        Transform _hipsTransform;
        Vector3 _initialObjectPos;
        float _initialHipsY;
        float _frame0RootY;

        SmplhPoseFrame _pendingPose;
        bool _hasPendingPose;

        // ── Lifecycle ──

        void Awake()
        {
            _converter = GetComponent<SmplhConverter>();
            if (targetAnimator != null)
                CacheBones();
        }

        void Update()
        {
            if (!_hasPendingPose) return;
            ApplyPose(_pendingPose);
            _hasPendingPose = false;
        }

        // ── Bone initialization ──

        void CacheBones()
        {
            _characterRoot = targetAnimator.transform;

            int n = SmplhConverter.NumJoints;
            _boneTransforms = new Transform[n];
            _bindModel = new Quaternion[n];

            for (int i = 0; i < n; i++)
                _boneTransforms[i] = targetAnimator.GetBoneTransform(SmplhConverter.BoneMapping[i]);

            _hipsTransform = _boneTransforms[0];
            _initialObjectPos = _characterRoot.position;
            _initialHipsY = _hipsTransform != null ? _hipsTransform.position.y : 0f;

            ComputeBindPoseModelRotations();
            RebuildRetargetOffsets();
        }

        /// <summary>
        /// Compute bind pose rotations in MODEL space (B[j]) from avatar.humanDescription.skeleton.
        /// Model space starts from identity (not root.rotation), independent of scene placement.
        /// </summary>
        void ComputeBindPoseModelRotations()
        {
            int n = SmplhConverter.NumJoints;

            // Build name → bind pose local rotation from Avatar skeleton data
            var skelBones = targetAnimator.avatar.humanDescription.skeleton;
            var nameToBindLocal = new Dictionary<string, Quaternion>(skelBones.Length);
            for (int i = 0; i < skelBones.Length; i++)
                nameToBindLocal[skelBones[i].name] = skelBones[i].rotation;

            for (int i = 0; i < n; i++)
            {
                if (_boneTransforms[i] == null)
                {
                    _bindModel[i] = Quaternion.identity;
                    continue;
                }

                // Build ancestor chain: character root → ... → bone
                var chain = new List<Transform>();
                var t = _boneTransforms[i];
                while (t != null && t != _characterRoot)
                {
                    chain.Add(t);
                    t = t.parent;
                }
                chain.Reverse();

                // Chain bind pose local rotations via FK (model space: start from identity)
                Quaternion modelRot = Quaternion.identity;
                foreach (var ancestor in chain)
                {
                    modelRot *= nameToBindLocal.TryGetValue(ancestor.name, out Quaternion bindLocal)
                        ? bindLocal
                        : ancestor.localRotation;
                }
                _bindModel[i] = modelRot;
            }
        }

        /// <summary>
        /// Rebuild per-SMPL-H-joint offset lookup from the retargetOffsets list.
        /// Called at init and every frame to support live Inspector editing.
        /// </summary>
        void RebuildRetargetOffsets()
        {
            int n = SmplhConverter.NumJoints;
            if (_retargetOffsetByJoint == null || _retargetOffsetByJoint.Length != n)
                _retargetOffsetByJoint = new Quaternion[n];

            for (int i = 0; i < n; i++)
                _retargetOffsetByJoint[i] = Quaternion.identity;

            if (retargetOffsets == null) return;
            var mapping = SmplhConverter.BoneMapping;

            foreach (var entry in retargetOffsets)
            {
                if (entry == null) continue;
                for (int i = 0; i < n; i++)
                {
                    if (mapping[i] == entry.bone)
                    {
                        _retargetOffsetByJoint[i] = Quaternion.Euler(entry.euler);
                        break;
                    }
                }
            }
        }

        // ── Public API ──

        /// <summary>
        /// Set initial idle motion, fill buffer, and start continuous playback.
        /// _frame0RootY is set once here and never reset.
        /// </summary>
        public void Initialize(SmplhMotionData idleData)
        {
            Stop();

            // Cache bones on first Initialize if not done in Awake
            // (targetAnimator may be assigned after Awake)
            if (_boneTransforms == null && targetAnimator != null)
                CacheBones();

            _idleFramesCache = _converter.ConvertAll(idleData);
            framerate = idleData.framerate > 0 ? idleData.framerate : 30;

            _buffer.Clear();
            _consumeIndex = 0;
            for (int i = 0; i < _idleFramesCache.Length; i++)
                _buffer.Add(SmplhConverter.DeepCopy(_idleFramesCache[i]));

            // Set frame0RootY from the very first frame — never changes after this
            if (_buffer.Count > 0)
                _frame0RootY = _buffer[0].rootY;

            if (targetAnimator != null)
                targetAnimator.enabled = false;

            IsPlaying = true;
            _playCoroutine = StartCoroutine(PlaybackCoroutine());

            Debug.Log($"[MotionPlayer] Initialized: {_idleFramesCache.Length} idle frames, " +
                      $"framerate={framerate}, frame0RootY={_frame0RootY:F4}");
        }

        /// <summary>
        /// Replace idle motion (takes effect on next auto-refill).
        /// </summary>
        public void SetIdleMotion(SmplhMotionData newIdle)
        {
            _idleFramesCache = _converter.ConvertAll(newIdle);
            Debug.Log($"[MotionPlayer] Idle updated: {_idleFramesCache.Length} frames");
        }

        /// <summary>
        /// Inject an action motion (must be > 30 frames).
        /// Truncates buffer to 15 frames ahead, then crossfade-blends the action in.
        /// Result layout: 5 original + 10 blended + (len-10) new.
        /// </summary>
        public void SetCurrentMotion(SmplhPoseFrame[] frames)
        {
            if (frames == null || frames.Length <= 30)
            {
                Debug.LogWarning($"[MotionPlayer] SetCurrentMotion rejected: " +
                                 $"{frames?.Length ?? 0} frames (need > 30)");
                return;
            }

            // Truncate buffer: keep only 15 unconsumed frames
            int keep = _consumeIndex + refillThreshold;
            if (keep < _buffer.Count)
                _buffer.RemoveRange(keep, _buffer.Count - keep);

            _currentMotionCache = frames;
            AppendWithBlend(frames);

            Debug.Log($"[MotionPlayer] Action injected: remaining={BufferRemaining}, " +
                      $"buffer={_buffer.Count}, loopMotion={loopMotion}");
        }

        /// <summary>
        /// Truncate buffer to 15 frames ahead and trigger idle refill.
        /// </summary>
        public void ReturnToIdle()
        {
            _currentMotionCache = null;

            int keep = _consumeIndex + refillThreshold;
            if (keep < _buffer.Count)
                _buffer.RemoveRange(keep, _buffer.Count - keep);

            CheckRefill();

            Debug.Log($"[MotionPlayer] ReturnToIdle: remaining={BufferRemaining}");
        }

        public void Stop()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }
            IsPlaying = false;
            _hasPendingPose = false;
        }

        /// <summary>
        /// Stop playback and reset character to rest pose.
        /// </summary>
        public void ResetPose()
        {
            Stop();
            _buffer.Clear();
            _consumeIndex = 0;

            if (_boneTransforms != null)
            {
                // Reset to bind pose: targetWorld = rootRot * B[j]
                Quaternion rootRot = _characterRoot != null ? _characterRoot.rotation : Quaternion.identity;
                for (int i = 0; i < SmplhConverter.NumJoints; i++)
                {
                    if (_boneTransforms[i] != null)
                    {
                        Quaternion targetWorld = rootRot * _bindModel[i];
                        _boneTransforms[i].localRotation =
                            Quaternion.Inverse(_boneTransforms[i].parent.rotation) * targetWorld;
                    }
                }
            }
            if (_characterRoot != null)
                _characterRoot.position = _initialObjectPos;
            if (_hipsTransform != null)
            {
                Vector3 hipsPos = _hipsTransform.position;
                hipsPos.y = _initialHipsY;
                _hipsTransform.position = hipsPos;
            }

            if (targetAnimator != null)
                targetAnimator.enabled = true;
        }

        // ── Buffer management ──

        /// <summary>
        /// Crossfade buffer tail (last blendLength frames) with incoming front,
        /// then deep-copy-append remaining incoming frames.
        /// </summary>
        void AppendWithBlend(SmplhPoseFrame[] incoming)
        {
            int blendLen = Mathf.Min(blendLength, _buffer.Count, incoming.Length);
            int blendStart = _buffer.Count - blendLen;

            // Crossfade overlap region
            for (int i = 0; i < blendLen; i++)
            {
                float t = (float)i / blendLength;
                var bufFrame = _buffer[blendStart + i];
                var incFrame = incoming[i];

                var blended = new SmplhPoseFrame
                {
                    deformations = new Quaternion[SmplhConverter.NumJoints],
                    rootStepXZ = Vector3.Lerp(bufFrame.rootStepXZ, incFrame.rootStepXZ, t),
                    rootY = Mathf.Lerp(bufFrame.rootY, incFrame.rootY, t),
                };

                for (int j = 0; j < SmplhConverter.NumJoints; j++)
                    blended.deformations[j] = Quaternion.Slerp(
                        bufFrame.deformations[j], incFrame.deformations[j], t);

                _buffer[blendStart + i] = blended;
            }

            // Append remaining incoming frames (deep copied)
            for (int i = blendLen; i < incoming.Length; i++)
                _buffer.Add(SmplhConverter.DeepCopy(incoming[i]));
        }

        void CheckRefill()
        {
            if (BufferRemaining > refillThreshold) return;

            // loopMotion: prefer current action, fall back to idle
            var source = (loopMotion && _currentMotionCache != null)
                ? _currentMotionCache
                : _idleFramesCache;

            if (source == null || source.Length == 0) return;
            AppendWithBlend(source);
        }

        /// <summary>
        /// Remove consumed frames from the front of the buffer to prevent unbounded growth.
        /// </summary>
        void CompactBuffer()
        {
            if (_consumeIndex < CompactThreshold) return;
            _buffer.RemoveRange(0, _consumeIndex);
            _consumeIndex = 0;
        }

        // ── Playback ──

        IEnumerator PlaybackCoroutine()
        {
            while (true)
            {
                int fps = framerate > 0 ? framerate : 30;
                float interval = 1f / fps;

                if (_consumeIndex < _buffer.Count)
                {
                    _pendingPose = _buffer[_consumeIndex];
                    _hasPendingPose = true;
                    _consumeIndex++;
                }

                CheckRefill();
                CompactBuffer();

                yield return new WaitForSeconds(interval / playbackSpeed);
            }
        }

        // ── Pose application ──

        void ApplyPose(SmplhPoseFrame pose)
        {
            RebuildRetargetOffsets();

            Quaternion rootRot = _characterRoot.rotation;

            // Pass 1: Set all bones to base SMPL-H pose
            for (int j = 0; j < SmplhConverter.NumJoints; j++)
            {
                if (_boneTransforms[j] == null) continue;
                Quaternion targetWorld = rootRot * pose.deformations[j] * _bindModel[j];
                _boneTransforms[j].localRotation =
                    Quaternion.Inverse(_boneTransforms[j].parent.rotation) * targetWorld;
            }

            // Pass 2: Apply retarget offsets in SMPL-H deformation frame
            // localRot *= Inv(B[j]) * offset * B[j] inserts offset between D[j] and B[j],
            // equivalent to: worldRot = rootRot * ... * sL[j] * offset * ... * B[j]
            // Offset follows bone pose and propagates to children via hierarchy.
            for (int j = 0; j < SmplhConverter.NumJoints; j++)
            {
                if (_boneTransforms[j] == null) continue;
                if (_retargetOffsetByJoint[j] == Quaternion.identity) continue;
                _boneTransforms[j].localRotation *=
                    Quaternion.Inverse(_bindModel[j]) * _retargetOffsetByJoint[j] * _bindModel[j];
            }

            // XZ: frame-to-frame step rotated into character's orientation
            _characterRoot.position += rootRot * pose.rootStepXZ;

            // Y: SMPL delta from frame 0, applied to initial hips Y
            if (_hipsTransform != null)
            {
                Vector3 hipsPos = _hipsTransform.position;
                hipsPos.y = _initialHipsY + (pose.rootY - _frame0RootY);
                _hipsTransform.position = hipsPos;
            }
        }

    }
}
