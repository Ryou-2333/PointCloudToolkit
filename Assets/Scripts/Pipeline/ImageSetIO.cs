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

    public class ImageSetIO
    {
        const string datasetDir = "../../Datasets";
        const string imageSetDir = "ImageSets";
        const string mvcFilename = "mvc.config";
        const string lightingFileNameFormatter = "lighmting_{0}.config";

        public static void SaveImageSet(MultiViewImageSet mvis)
        {
            var dir = string.Format("{0}/{1}/{2}/{3}", Application.dataPath, datasetDir, imageSetDir, mvis.fileName);
            Directory.CreateDirectory(dir);
            var mvcInfo = new MVCInfo();
            mvcInfo.bounds = mvis.bounds;
            mvcInfo.rect = mvis.rect;
            for (int i = 0; i < mvis.count; i++)
            {
                mvcInfo.imageToWorlds.Add(mvis[i].imageToWorld);
                mvcInfo.worldToImages.Add(mvis[i].worldToImage);
            }

            File.WriteAllText(dir + "/" + mvcFilename, JsonUtility.ToJson(mvcInfo));
            File.WriteAllText(string.Format(dir + "/" + lightingFileNameFormatter, mvis.variantName), JsonUtility.ToJson(mvis.lightingParams));
            for (int i = 0; i < mvis.count; i++)
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

                if (mvis[i].normal != null)
                {
                    var bytes = mvis[i].normal.EncodeToPNG();
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Normal.ToString());
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }

                if (mvis[i].detail != null)
                {
                    var bytes = mvis[i].detail.EncodeToPNG();
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Detail.ToString());
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
                    string filename;
                    if (string.IsNullOrEmpty(mvis.variantName))
                    {
                        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Shaded.ToString());
                    }
                    else
                    {
                        filename = string.Format("{0}_{1}_{2}.png", i, MeshRenderMode.Shaded.ToString(), mvis.variantName);
                    }
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }

                if (mvis[i].onlyLighting != null)
                {
                    var bytes = mvis[i].onlyLighting.EncodeToPNG();
                    string filename;
                    if (string.IsNullOrEmpty(mvis.variantName))
                    {
                        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.OnlyLighting.ToString());
                    }
                    else
                    {
                        filename = string.Format("{0}_{1}_{2}.png", i, MeshRenderMode.OnlyLighting.ToString(), mvis.variantName);
                    }
                    File.WriteAllBytes(string.Format("{0}/{1}", dir, filename), bytes);
                }
            }
        }

        public static MultiViewImageSet LoadImageSet(string dirName, string variant)
        {
            var dir = string.Format("{0}/{1}/{2}/{3}", Application.dataPath, datasetDir, imageSetDir, dirName);
            try
            {
                var text = File.ReadAllText(dir + "/" + mvcFilename);
                var mvcInfo = JsonUtility.FromJson<MVCInfo>(text);
                var mvis = new MultiViewImageSet();
                mvis.bounds = mvcInfo.bounds;
                mvis.imageSets = new List<ImageSet>();
                mvis.variantName = variant;
                // iterate camera index
                for (int i = 0; i < mvcInfo.imageToWorlds.Count; i++)
                {
                    var imageSet = new ImageSet();
                    imageSet.imageToWorld = mvcInfo.imageToWorlds[i];
                    imageSet.worldToImage = mvcInfo.worldToImages[i];
                    //depth
                    var filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Depth.ToString());
                    var pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D depthTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, false);
                    depthTex.LoadImage(pngBytes);
                    imageSet.depth = depthTex;
                    //albedo
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Albedo.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D albedoTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, false);
                    albedoTex.LoadImage(pngBytes);
                    imageSet.albedo = albedoTex;
                    //params
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Parameter.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D paramTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, false);
                    paramTex.LoadImage(pngBytes);
                    imageSet.parameters = paramTex;
                    //normal
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Normal.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D norTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, false);
                    norTex.LoadImage(pngBytes);
                    imageSet.normal = norTex;
                    //normal
                    filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Detail.ToString());
                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D detailTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, false);
                    detailTex.LoadImage(pngBytes);
                    imageSet.detail = detailTex;
                    //shaded
                    if (string.IsNullOrEmpty(variant))
                    {
                        filename = string.Format("{0}_{1}.png", i, MeshRenderMode.Shaded.ToString());
                    }
                    else
                    {
                        filename = string.Format("{0}_{1}_{2}.png", i, MeshRenderMode.Shaded.ToString(), variant);
                    }

                    pngBytes = File.ReadAllBytes(string.Format("{0}/{1}", dir, filename));
                    Texture2D rawTex = new Texture2D(mvcInfo.rect.x, mvcInfo.rect.y, TextureFormat.RGBAFloat, false, false);
                    rawTex.LoadImage(pngBytes);
                    imageSet.shaded = rawTex;

                    mvis.imageSets.Add(imageSet);
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
