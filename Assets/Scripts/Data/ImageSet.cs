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
        public Texture2D shaded;
        public Matrix4x4 imageToWorld;
        public Vector2 size
        { 
            get 
            { 
                return new Vector2(albedo.width, albedo.height); 
            } 
        }
    }

    public class MultiViewImageSet
    {
        public Bounds bounds;
        public ImageSet[] imageSets;
        public Vector2 size
        {
            get
            {
                return imageSets[0].size;
            }
        }
    }
}

