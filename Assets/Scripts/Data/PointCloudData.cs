using UnityEngine;

namespace PCToolkit.Data
{
    [CreateAssetMenu(fileName = "PointCloudData", menuName = "PCTK/PointCloudData", order = 1)]
    [PreferBinarySerialization]
    public class PointCloudData : ScriptableObject
    {
        public int pointCount { get { return pointCloudBuffer.Length; } }
        public Point[] pointCloudBuffer;
    }
}

