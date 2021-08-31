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

        public CaptureData CaptureColor()
        {
            cam.targetTexture = colorRT;
            var data = new CaptureData();
            data.texture = new Texture2D(depthRT.width, depthRT.height, TextureFormat.RGBAFloat, false, true);
            Capture(data);
            return data;
        }

        public CaptureData CaptureDepth()
        {
            cam.targetTexture = depthRT;
            var data = new CaptureData();
            data.texture = new Texture2D(depthRT.width, depthRT.height, TextureFormat.RGBAFloat, false, true);
            Capture(data);
            return data;
        }

        private void Capture(CaptureData data)
        {
            gameObject.SetActive(true);
            var targetTexture = cam.targetTexture;
            RenderTexture.active = targetTexture;
            if (sampleCount > 1)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    cam.Render();
                }
            }
            else
            {
                cam.Render();
            }

            data.texture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            data.texture.Apply();
            RenderTexture.active = null;
            data.worldToImage = cam.projectionMatrix * cam.worldToCameraMatrix;
            data.imageToWorld = data.worldToImage.inverse;
            data.camIndex = index;
            gameObject.SetActive(false);
        }
    }
}