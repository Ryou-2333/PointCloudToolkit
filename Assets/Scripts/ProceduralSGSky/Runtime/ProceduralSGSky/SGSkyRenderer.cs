using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEngine.Rendering.HighDefinition
{
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

        protected override bool Update(BuiltinSkyParameters builtinParams)
        {
            return needUpdate;
        }

        public override void Build()
        {
            var hdrp = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
            m_ProceduralSkyMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/HDRP/Sky/SGSky"));
            GenerateSGs();
        }

        public void GenerateSGs()
        {
            m_SGs = new SphereGuassians();
            needUpdate = true;
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_ProceduralSkyMaterial);
        }

        public override void RenderSky(BuiltinSkyParameters builtinParams, bool renderForCubemap, bool renderSunDisk)
        {
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
