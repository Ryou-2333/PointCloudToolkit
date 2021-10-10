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
        public MeshRenderer[] meshes;
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
                    foreach (var mesh in meshes)
                    {
                        mesh.material = shadingMaterial;
                    }
                }
                else if (value == MeshRenderMode.Depth)
                {
                    foreach (var mesh in meshes)
                    {
                        mesh.material = depthMaterial;
                    }
                }
                else
                {
                    foreach (var mesh in meshes)
                    {
                        paramMaterial.SetInt("_RenderMode", (int)value);
                        mesh.material = paramMaterial;
                    }
                }
            }
        }
        public Bounds bounds
        {
            get
            {
                Vector3 min = meshes[0].bounds.min;
                Vector3 max = meshes[0].bounds.max;
                foreach (var mesh in meshes)
                {
                    min = Vector3.Min(min, mesh.bounds.min);
                    max = Vector3.Max(max, mesh.bounds.max);
                }
                var bounds = new Bounds();
                bounds.SetMinMax(min, max);
                return bounds;
            }
        }
    }
}

