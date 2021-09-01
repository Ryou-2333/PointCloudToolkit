using System.IO;
using System.Collections;
using UnityEngine;
using UnityEditor;
using PCToolkit.Rendering;
using PCToolkit.Sampling;
using PCToolkit.Data;

namespace PCToolkit.Pipeline
{
    public class TestIO : MonoBehaviour
    {
        public MultiViewCamera mvc;
        public TargetRenderer target;

        //public IEnumerator GeneratePointCloud()
        //{
        //    mvc.SpawnCameras(target);
        //    var mvis = new MultiViewImageSet();
        //    yield return mvc.CaptureParameters(target, mvis, 1);
        //    var sampler = new PointCloudSampler(mvis);
        //    var pointCloud = sampler.SamplePoints();
        //    var pcRender = new PointCloudRenderData();
        //    pcRender.pointCloudBuffer = pointCloud.pointCloudBuffer;
        //    AssetDatabase.CreateAsset(pcRender, string.Format("Assets/Datasets/PointClouds/{0}_render.asset", target.name));
        //}

        //public IEnumerator GenerateParamImages()
        //{
        //    mvc.SpawnCameras(target);
        //    var imageSet = new MultiViewImageSet();
        //    yield return mvc.CaptureParameters(target, imageSet, 1);

        //    var dir = string.Format("{0}/Datasets/ImageSets/{1}", Application.dataPath, target.name);
        //    Directory.CreateDirectory(dir);
        //    for (int i = 0; i < imageSet.imageSets.Length; i++)
        //    {
        //        byte[] bytes = imageSet.imageSets[i].depth.EncodeToPNG();
        //        var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Depth.ToString());
        //        File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);

        //        bytes = imageSet.imageSets[i].albedo.EncodeToPNG();
        //        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Albedo.ToString());
        //        File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);

        //        bytes = imageSet.imageSets[i].parameters.EncodeToPNG();
        //        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Parameter.ToString());
        //        File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);

        //        bytes = imageSet.imageSets[i].normal.EncodeToPNG();
        //        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Normal.ToString());
        //        File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);

        //        bytes = imageSet.imageSets[i].detail.EncodeToPNG();
        //        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Detail.ToString());
        //        File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
        //    }
        //}

        //public IEnumerator GenerateRawImages()
        //{
        //    mvc.SpawnCameras(target);
        //    var imageSet = new MultiViewImageSet();
        //    yield return mvc.CaptureRaw(target, imageSet);
        //    var dir = string.Format("{0}/Datasets/ImageSets/{1}", Application.dataPath, target.name);
        //    Directory.CreateDirectory(dir);
        //    for (int i = 0; i < imageSet.imageSets.Length; i++)
        //    {
        //        byte[] bytes = imageSet.imageSets[i].shaded.EncodeToPNG();
        //        var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Shaded.ToString());
        //        File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
        //    }
        //}

        //public void Start()
        //{
        //    StartCoroutine(GeneratePointCloud());
        //}
    }
}

