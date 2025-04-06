using System.Collections;
using UnityEngine;

namespace SuperTiled2Unity.Sample.Seams
{
    public class SeamMaker : MonoBehaviour
    {
        public float MinCameraSize;
        public float MaxCameraSize;
        public float Duration = 4.0f;

        IEnumerator Start()
        {
            Camera camera = GetComponent<Camera>();

            while (true)
            {
                float t = Mathf.Repeat(Time.time, 2 * Duration);
                if (t < Duration)
                {
                    // Min to max
                    camera.orthographicSize = Mathf.Lerp(MinCameraSize, MaxCameraSize, (t * 0.5f) / Duration);
                }
                else
                {
                    // Max to min
                    camera.orthographicSize = Mathf.Lerp(MaxCameraSize, MinCameraSize, (t * 0.5f) / Duration);
                }
                yield return null;
            }
        }
    }
}
