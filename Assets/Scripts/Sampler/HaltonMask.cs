using System;
using System.Collections.Generic;
using UnityEngine;

namespace PCToolkit.Sampling
{
    public class HaltonMask
    {
        public List<Vector2Int> samplePoints { get; private set; }
        private Vector2 rect;
        private const float pointsUnit = 3000000f;
        private int perCamCount;

        private void GenerateSequence(int skip = 0)
        {
            samplePoints = new List<Vector2Int>();
            var haltonCount = perCamCount;
            var size = rect.x;
            if (rect.x != rect.y)
            {
                size = rect.x > rect.y ? rect.x : rect.y;
                haltonCount = (int)(perCamCount * size * size / rect.x / rect.y) + skip;
            }

            var haltonSeq = new HaltonSequence2D();
            for (int i = 0; i < haltonCount; i++)
            {
                haltonSeq.Increment();
                if (i >= skip)
                {
                    var pointf = haltonSeq.m_CurrentPos * size;
                    if (pointf.x < rect.x && pointf.y < rect.y)
                    {
                        samplePoints.Add(new Vector2Int(Mathf.RoundToInt(pointf.x), Mathf.RoundToInt(pointf.y)));
                    }
                }
            }
        }

        public HaltonMask(Vector3 volume, int cameraCount = 64, float width = 1024f, float height = 1024f, float density = 20f)
        {
            rect = new Vector2(width, height);
            var sumCount = volume.x * volume.y * volume.z * pointsUnit * density;
            perCamCount = (int)(sumCount / cameraCount);
            GenerateSequence();
        }

    }
}
