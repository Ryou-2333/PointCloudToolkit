using UnityEngine;

namespace PCToolkit.Rendering
{

    public enum MeshRenderMode
    {
        Shaded = 0,
        Albedo = 1,
        Parameter = 2,
        Normal = 4,
        Detail = 8,
        //Last
        Depth = 16
    }
    public class TargetRenderer : MonoBehaviour
    {
        public MeshRenderer mesh;
        public Material depthMaterial;
        public Material shadingMaterial;
        public Material paramMaterial;
        public MeshRenderMode _renderMode;
        public MeshRenderMode renderMode
        {
            get
            {
                return _renderMode;
            }
            set 
            {
                _renderMode = value;
                if (value == MeshRenderMode.Shaded)
                {
                    mesh.material = shadingMaterial;
                }
                else if (value == MeshRenderMode.Depth)
                {
                    mesh.material = depthMaterial;
                }
                else
                {
                    paramMaterial.SetInt("_RenderMode", (int)value);
                    mesh.material = paramMaterial;
                }
            }
        }
        public Bounds bounds
        {
            get
            {
                return mesh.bounds;
            }
        }
        public float maxBound
        {
            get
            {
                return Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            }
        }
    }
}

