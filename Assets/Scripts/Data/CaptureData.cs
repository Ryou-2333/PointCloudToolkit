using System;
using UnityEngine;

namespace PCToolkit.Data
{
    [Serializable]
    public class CaptureData
    {
        public int camIndex;
        public Texture2D texture;
        public Matrix4x4 imageToWorld;
    }
}
