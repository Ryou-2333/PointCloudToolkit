using UnityEngine;
using PCToolkit.Data;
using System.Collections.Generic;
using System;

namespace PCToolkit.Sampling
{
    public class PointCloudSampler
    {
        private HaltonMask mask;
        private MultiViewImageSet imageSets;
        private Bounds totalBounds;
        private List<Bounds> subBoundsList = new List<Bounds>();

        public PointCloudSampler(MultiViewImageSet imageSets)
        {
            this.imageSets = imageSets;
            totalBounds = imageSets.bounds;
            var maxLen = Mathf.Max(totalBounds.size.x, totalBounds.size.y, totalBounds.size.z);
            var secondMaxLen = Mathf.Max(Mathf.Min(totalBounds.size.x, totalBounds.size.y), Mathf.Min(totalBounds.size.y, totalBounds.size.z), Mathf.Min(totalBounds.size.x, totalBounds.size.z));
            var devide = maxLen / secondMaxLen;
            devide = Mathf.FloorToInt(devide);
            var variant = 0;
            if (!string.IsNullOrEmpty(imageSets.variantName))
            {
                variant = int.Parse(imageSets.variantName);
            }

            mask = new HaltonMask(imageSets.imageSets.Count, imageSets.rect.x, imageSets.rect.y, devide, variant);
            if (devide >= 2)
            {
                var step = 1f / devide;
                Vector3 dir = maxLen == totalBounds.size.x ? Vector3.right : maxLen == totalBounds.size.y ? Vector3.up : Vector3.forward;
                Vector3 dirCom = Vector3.one - dir;
                var minCom = Vector3.Scale(totalBounds.min, dirCom);
                var maxCom = Vector3.Scale(totalBounds.max, dirCom);
                var lastMaxDir = Vector3.Scale(totalBounds.min, dir);
                for (int i = 0; i < devide; i++)
                {
                    var newMin = lastMaxDir + minCom;
                    lastMaxDir += maxLen * step * dir;
                    var newMax = lastMaxDir + maxCom;
                    if (i == devide)
                    {
                        newMax = totalBounds.max;
                    }

                    var bounds = new Bounds();
                    bounds.SetMinMax(newMin, newMax);
                    bounds.extents *= 1.05f;
                    subBoundsList.Add(bounds);
                }
            }
            else
            {
                subBoundsList.Add(imageSets.bounds);
            }
        }

        public static Color PointSample(Texture2D sampler, Vector2Int pos)
        {
            return sampler.GetPixel(pos.x, pos.y);
        }

        public static float FilterSample(Texture2D sampler, Vector2Int pos)
        {
            var a = sampler.GetPixel(pos.x, pos.y).a / 9f;
            a += sampler.GetPixel(pos.x + 3, pos.y + 3).a / 9f;
            a += sampler.GetPixel(pos.x - 3, pos.y + 3).a / 9f;
            a += sampler.GetPixel(pos.x + 3, pos.y - 3).a / 9f;
            a += sampler.GetPixel(pos.x - 3, pos.y - 3).a / 9f;
            a += sampler.GetPixel(pos.x + 6, pos.y).a / 9f;
            a += sampler.GetPixel(pos.x - 6, pos.y).a / 9f;
            a += sampler.GetPixel(pos.x, pos.y + 6).a / 9f;
            a += sampler.GetPixel(pos.x, pos.y - 6).a / 9f;
            return a;
        }

        const uint factor1 = 64;
        const uint factor2 = factor1 * factor1;
        const uint factor3 = factor2 * factor1;
        private static float DecodeDepth(Color encodedDepth)
        {
            float depth = encodedDepth.r + encodedDepth.g / factor1 + encodedDepth.b / factor2 + encodedDepth.a / factor3;
            var res = 1 - depth * 2;
            return res;
        }

