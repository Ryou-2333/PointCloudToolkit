using PCToolkit.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace PCToolkit.Rendering
{
    public class MultiViewCamera : MonoBehaviour
    {
        [SerializeField] MVCParams parameters;
        public List<Vector3> cameraOffsets = new List<Vector3>();
        public List<CaptureCamera> cameras = new List<CaptureCamera>();
        [SerializeField] Volume globalVolume;

        public void CalculateCameraPositions()
        {
            cameraOffsets.Clear();
            int layerCounts = parameters.perLaryerCameras.Length;

            for (int i = 0; i < layerCounts; i++)
            {
                Vector3 curFront, curRight, curBack, curLeft;
                //Calculate offsets for up/front/down
                if (i < layerCounts / 2)
                {
                    float t = 1f / (layerCounts / 2f) * i;
                    curFront = Vector3.Slerp(Vector3.up, Vector3.forward, t);
                    curRight = Vector3.Slerp(Vector3.up, Vector3.right, t);
                    curBack = Vector3.Slerp(Vector3.up, Vector3.back, t);
                    curLeft = Vector3.Slerp(Vector3.up, Vector3.left, t);
                }
                else
                {
                    float t = 1f / (layerCounts / 2f) * (i - layerCounts / 2);
                    curFront = Vector3.Slerp(Vector3.forward, Vector3.down, t);
                    curRight = Vector3.Slerp(Vector3.right, Vector3.down, t);
                    curBack = Vector3.Slerp(Vector3.back, Vector3.down, t);
                    curLeft = Vector3.Slerp(Vector3.left, Vector3.down, t);
                }
                for (int j = 0; j < parameters.perLaryerCameras[i]; j++)
                {
                    if (j < parameters.perLaryerCameras[i] / 4)
                    {
                        float t = 1f / (parameters.perLaryerCameras[i] / 4f) * j;
                        cameraOffsets.Add(Vector3.Slerp(curFront, curRight, t));
                    }
                    else if (j < parameters.perLaryerCameras[i] / 2)
                    {
                        float t = 1f / (parameters.perLaryerCameras[i] / 4f) * (j - parameters.perLaryerCameras[i] / 4);
                        cameraOffsets.Add(Vector3.Slerp(curRight, curBack, t));
                    }
                    else if (j < parameters.perLaryerCameras[i] / 4 * 3)
                    {
                        float t = 1f / (parameters.perLaryerCameras[i] / 4f) * (j - parameters.perLaryerCameras[i] / 2);
                        cameraOffsets.Add(Vector3.Slerp(curBack, curLeft, t));
                    }
                    else
                    {
                        float t = 1f / (parameters.perLaryerCameras[i] / 4f) * (j - parameters.perLaryerCameras[i] / 4 * 3);
                        cameraOffsets.Add(Vector3.Slerp(curLeft, curFront, t));
                    }
                }
            }
        }

        private float CalculateDistance(TargetRenderer targetRenderer)
        {

            return targetRenderer.maxBound * parameters.distFactor;
        }

        public void SpawnCameras(TargetRenderer targetRenderer)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i] != null)
                {
                    Destroy(cameras[i]);
                }
            }

            cameras.Clear();
            CalculateCameraPositions();
            int sampleCount = 1;
            if (globalVolume != null)
            {
                PathTracing pt;
                if (globalVolume.profile.TryGet(out pt))
                {
                    if (pt.enable.GetValue<bool>())
                    {
                        sampleCount = pt.maximumSamples.GetValue<int>();
                    }
                }
            }

            var targetPos = targetRenderer.bounds.center;
            var distance = CalculateDistance(targetRenderer);
            foreach (var offset in cameraOffsets)
            {
                var pos = offset * distance + targetPos;
                if (pos.y < 0)
                {
                    //under ground camera is not realistic.
                    continue;
                }
                var cam = Instantiate(parameters.cameraPrefab);
                cam.transform.position = pos;
                cam.transform.LookAt(targetPos);
                cam.sampleCount = sampleCount;
                cam.gameObject.SetActive(false);
                cameras.Add(cam);
            }
        }

        private IEnumerator CaptureCameras(TargetRenderer targetRenderer, int count, List<CaptureData> data)
        {
            var c = count > 0 ? count : cameras.Count;
            for (int i = 0; i < c; i++)
            {
                Debug.Log(string.Format("Capturing target: {0}, mode: {1}, camera index: {2}",
                    targetRenderer.name, targetRenderer.renderMode.ToString(), i));
                if (targetRenderer.renderMode == MeshRenderMode.Depth)
                {
                    data.Add(cameras[i].CaptureDepth());
                }
                else
                {
                    data.Add(cameras[i].CaptureColor());
                }
                yield return null;
            }
        }

        public IEnumerator CaptureParameters(TargetRenderer targetRenderer, MultiViewImageSet mvis, int count = -1)
        {
            Debug.Log("Start capture target: " + targetRenderer.name + ". mode: parameters.");
            targetRenderer.renderMode = MeshRenderMode.Depth;
            var depthImgs = new List<CaptureData>();
            yield return CaptureCameras(targetRenderer, count, depthImgs);
            targetRenderer.renderMode = MeshRenderMode.Albedo;
            var albedoImgs = new List<CaptureData>();
            yield return CaptureCameras(targetRenderer, count, albedoImgs);
            targetRenderer.renderMode = MeshRenderMode.Parameter;
            var paramImgs = new List<CaptureData>();
            yield return CaptureCameras(targetRenderer, count, paramImgs);
            targetRenderer.renderMode = MeshRenderMode.Normal;
            var normalImgs = new List<CaptureData>();
            yield return CaptureCameras(targetRenderer, count, paramImgs);
            var imageSets = new List<ImageSet>();
            for (int i = 0; i < albedoImgs.Count; i++)
            {
                var set = new ImageSet();
                set.imageToWorld = depthImgs[i].imageToWorld;
                set.worldToImage = depthImgs[i].worldToImage;
                set.depth = depthImgs[i].texture;
                set.albedo = albedoImgs[i].texture;
                set.parameters = paramImgs[i].texture;
                set.normal = normalImgs[i].texture;
                imageSets.Add(set);
            }

            mvis.bounds = targetRenderer.bounds;
            mvis.imageSets = imageSets.ToArray();
            Debug.Log("End capture target: " + targetRenderer.name + ". mode: parameters.");
        }

        public IEnumerator CaptureRaw(TargetRenderer targetRenderer, MultiViewImageSet mvis, int count = -1)
        {
            Debug.Log("Start capture target: " + targetRenderer.name + ". mode: raw.");
            targetRenderer.renderMode = MeshRenderMode.Shaded;
            var rawImgs = new List<CaptureData>();
            yield return CaptureCameras(targetRenderer, count, rawImgs);
            var imageSets = new List<ImageSet>();
            for (int i = 0; i < rawImgs.Count; i++)
            {
                var set = new ImageSet();
                set.imageToWorld = rawImgs[i].imageToWorld;
                set.worldToImage = rawImgs[i].worldToImage;
                set.shaded = rawImgs[i].texture;
                imageSets.Add(set);
            }

            mvis.bounds = targetRenderer.bounds;
            mvis.imageSets = imageSets.ToArray();
            Debug.Log("End capture target: " + targetRenderer.name + ". mode: raw.");
        }
    }
}

