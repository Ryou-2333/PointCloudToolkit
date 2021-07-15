using UnityEngine;
using PCToolkit.Data;
using System.Collections.Generic;

namespace PCToolkit.Sampling
{
    public class ParameterSampler
    {
        private HaltonMask mask;
        private ImageSet imageSet;

        public ParameterSampler(ImageSet imageSet, Vector3 volume, int camCount)
        {
            this.imageSet = imageSet;
            mask = new HaltonMask(volume, camCount, imageSet.size.x, imageSet.size.y);
        }

        public static Color BilinearSample(Texture2D sampler, Vector2 pos)
        {
            var uv = pos / new Vector2(sampler.width, sampler.height);
            return sampler.GetPixelBilinear(uv.x, uv.y);
        }

        public static Color PointSample(Texture2D sampler, Vector2 pos)
        {
            return sampler.GetPixel(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }

        const float factor1 = 128f;
        const float factor2 = factor1 * factor1;
        const float factor3 = factor2 * factor1;
        private static float DecodeDepth(Color encodedDepth)
        {
            var depth = encodedDepth.r + encodedDepth.g / factor1 + encodedDepth.b / factor2 + encodedDepth.a / factor3;
            var res = 1 - depth * 2;
            return res;
        }

        public PointCloudData SamplePoints()
        {
            var points = new List<Point>();
            foreach (var sp in mask.samplePoints)
            {
                var encodedDepth = PointSample(imageSet.depth, sp);
                if (encodedDepth == Color.clear)
                {
                    //empty pixel
                    continue;
                }

                var depth = DecodeDepth(encodedDepth);
                var clipPos = new Vector4(sp.x / imageSet.size.x * 2f - 1f, sp.y / imageSet.size.y * 2f - 1f, depth);
                var worldPos = imageSet.imageToWorld.MultiplyPoint(clipPos);
                var p = new Point(worldPos);
                var param = BilinearSample(imageSet.parameters, sp);
                p.roughness = param.r;
                p.metallic = param.g;
                var albedo = BilinearSample(imageSet.albedo, sp);
                p.albedo = new PCTColor(albedo);
                var normalColor = BilinearSample(imageSet.normal, sp);
                var normal = new Vector3(normalColor.r, normalColor.g, normalColor.b);
                normal = normal * 2 - Vector3.one;
                var height = Vector3.Dot(normal, Vector3.up);
                p.height = height;
                points.Add(p);
            }

            PointCloudData data = new PointCloudData();
            data.pointCloudBuffer = points.ToArray();
            return data;
        }
    }
}

