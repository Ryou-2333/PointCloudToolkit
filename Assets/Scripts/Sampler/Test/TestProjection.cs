using UnityEditor;
using UnityEngine;

namespace PCToolkit.Test
{
    public class TestProjection : MonoBehaviour
    {
        public Camera cam;
        public Matrix4x4 worldToClip;
        public Matrix4x4 clipToWorld;
        void Awake()
        {
            worldToClip = cam.projectionMatrix * cam.worldToCameraMatrix;
            clipToWorld = worldToClip.inverse;
        }

        private void Start()
        {
            var worldPosNear = new Vector3(0, 0, -9.7f);
            var worldPosFar = new Vector3(0, 0, 10f);
            Vector3 clipPosNear = worldToClip.MultiplyPoint(worldPosNear);
            Vector3 clipPosFar = worldToClip.MultiplyPoint(worldPosFar);
            Debug.Log(string.Format("Clip pos near: {0}\nClip pos far: {1}", clipPosNear, clipPosFar));
        }

        [MenuItem("PCToolkit/Test Encoding")]
        static void TestEncoding()
        {
            float depth = 0.912345678912f;
            Debug.Log(depth);
            int factor1 = 128;
            int factor2 = factor1 * factor1;
            int factor3 = factor2 * factor1;
            int factor4 = factor3 * factor1;
            float depth0 = (float)Mathf.FloorToInt(depth * factor1) / factor1;
            float depth1 = (float)(Mathf.FloorToInt(depth * factor2) - depth0 * factor2) / factor1;
            float depth2 = (float)(Mathf.FloorToInt(depth * factor3) - depth0 * factor3 - depth1 * factor2) / factor1;
            float depth3 = (float)(Mathf.FloorToInt(depth * factor4) - depth0 * factor4 - depth1 * factor3 - depth2 * factor2) / factor1;
            Debug.Log(string.Format("{0}, {1}, {2}, {3}", depth0, depth1, depth2, depth3));
            depth = depth0 + depth1 / factor1 + depth2 / factor2 + depth3 / factor3;
            Debug.Log(depth);
        }
    }
}
