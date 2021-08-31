using UnityEngine;

namespace PCToolkit.Data
{
    public class CaptureData
    {
        public int camIndex;
        public Texture2D texture;
        public Matrix4x4 imageToWorld;
        public Matrix4x4 worldToImage;
    }
}
