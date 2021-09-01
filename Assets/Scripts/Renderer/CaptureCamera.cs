using UnityEngine;
using PCToolkit.Data;

namespace PCToolkit.Rendering
{
    [RequireComponent(typeof(Camera))]
    public class CaptureCamera : MonoBehaviour
    {
        private static int camCounter;
        private int index;
        public int sampleCount = 1;
        [SerializeField] RenderTexture colorRT;
        [SerializeField] RenderTexture depthRT;
        private CaptureCamera()
        {
            index = camCounter;
            camCounter++;
        }

        private Camera _cam;
        private Camera cam
        {
            get
            {
                if (_cam == null)
                {
                    _cam = GetComponent<Camera>();
                }
                return _cam;
            }
        }

        public void PreCpatureRaw()
        {
            PreCaptureColor();
            if (sampleCount > 1)
            {
                for (int i = 0; i < sampleCount - 1; i++)
                {
                    cam.Render();
                }
            }
        }

        public void PreCaptureColor()
        {
            //cam.targetTexture = colorRT;
            gameObject.SetActive(true);
        }

        public void PreCaptureDepth()
        {
            //cam.targetTexture = depthRT;
            gameObject.SetActive(true);
        }

        public CaptureData Capture()
        {
            var data = new CaptureData();
            //RenderTexture.active = cam.targetTexture;
            //var targetTexture = RenderTexture.active;
            data.texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBAFloat, false, true);
            data.texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            data.texture.Apply();
            RenderTexture.active = null;
            data.worldToImage = cam.projectionMatrix * cam.worldToCameraMatrix;
            data.imageToWorld = (cam.projectionMatrix * cam.worldToCameraMatrix).inverse; ;
            data.camIndex = index;
            gameObject.SetActive(false);
            return data;
        }
    }
}