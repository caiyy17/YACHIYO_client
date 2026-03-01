using UnityEngine;

namespace SmplhMotion
{
    /// <summary>
    /// Converted pose data for a single frame.
    /// Contains deformation rotations in Unity's coordinate system.
    /// NOT final bone rotations — must be combined with rest rotations by the player.
    /// </summary>
    public struct SmplhPoseFrame
    {
        /// <summary>
        /// X-mirrored SMPL world rotation for each joint (52).
        /// Represents deformation from T-pose in Unity's coordinate system.
        /// Final bone rotation = deformations[j] * canonicalRestRot[j]
        /// </summary>
        public Quaternion[] deformations;

        /// <summary>
        /// Root XZ step this frame (current - previous frame), Unity coords.
        /// Applied as: gameObject.position += rootStepXZ
        /// </summary>
        public Vector3 rootStepXZ;

        /// <summary>Absolute root Y from SMPL (Unity coords, scaled).</summary>
        public float rootY;
    }

    /// <summary>
    /// Converts SMPL-H motion parameters to Unity-compatible rotation data.
    ///
    /// Coordinate systems:
    ///   SMPL-H: right-handed, Y-up, Z-forward, X-left
    ///   Unity:  left-handed,  Y-up, Z-forward, X-right
    ///   Conversion: X-mirror → positions (-x, y, z), quaternions (qx, -qy, -qz, qw)
    ///
    /// Pipeline per frame:
    ///   1. Parse axis-angle → quaternion for each joint (SMPL space)
    ///   2. Forward kinematics: accumulate parent * local → world rotation (SMPL space)
    ///   3. X-mirror world rotation → Unity deformation quaternion
    /// </summary>
    public class SmplhConverter : MonoBehaviour
    {
        public const int NumJoints = 52;
        public const int PoseStride = NumJoints * 3; // 156 floats per frame

        [Tooltip("Scale factor applied to SMPL translation xyz")]
        public float transScale = 1f;

        // ── SMPL-H skeleton definition ──

        /// <summary>Parent joint index for each SMPL-H joint. -1 = root (no parent).</summary>
        public static readonly int[] Parents = {
            -1, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 9, 9, 12, 13, 14, 16, 17, 18, 19,
            // Left hand (22-36): index, middle, pinky, ring, thumb
            20, 22, 23,  20, 25, 26,  20, 28, 29,  20, 31, 32,  20, 34, 35,
            // Right hand (37-51): index, middle, pinky, ring, thumb
            21, 37, 38,  21, 40, 41,  21, 43, 44,  21, 46, 47,  21, 49, 50,
        };

        /// <summary>SMPL-H joint index → Unity HumanBodyBones enum.</summary>
        public static readonly HumanBodyBones[] BoneMapping = {
            HumanBodyBones.Hips,                     // 0  pelvis
            HumanBodyBones.LeftUpperLeg,             // 1  l_hip
            HumanBodyBones.RightUpperLeg,            // 2  r_hip
            HumanBodyBones.Spine,                    // 3  spine1
            HumanBodyBones.LeftLowerLeg,             // 4  l_knee
            HumanBodyBones.RightLowerLeg,            // 5  r_knee
            HumanBodyBones.Chest,                    // 6  spine2
            HumanBodyBones.LeftFoot,                 // 7  l_ankle
            HumanBodyBones.RightFoot,                // 8  r_ankle
            HumanBodyBones.UpperChest,               // 9  spine3
            HumanBodyBones.LeftToes,                 // 10 l_foot
            HumanBodyBones.RightToes,                // 11 r_foot
            HumanBodyBones.Neck,                     // 12 neck
            HumanBodyBones.LeftShoulder,             // 13 l_collar
            HumanBodyBones.RightShoulder,            // 14 r_collar
            HumanBodyBones.Head,                     // 15 head
            HumanBodyBones.LeftUpperArm,             // 16 l_shoulder
            HumanBodyBones.RightUpperArm,            // 17 r_shoulder
            HumanBodyBones.LeftLowerArm,             // 18 l_elbow
            HumanBodyBones.RightLowerArm,            // 19 r_elbow
            HumanBodyBones.LeftHand,                 // 20 l_wrist
            HumanBodyBones.RightHand,                // 21 r_wrist
            // Left hand 22-36
            HumanBodyBones.LeftIndexProximal,        // 22
            HumanBodyBones.LeftIndexIntermediate,    // 23
            HumanBodyBones.LeftIndexDistal,          // 24
            HumanBodyBones.LeftMiddleProximal,       // 25
            HumanBodyBones.LeftMiddleIntermediate,   // 26
            HumanBodyBones.LeftMiddleDistal,         // 27
            HumanBodyBones.LeftLittleProximal,       // 28
            HumanBodyBones.LeftLittleIntermediate,   // 29
            HumanBodyBones.LeftLittleDistal,         // 30
            HumanBodyBones.LeftRingProximal,         // 31
            HumanBodyBones.LeftRingIntermediate,     // 32
            HumanBodyBones.LeftRingDistal,           // 33
            HumanBodyBones.LeftThumbProximal,        // 34
            HumanBodyBones.LeftThumbIntermediate,    // 35
            HumanBodyBones.LeftThumbDistal,          // 36
            // Right hand 37-51
            HumanBodyBones.RightIndexProximal,       // 37
            HumanBodyBones.RightIndexIntermediate,   // 38
            HumanBodyBones.RightIndexDistal,         // 39
            HumanBodyBones.RightMiddleProximal,      // 40
            HumanBodyBones.RightMiddleIntermediate,  // 41
            HumanBodyBones.RightMiddleDistal,        // 42
            HumanBodyBones.RightLittleProximal,      // 43
            HumanBodyBones.RightLittleIntermediate,  // 44
            HumanBodyBones.RightLittleDistal,        // 45
            HumanBodyBones.RightRingProximal,        // 46
            HumanBodyBones.RightRingIntermediate,    // 47
            HumanBodyBones.RightRingDistal,          // 48
            HumanBodyBones.RightThumbProximal,       // 49
            HumanBodyBones.RightThumbIntermediate,   // 50
            HumanBodyBones.RightThumbDistal,         // 51
        };