        public List<List<Point>> SamplePoints()
        {
            var pointList = new List<List<Point>>();
            foreach (var bounds in subBoundsList)
            {
                pointList.Add(new List<Point>());
            }
            var visMasks = new List<List<VisibilityMask>>();
            foreach (var bounds in subBoundsList)
            {
                visMasks.Add(new List<VisibilityMask>());
            }

            var maxBounds = totalBounds;
            //enlager bounds to avoid cull edge points.
            maxBounds.extents *= 1.05f;
            // Sample point positions and parameters
            for (int imgIdx = 0; imgIdx < imageSets.count; imgIdx++)
            {
                foreach (var sp in mask.samplePoints)
                {
                    if (FilterSample(imageSets[imgIdx].normal, sp) < 0.99f)
                    {
                        continue;
                    }

                    var encodedDepth = PointSample(imageSets[imgIdx].depth, sp);
                    var depth = DecodeDepth(encodedDepth);
                    var imgPos = new Vector3(sp.x / (float)imageSets[imgIdx].rect.x * 2f - 1f, sp.y / (float)imageSets[imgIdx].rect.y * 2f - 1f, depth);
                    var worldPos = imageSets[imgIdx].imageToWorld.MultiplyPoint(imgPos);
                    if (!maxBounds.Contains(worldPos))
                    {
                        //out of bounds.
                        continue;
                    }

                    var p = new Point(worldPos);
                    var param = PointSample(imageSets[imgIdx].parameters, sp);
                    p.roughness = param.r;
                    p.metallic = param.g;
                    var normalColor = PointSample(imageSets[imgIdx].normal, sp);
                    var normal = new Vector3(normalColor.r, normalColor.g, normalColor.b);
                    p.normal = normal;
                    var detailColor = PointSample(imageSets[imgIdx].detail, sp);
                    var detailedNormal = new Vector3(detailColor.r, detailColor.g, detailColor.b);
                    p.detailedNormal = detailedNormal;
                    var albedo = PointSample(imageSets[imgIdx].albedo, sp);
                    p.albedo = new PCTColor(albedo);
                    var visMask = new VisibilityMask(imageSets.count);
                    visMask[imgIdx] = true;

                    for (int boundsIdx = 0; boundsIdx < subBoundsList.Count; boundsIdx++)
                    {
                        if (subBoundsList[boundsIdx].Contains(p.position))
                        {
                            pointList[boundsIdx].Add(p);
                            visMasks[boundsIdx].Add(visMask);
                        }
                    }
                }
            }

            GC.Collect();
            // Calculate visibilities
            for (int i = 0; i < imageSets.count; i++)
            {
                for (int j = 0; j < pointList.Count; j++)
                {
                    var curIndex = 0;
                    foreach (var p in pointList[j])
                    {
                        if (!visMasks[j][curIndex][i])
                        {
                            var imgPos = imageSets[i].worldToImage.MultiplyPoint(p.position);
                            var sp = new Vector2Int(Mathf.RoundToInt((imgPos.x + 1f) / 2f * imageSets[i].rect.x),
                                Mathf.RoundToInt((imgPos.y + 1f) / 2f * imageSets[i].rect.y));
                            var curDepth = DecodeDepth(PointSample(imageSets[i].depth, sp));
                            // Visibility check
                            if (Mathf.Abs(curDepth - imgPos.z) <= 0.001f)
                            {
                                visMasks[j][curIndex][i] = true;
                            }
                        }
                        curIndex++;
                    }
                }
            }

            GC.Collect();
            for (int j = 0; j < pointList.Count; j++)
            {
                // Sample raw colors
                for (int i = 0; i < pointList[j].Count; i++)
                {
                    var weight = 1f / visMasks[j][i].weight;
                    Vector3 colorValues = Vector3.zero;
                    for (int k = 0; k < imageSets.count; k++)
                    {
                        if (visMasks[j][i][k])
                        {
                            var imgPos = imageSets[k].worldToImage.MultiplyPoint(pointList[j][i].position);
                            var sp = new Vector2Int(Mathf.RoundToInt((imgPos.x + 1f) / 2f * imageSets[k].rect.x),
                                Mathf.RoundToInt((imgPos.y + 1f) / 2f * imageSets[k].rect.y));
                            var rawColor = PointSample(imageSets[k].shaded, sp);
                            colorValues += weight * new Vector3(rawColor.r, rawColor.g, rawColor.b);
                        }
                    }

                    var p = pointList[j][i];
                    p.rawColor = new PCTColor(colorValues.x, colorValues.y, colorValues.z);
                    pointList[j][i] = p;
                }
            }

            GC.Collect();
            return pointList;
        }
    }
}

