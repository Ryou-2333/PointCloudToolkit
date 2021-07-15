using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCToolkit.Rendering
{
    [RequireComponent(typeof(Camera))]
    public class DepthCamera : MonoBehaviour
    {
        private void Awake()
        {
            var cam = GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.Depth;
        }
    }
}

