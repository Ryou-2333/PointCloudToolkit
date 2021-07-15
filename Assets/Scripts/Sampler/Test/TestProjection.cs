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
    }
}
