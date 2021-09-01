using PCToolkit.Data;
using UnityEngine;

namespace PCToolkit.Rendering
{
    [ExecuteInEditMode]
    public class PointCloudRenderer : MonoBehaviour
    {
        [SerializeField] PointCloudRenderData renderData;
        [SerializeField] float pointSize = 0.05f;
        [SerializeField] Shader pointShader = null;
        [SerializeField] Shader diskDhader = null;

        [SerializeField] Material pointMaterial;
        [SerializeField] Material diskMaterial;

        public enum PointRenderMode
        {
            RawColor = 1,
            DetailedNormal = 2,
            Metallic = 4,
            Roughness = 8,
            Albedo = 16,
            Normal = 32,
        }

        public PointRenderMode renderMode;
        void OnValidate()
        {
            pointSize = Mathf.Max(0, pointSize);
        }

        private void OnDisable()
        {
            renderData.Refresh();
        }

        void OnDestroy()
        {
            if (pointMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(pointMaterial);
                }
                else
                {
                    DestroyImmediate(pointMaterial);
                }
            }
        }

        void OnRenderObject()
        {
            if (renderData == null || renderData.renderBuffer0 == null || renderData.renderBuffer1 == null || renderData.renderBuffer2 == null) return;

            var camera = Camera.current;
            if ((camera.cullingMask & (1 << gameObject.layer)) == 0) return;
            if (camera.name == "Preview Scene Camera") return;

            var renderBuffer0 = renderData.renderBuffer0;
            var renderBuffer1 = renderData.renderBuffer1;
            var renderBuffer2 = renderData.renderBuffer2;

            if (pointSize == 0)
            {
                if (pointMaterial == null)
                {
                    pointMaterial = new Material(pointShader);
                    pointMaterial.hideFlags = HideFlags.DontSave;
                }
                pointMaterial.SetInt("_RenderMod", (int)renderMode);
                pointMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                pointMaterial.SetBuffer("_PosRCBuffer", renderBuffer0);
                pointMaterial.SetBuffer("_MRDABuffer", renderBuffer1);
                pointMaterial.SetBuffer("_NormalBuffer", renderBuffer2);
#if UNITY_2019_1_OR_NEWER
                Graphics.DrawProceduralNow(MeshTopology.Points, renderBuffer0.count, 1);
#else
                Graphics.DrawProcedural(MeshTopology.Points, renderBuffer0.count, 1);
#endif
            }
            else
            {
                if (diskMaterial == null)
                {
                    diskMaterial = new Material(diskDhader);
                    diskMaterial.hideFlags = HideFlags.DontSave;
                }
                diskMaterial.SetInt("_RenderMod", (int)renderMode);
                diskMaterial.SetPass(0);
                diskMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                diskMaterial.SetBuffer("_PosRCBuffer", renderBuffer0);
                diskMaterial.SetBuffer("_MRDABuffer", renderBuffer1);
                diskMaterial.SetBuffer("_NormalBuffer", renderBuffer2);
                diskMaterial.SetFloat("_PointSize", pointSize);
#if UNITY_2019_1_OR_NEWER
                Graphics.DrawProceduralNow(MeshTopology.Points, renderBuffer0.count, 1);
#else
                Graphics.DrawProcedural(MeshTopology.Points, renderBuffer.count, 1);
#endif
            }
        }
    }
}
