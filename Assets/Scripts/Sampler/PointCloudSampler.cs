using UnityEngine;
using PCToolkit.Data;
using System.Collections.Generic;

namespace PCToolkit.Sampling
{
    public class PointCloudSampler
    {
        private HaltonMask mask;
        private MultiViewImageSet imageSets;

        public PointCloudSampler(MultiViewImageSet imageSets)
        {
            this.imageSets = imageSets;
            mask = new HaltonMask(imageSets.bounds.size, imageSets.imageSets.Length, imageSets.rect.x, imageSets.rect.y, 100f);
        }

        public static Color PointSample(Texture2D sampler, Vector2Int pos)
        {
            return sampler.GetPixel(pos.x, pos.y);
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
            var visMasks = new List<VisibilityMask>();
            // Sample point positions and parameters
            for (int i = 0; i < imageSets.length; i++)
            {
                foreach (var sp in mask.samplePoints)
                {
                    var encodedDepth = PointSample(imageSets[i].depth, sp);
                    var depth = DecodeDepth(encodedDepth);
                    var imgPos = new Vector3(sp.x / (float)imageSets[i].rect.x * 2f - 1f, sp.y / (float)imageSets[i].rect.y * 2f - 1f, depth);
                    var worldPos = imageSets[i].imageToWorld.MultiplyPoint(imgPos);
                    if (!imageSets.bounds.Contains(worldPos))
                    {
                        //out of bounds.
                        continue;
                    }

                    var p = new Point(worldPos);
                    var param = PointSample(imageSets[i].parameters, sp);
                    p.roughness = param.r;
                    p.metallic = param.g;
                    var normalColor = PointSample(imageSets[i].normal, sp);
                    var normal = new Vector3(normalColor.r, normalColor.g, normalColor.b);
                    normal = normal * 2 - Vector3.one;
                    p.normal = normal;
                    var detailColor = PointSample(imageSets[i].detail, sp);
                    var detailedNormal = new Vector3(detailColor.r, detailColor.g, detailColor.b);
                    p.detailedNormal = detailedNormal;
                    var albedo = PointSample(imageSets[i].albedo, sp);
                    p.albedo = new PCTColor(albedo);
                    var visMask = new VisibilityMask(imageSets.length);
                    visMask[i] = true;
                    points.Add(p);
                    visMasks.Add(visMask);
                }
            }

            //// Calculate visibilities
            for (int i = 0; i < imageSets.length; i++)
            {
                var curIndex = 0;
                foreach (var p in points)
                {
                    if (!visMasks[curIndex][i])
                    {
                        var imgPos = imageSets[i].worldToImage.MultiplyPoint(p.position);
                        var sp = new Vector2Int(Mathf.RoundToInt((imgPos.x + 1f) / 2f * imageSets[i].rect.x),
                            Mathf.RoundToInt((imgPos.y + 1f) / 2f * imageSets[i].rect.y));
                        var curDepth = DecodeDepth(PointSample(imageSets[i].depth, sp));
                        // Visibility check
                        if (Mathf.Abs(curDepth - imgPos.z) <= 0.001f)
                        {
                            visMasks[curIndex][i] = true;
                        }
                    }
                    curIndex++;
                }
            }

            //// Sample raw colors
            for (int i = 0; i < points.Count; i++)
            {
                var weight = 1f / visMasks[i].weight;
                Vector3 colorValues = Vector3.zero;
                for (int j = 0; j < imageSets.length; j++)
                {
                    if (visMasks[i][j])
                    {
                        var imgPos = imageSets[j].worldToImage.MultiplyPoint(points[i].position);
                        var sp = new Vector2Int(Mathf.RoundToInt((imgPos.x + 1f) / 2f * imageSets[j].rect.x),
                            Mathf.RoundToInt((imgPos.y + 1f) / 2f * imageSets[j].rect.y));
                        var rawColor = PointSample(imageSets[i].shaded, sp);
                        colorValues += weight * new Vector3(rawColor.r, rawColor.g, rawColor.b);
                    }
                }

                points[i].rawColor = new PCTColor(colorValues.x, colorValues.y, colorValues.z);
            }

            PointCloudData data = new PointCloudData();
            data.pointCloudBuffer = points.ToArray();
            return data;
        }
    }
}

