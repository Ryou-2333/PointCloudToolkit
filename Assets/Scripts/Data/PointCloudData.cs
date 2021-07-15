using UnityEngine;

namespace PCToolkit.Data
{
    [CreateAssetMenu(fileName = "PointCloudData", menuName = "PointCloud/PointCloudData", order = 1)]
    public class PointCloudData : ScriptableObject
    {
        public int pointCount { get { return pointCloudBuffer.Length; } }
        public Point[] pointCloudBuffer;
    }
}

