using UnityEngine;

namespace PCToolkit.Rendering
{

    public enum MeshRenderMode
    {
        Shaded = 0,
        Albedo = 1,
        Parameter = 2,
        Normal = 4,
        Depth = 8
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
        public Vector3 size
        {
            get
            {
                return mesh.bounds.size;
            }
        }
        public float maxBound
        {
            get
            {
                return Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            }
        }
        public Vector3 center
        {
            get
            {
                return transform.position + Vector3.up * size.y / 2f;
            }
        }
    }
}

