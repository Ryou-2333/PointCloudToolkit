using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEngine.Rendering.HighDefinition
{
    class SGSkyContext
    {
        #region Singleton Pattern
        public static SGSkyContext Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly SGSkyContext instance = new SGSkyContext();
        }
        #endregion
        public SphereGuassians SGs = null;
    }

    class SGSkyRenderer : SkyRenderer
    {
        Material m_ProceduralSkyMaterial;
        MaterialPropertyBlock m_PropertyBlock = new MaterialPropertyBlock();
        SphereGuassians m_SGs = null;
        bool needUpdate = false;

        readonly int _PixelCoordToViewDirWS = Shader.PropertyToID("_PixelCoordToViewDirWS");
        readonly int _SGLength = Shader.PropertyToID("_SGLength");
        readonly int _DirArray = Shader.PropertyToID("_DirArray");
        readonly int _FeatureArray = Shader.PropertyToID("_FeatureArray");

        public SGSkyRenderer()
        {
        }

        ~SGSkyRenderer()
        {
        }

        public void UpdateParameters(SphereGuassians SGs)
        {
            if(m_SGs != SGs)
            m_SGs = SGs;
            needUpdate = true;
        }

        protected override bool Update(BuiltinSkyParameters builtinParams)
        {
            return needUpdate;
        }

        public override void Build()
        {
            var hdrp = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
            m_ProceduralSkyMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/HDRP/Sky/SGSky"));
            if (SGSkyContext.Instance.SGs != null) UpdateParameters(SGSkyContext.Instance.SGs);
            else UpdateParameters(new SphereGuassians());
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_ProceduralSkyMaterial);
        }

        public override void RenderSky(BuiltinSkyParameters builtinParams, bool renderForCubemap, bool renderSunDisk)
        {
            if (SGSkyContext.Instance.SGs != null) UpdateParameters(SGSkyContext.Instance.SGs);
            SGSky skySettings = builtinParams.skySettings as SGSky;
            m_PropertyBlock.SetMatrix(_PixelCoordToViewDirWS, builtinParams.pixelCoordToViewDirMatrix);
            if (needUpdate)
            {
                m_PropertyBlock.SetInt(_SGLength, SphereGuassians.length);
                m_PropertyBlock.SetVectorArray(_DirArray, m_SGs.directions);
                m_PropertyBlock.SetVectorArray(_FeatureArray, m_SGs.features);
                needUpdate = false;
            }

            CoreUtils.DrawFullScreen(builtinParams.commandBuffer, m_ProceduralSkyMaterial, m_PropertyBlock, renderForCubemap ? 0 : 1);
        }
    }
}
