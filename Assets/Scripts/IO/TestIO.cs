using System.IO;
using UnityEngine;
using UnityEditor;
using PCToolkit.Rendering;
using PCToolkit.Sampling;
using PCToolkit.Data;

namespace PCToolkit.IO
{
    public class TestIO : MonoBehaviour
    {
        public MultiViewCamera mvc;
        public TargetRenderer target;

        public void GeneratePointCloud()
        {
            mvc.SpawnCameras(target);
            var mvis = mvc.CaptureModes(target, 1);
            var sampler = new ParameterSampler(mvis.imageSets[0], mvis.volume, 1);
            var pointCloud = sampler.SamplePoints();
            var pcRender = new PointCloudRenderData();
            pcRender.pointCloudBuffer = pointCloud.pointCloudBuffer;
            AssetDatabase.CreateAsset(pcRender, string.Format("Assets/PointClouds/{0}_render.asset", target.name));
        }

        public void GenerateImages()
        {
            mvc.SpawnCameras(target);
            var imageSet = mvc.CaptureModes(target);

            var dir = string.Format("{0}/Sampled/{1}", Application.streamingAssetsPath, target.name);
            Directory.CreateDirectory(dir);
            for (int i = 0; i < imageSet.imageSets.Length; i++)
            {
                byte[] bytes = imageSet.imageSets[i].albedo.EncodeToTGA();
                var filename = string.Format("{0}_{1}.tga", i, MeshRenderMode.Albedo.ToString());
                File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);

                bytes = imageSet.imageSets[i].parameters.EncodeToTGA();
                filename = string.Format("{0}_{1}.tga", i, MeshRenderMode.Parameter.ToString());
                File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);

                bytes = imageSet.imageSets[i].normal.EncodeToTGA();
                filename = string.Format("{0}_{1}.tga", i, MeshRenderMode.Normal.ToString());
                File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
            }
        }

        public void Start()
        {
            GeneratePointCloud();
        }
    }
}

