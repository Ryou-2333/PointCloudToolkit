using System.IO;
using UnityEngine;
using PCToolkit.Data;
using PCToolkit.Rendering;
using System;
using System.Collections.Generic;

namespace PCToolkit.Pipeline
{
    [Serializable]
    public class MVCInfo
    {
        public Bounds bounds;
        public Vector2Int rect;
        public List<Matrix4x4> imageToWorlds = new List<Matrix4x4>();
        public List<Matrix4x4> worldToImages = new List<Matrix4x4>();
    }

    public class ImageSetIO : MonoBehaviour
    {
        const string datasetDir = "Datasets";
        const string imageSetDir = "ImageSets";
        const string pointCloudDir = "PointClouds";
        const string mvcFilename = "mvc.config";

        public static void SaveImageSet(MultiViewImageSet mvis)
        {
            var dir = string.Format("{0}/{1}/{2}/{3}", Application.dataPath, datasetDir, imageSetDir, mvis.fileName);
            Directory.CreateDirectory(dir);
            if (!File.Exists(dir + "/" + mvcFilename))
            {
                var mvcInfo = new MVCInfo();
                mvcInfo.bounds = mvis.bounds;
                mvcInfo.rect = mvis.rect;
                for (int i = 0; i < mvis.length; i++)
                {
                    mvcInfo.imageToWorlds.Add(mvis[i].imageToWorld);
                    mvcInfo.worldToImages.Add(mvis[i].worldToImage);
                }

                File.WriteAllText(dir + "/" + mvcFilename, JsonUtility.ToJson(mvcInfo));
            }

            for (int i = 0; i < mvis.length; i++)
            {


                if (mvis[i].albedo != null)
                {
                    var bytes = mvis[i].albedo.EncodeToPNG();
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Albedo.ToString());
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }

                if (mvis[i].parameters != null)
                {
                    var bytes = mvis[i].parameters.EncodeToPNG();
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Parameter.ToString());
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }

                if (mvis[i].depth != null)
                {
                    var bytes = mvis[i].depth.EncodeToPNG();
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Depth.ToString());
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }

                if (mvis[i].shaded != null)
                {
                    var bytes = mvis[i].shaded.EncodeToPNG();
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Shaded.ToString());
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }
            }
        }

        public static MultiViewImageSet LoadImageSet(string dirName)
        {
            var dir = string.Format("{0}/{1}/{2}/{3}", Application.dataPath, datasetDir, imageSetDir, dirName);
            try
            {
                var text = File.ReadAllText(dir + "/" + mvcFilename);
                var mvcInfo = JsonUtility.FromJson<MVCInfo>(text);
                var mvis = new MultiViewImageSet();
                mvis.bounds = mvcInfo.bounds;
                // iterate camera index
                for (int i = 0; i < mvcInfo.imageToWorlds.Count; i++)
                {
                    var imageSet = new ImageSet();
                    imageSet.imageToWorld = mvcInfo.imageToWorlds[i];
                    imageSet.worldToImage = mvcInfo.worldToImages[i];
                    //depth
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Depth.ToString());
                    var pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D depthTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, true);
                    depthTex.LoadImage(pngBytes);
                    imageSet.depth = depthTex;
                    //albedo
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Albedo.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D albedoTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, true);
                    albedoTex.LoadImage(pngBytes);
                    imageSet.albedo = albedoTex;
                    //params
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Parameter.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D paramTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, true);
                    paramTex.LoadImage(pngBytes);
                    imageSet.parameters = paramTex;
                    //normal
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Normal.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D norTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, true);
                    norTex.LoadImage(pngBytes);
                    imageSet.normal = norTex;
                    //normal
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Detail.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D detailTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, true);
                    detailTex.LoadImage(pngBytes);
                    imageSet.detail = detailTex;
                    //shaded
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Shaded.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D rawTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, true);
                    rawTex.LoadImage(pngBytes);
                    imageSet.shaded = rawTex;
                }

                return mvis;
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading image set at " + dir + " : " + e.Message + "\n" + e.StackTrace);
                return null;
            }
        }
}

}
