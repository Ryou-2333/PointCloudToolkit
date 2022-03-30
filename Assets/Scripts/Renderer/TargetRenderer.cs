using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PCToolkit.Rendering
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TargetRenderer))]
    public class TargetRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            TargetRenderer renderer = (TargetRenderer)target;
            if (GUILayout.Button("Show Bounds"))
            {
                ShowBounds(renderer);
            }
        }

        public void ShowBounds(TargetRenderer renderer)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = renderer.bounds.size;
            cube.transform.position = renderer.bounds.center;
        }
    }

#endif

    public enum MeshRenderMode
    {
        Shaded = 0,
        Albedo = 1,
        Parameter = 2,
        Normal = 4,
        Detail = 8,
        //Last
        Depth = 16,
        OnlyLighting = 17,
    }
    public class TargetRenderer : MonoBehaviour
    {
        public MeshRenderer[] meshes;
        public Material depthMaterial;
        public Material shadingMaterial;
        public Material paramMaterial;
        public Material onlyLightingMaterial;
        public Bounds bounds;
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
                else if (value == MeshRenderMode.OnlyLighting)
                {
                    foreach (var mesh in meshes)
                    {
                        mesh.material = onlyLightingMaterial;
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


    }
}

