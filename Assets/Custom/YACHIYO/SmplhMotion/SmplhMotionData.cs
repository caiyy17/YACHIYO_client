namespace SmplhMotion
{
    /// <summary>
    /// Decoded SMPL-H motion data ready for playback.
    /// Matches the Motion API JSON response format after base64 decoding.
    /// </summary>
    public class SmplhMotionData
    {
        public string prompt;
        public int numFrames;
        public int framerate;
        public float[] poses;    // flat [numFrames * 156]
        public int[] posesShape; // [numFrames, 156]
        public float[] trans;    // flat [numFrames * 3]
        public int[] transShape; // [numFrames, 3]
        public float[] betas;    // flat
        public int[] betasShape;
    }
}
