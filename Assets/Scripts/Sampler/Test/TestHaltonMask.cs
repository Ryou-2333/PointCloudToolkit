using UnityEngine;

namespace PCToolkit.Test
{
    public class TestHaltonMask : MonoBehaviour
    {
        Sampling.HaltonMask mask;
        void Start()
        {
            var cam = Camera.main;
            mask = new Sampling.HaltonMask(Vector3.one * 2, 56, 1024, 768);
            for (int i = 0; i < mask.samplePoints.Count; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = cam.ScreenToWorldPoint(new Vector3(mask.samplePoints[i].x, mask.samplePoints[i].y, 10f));
                sphere.transform.localScale = Vector3.one * 0.02f;
            }
        }
    }
}

