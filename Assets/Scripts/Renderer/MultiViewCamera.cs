using PCToolkit.Data;
using System.Collections.Generic;
using UnityEngine;

namespace PCToolkit.Rendering
{
    public class MultiViewCamera : MonoBehaviour
    {
        public CaptureCamera cameraPrefab;
        public int[] perLaryerCameras;
        public List<Vector3> cameraOffsets = new List<Vector3>();
        public List<CaptureCamera> cameras = new List<CaptureCamera>();
        public float dixtFactor = 1.5f;

        public void CalculateCameraPositions()
        {
            cameraOffsets.Clear();
            int layerCounts = perLaryerCameras.Length;

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
                for (int j = 0; j < perLaryerCameras[i]; j++)
                {
                    if (j < perLaryerCameras[i] / 4)
                    {
                        float t = 1f / (perLaryerCameras[i] / 4f) * j;
                        cameraOffsets.Add(Vector3.Slerp(curFront, curRight, t));
                    }
                    else if (j < perLaryerCameras[i] / 2)
                    {
                        float t = 1f / (perLaryerCameras[i] / 4f) * (j - perLaryerCameras[i] / 4);
                        cameraOffsets.Add(Vector3.Slerp(curRight, curBack, t));
                    }
                    else if (j < perLaryerCameras[i] / 4 * 3)
                    {
                        float t = 1f / (perLaryerCameras[i] / 4f) * (j - perLaryerCameras[i] / 2);
                        cameraOffsets.Add(Vector3.Slerp(curBack, curLeft, t));
                    }
                    else
                    {
                        float t = 1f / (perLaryerCameras[i] / 4f) * (j - perLaryerCameras[i] / 4 * 3);
                        cameraOffsets.Add(Vector3.Slerp(curLeft, curFront, t));
                    }
                }
            }
        }

        public void InitCameras()
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
            foreach (var pos in cameraOffsets)
            {
                var cam = Instantiate(cameraPrefab);
                cam.gameObject.SetActive(false);
                cameras.Add(cam);
            }
        }

        private float CalculateDistance(TargetRenderer targetRenderer)
        {

            return targetRenderer.maxBound * dixtFactor;
        }

        public void SpawnCameras(TargetRenderer targetRenderer)
        {
            if (cameras.Count != cameraOffsets.Count || cameras.Count == 0)
            {
                InitCameras();
            }

            var distance = CalculateDistance(targetRenderer);
            Vector3 targetPos = targetRenderer.center;

            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].transform.position = cameraOffsets[i] * distance + targetPos;
                cameras[i].transform.LookAt(targetPos);
            }
        }

        private List<CaptureData> CaptureCameras(TargetRenderer targetRenderer, int count)
        {
            var c = count > 0 ? count : cameras.Count;
            var data = new List<CaptureData>();
            for (int i = 0; i < c; i++)
            {
                if (targetRenderer.renderMode == MeshRenderMode.Depth)
                {
                    data.Add(cameras[i].CaptureDepth());
                }
                else
                {
                    data.Add(cameras[i].CaptureColor());
                }
            }

            return data;
        }

        public MultiViewImageSet CaptureModes(TargetRenderer targetRenderer, int count = -1)
        {
            targetRenderer.renderMode = MeshRenderMode.Depth;
            var depthImgs = CaptureCameras(targetRenderer, count);
            targetRenderer.renderMode = MeshRenderMode.Albedo;
            var albedoImgs = CaptureCameras(targetRenderer, count);
            targetRenderer.renderMode = MeshRenderMode.Parameter;
            var paramImgs = CaptureCameras(targetRenderer, count);
            targetRenderer.renderMode = MeshRenderMode.Normal;
            var normalImgs = CaptureCameras(targetRenderer, count);
            var imageSets = new List<ImageSet>();
            for (int i = 0; i < albedoImgs.Count; i++)
            {
                var set = new ImageSet();
                set.imageToWorld = depthImgs[i].imageToWorld;
                set.depth = depthImgs[i].texture;
                set.albedo = albedoImgs[i].texture;
                set.parameters = paramImgs[i].texture;
                set.normal = normalImgs[i].texture;
                imageSets.Add(set);
            }

            var mvis = new MultiViewImageSet();
            mvis.volume = targetRenderer.size;
            mvis.imageSets = imageSets.ToArray();
            return mvis;
        }
    }
}

