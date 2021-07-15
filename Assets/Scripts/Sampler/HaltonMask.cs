using System;
using System.Collections.Generic;
using UnityEngine;

namespace PCToolkit.Sampling
{
    [Serializable]
    public class HaltonMask
    {
        public List<Vector2> samplePoints { get; private set; }
        private Vector2 rect;
        private float density;
        private int cameraCount;
        private const float pointsUnit = 3000000f;
        private int perCamCount;

        private void GenerateSequence()
        {
            samplePoints = new List<Vector2>();
            var haltonCount = perCamCount;
            var size = rect.x;
            if (rect.x != rect.y)
            {
                size = rect.x > rect.y ? rect.x : rect.y;
                perCamCount = (int)(perCamCount * size * size / rect.x / rect.y);
            }

            var haltonSeq = new HaltonSequence2D();
            for (int i = 0; i < perCamCount; i++)
            {
                haltonSeq.Increment();
                var pointf = haltonSeq.m_CurrentPos * size;
                if (pointf.x < rect.x && pointf.y < rect.y)
                {
                    samplePoints.Add(pointf);
                }
            }
        }

        public HaltonMask(Vector3 volume, int cameraCount = 64, float width = 1024f, float height = 1024f, float density = 5f)
        {
            rect = new Vector2(width, height);
            this.density = density;
            var sumCount = volume.x * volume.y * volume.z * pointsUnit * density;
            perCamCount = (int)(sumCount / cameraCount);
            GenerateSequence();
        }

    }
}