        /// <summary>SMPL-H joint names for debug output.</summary>
        public static readonly string[] JointNames = {
            "pelvis", "l_hip", "r_hip", "spine1", "l_knee", "r_knee", "spine2",
            "l_ankle", "r_ankle", "spine3", "l_foot", "r_foot", "neck",
            "l_collar", "r_collar", "head", "l_shoulder", "r_shoulder",
            "l_elbow", "r_elbow", "l_wrist", "r_wrist",
            "l_index1", "l_index2", "l_index3",
            "l_middle1", "l_middle2", "l_middle3",
            "l_pinky1", "l_pinky2", "l_pinky3",
            "l_ring1", "l_ring2", "l_ring3",
            "l_thumb1", "l_thumb2", "l_thumb3",
            "r_index1", "r_index2", "r_index3",
            "r_middle1", "r_middle2", "r_middle3",
            "r_pinky1", "r_pinky2", "r_pinky3",
            "r_ring1", "r_ring2", "r_ring3",
            "r_thumb1", "r_thumb2", "r_thumb3",
        };

        // ── Reusable buffers (avoid per-frame allocation) ──
        readonly Quaternion[] _smplWorldBuf = new Quaternion[NumJoints];
        readonly Quaternion[] _deformBuf = new Quaternion[NumJoints];

        // ── Conversion API ──

        /// <summary>
        /// Deep-copy a SmplhPoseFrame (allocates a new deformations array).
        /// </summary>
        public static SmplhPoseFrame DeepCopy(SmplhPoseFrame src)
        {
            var copy = new SmplhPoseFrame
            {
                rootStepXZ = src.rootStepXZ,
                rootY = src.rootY,
                deformations = new Quaternion[src.deformations.Length]
            };
            System.Array.Copy(src.deformations, copy.deformations, src.deformations.Length);
            return copy;
        }

        /// <summary>
        /// Convert all frames in a motion data set to SmplhPoseFrame[].
        /// Each frame gets its own deformations array (independent of internal buffers).
        /// </summary>
        public SmplhPoseFrame[] ConvertAll(SmplhMotionData data)
        {
            var frames = new SmplhPoseFrame[data.numFrames];
            for (int i = 0; i < data.numFrames; i++)
                frames[i] = DeepCopy(ConvertFrame(data.poses, i, data.trans));
            return frames;
        }

        /// <summary>
        /// Convert one frame of SMPL-H data to Unity deformation rotations.
        /// </summary>
        public SmplhPoseFrame ConvertFrame(float[] poses, int frame, float[] trans)
        {
            int poseOffset = frame * PoseStride;
            int t = frame * 3;

            var result = new SmplhPoseFrame
            {
                deformations = _deformBuf
            };

            // Step 1: Forward kinematics in SMPL space
            var smplWorld = _smplWorldBuf;
            for (int j = 0; j < NumJoints; j++)
            {
                int idx = poseOffset + j * 3;
                Quaternion localRot = AxisAngleToQuat(poses[idx], poses[idx + 1], poses[idx + 2]);

                smplWorld[j] = j == 0
                    ? localRot
                    : smplWorld[Parents[j]] * localRot;
            }

            // Step 2: X-mirror each world rotation to Unity coordinates
            for (int j = 0; j < NumJoints; j++)
            {
                Quaternion q = smplWorld[j];
                result.deformations[j] = new Quaternion(q.x, -q.y, -q.z, q.w);
            }

            // Step 3: Root translation (scaled, then X-mirrored)
            float sx = trans[t] * transScale;
            float sy = trans[t + 1] * transScale;
            float sz = trans[t + 2] * transScale;

            // XZ: frame-to-frame step. Frame 0 = zero.
            if (frame > 0)
            {
                int p = (frame - 1) * 3;
                float px = trans[p] * transScale;
                float pz = trans[p + 2] * transScale;
                result.rootStepXZ = new Vector3(-(sx - px), 0, sz - pz);
            }

            // Y: absolute (scaled)
            result.rootY = sy;

            return result;
        }

        /// <summary>Convert SMPL-H axis-angle (Rodrigues vector) to quaternion. No coordinate conversion.</summary>
        public static Quaternion AxisAngleToQuat(float rx, float ry, float rz)
        {
            float theta = Mathf.Sqrt(rx * rx + ry * ry + rz * rz);
            if (theta < 1e-8f)
                return Quaternion.identity;
            float half = theta * 0.5f;
            float s = Mathf.Sin(half) / theta;
            return new Quaternion(s * rx, s * ry, s * rz, Mathf.Cos(half));
        }

    }
}
