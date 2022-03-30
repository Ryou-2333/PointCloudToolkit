using System;
using UnityEngine;
using PCToolkit.Rendering;
using System.Collections.Generic;

namespace PCToolkit.Data
{
    public class ImageSet
    {
        public Texture2D depth;
        public Texture2D albedo;
        public Texture2D parameters;
        public Texture2D normal;
        public Texture2D detail;
        public Texture2D shaded;
        public Texture2D onlyLighting;
        public Matrix4x4 imageToWorld;
        public Matrix4x4 worldToImage;
        public Vector2Int rect
        { 
            get 
            {
                if (depth != null)
                {
                    return new Vector2Int(depth.width, depth.height);
                }
                else if (shaded != null)
                {
                    return new Vector2Int(shaded.width, shaded.height);
                }
                else
                {
                    return new Vector2Int(onlyLighting.width, onlyLighting.height);
                }
            } 
        }

        public void SetTexture(MeshRenderMode mode, Texture2D tex)
        {
            switch (mode)
            {
                case MeshRenderMode.Shaded:
                    shaded = tex;
                    return;
                case MeshRenderMode.Albedo:
                    albedo = tex;
                    return;
                case MeshRenderMode.Parameter:
                    parameters = tex;
                    return;
                case MeshRenderMode.Normal:
                    normal = tex;
                    return;
                case MeshRenderMode.Detail:
                    detail = tex;
                    return;
                case MeshRenderMode.Depth:
                    depth = tex;
                    return;
                case MeshRenderMode.OnlyLighting:
                    onlyLighting = tex;
                    return;
                default:
                    throw new Exception("ImageSet RenderMode out of index.");
            }
        }

        public void Dispose()
        {
            if (depth != null)
            {
                UnityEngine.Object.DestroyImmediate(depth);
            }
            if (albedo != null)
            {
                UnityEngine.Object.DestroyImmediate(albedo);
            }
            if (parameters != null)
            {
                UnityEngine.Object.DestroyImmediate(parameters);
            }
            if (normal != null)
            {
                UnityEngine.Object.DestroyImmediate(normal);
            }
            if (detail != null)
            {
                UnityEngine.Object.DestroyImmediate(detail);
            }
            if (shaded != null)
            {
                UnityEngine.Object.DestroyImmediate(shaded);
            }
            if (onlyLighting != null)
            {
                UnityEngine.Object.DestroyImmediate(onlyLighting);
            }
        }
    }

    public class MultiViewImageSet
    {
        public string fileName;
        public string variantName;
        public Bounds bounds;
        public List<ImageSet> imageSets;
        public int count { get { return imageSets.Count; } }
        public Vector2Int rect
        {
            get
            {
                return imageSets[0].rect;
            }
        }
        public LightingParams lightingParams;

        public ImageSet this[int key]
        {

            get
            {
                if (key < count)
                {
                    return imageSets[key];
                }

                throw new Exception("MultiViewImageSet out of index.");
            }
        }

        public void Dispose()
        {
            foreach (var set in imageSets)
            {
                set.Dispose();
            }
        }
    }
}

