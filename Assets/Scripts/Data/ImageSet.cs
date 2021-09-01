using System;
using UnityEngine;

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
        public Matrix4x4 imageToWorld;
        public Matrix4x4 worldToImage;
        public Vector2Int rect
        { 
            get 
            { 
                return new Vector2Int(albedo.width, albedo.height); 
            } 
        }
    }

    public class MultiViewImageSet
    {
        public string fileName;
        public Bounds bounds;
        public ImageSet[] imageSets;
        public int length { get { return imageSets.Length; } }
        public ImageSet this[int key]
        {

            get
            {
                if (key < length)
                {
                    return imageSets[key];
                }

                throw new Exception("MultiViewImageSet out of index.");
            }
        }
        public Vector2Int rect
        {
            get
            {
                return imageSets[0].rect;
            }
        }
    }
}

