using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCToolkit.Rendering;
using PCToolkit.Data;

namespace PCToolkit.Pipeline
{
    public class ShadedCapturePipeline : MonoBehaviour
    {
        [SerializeField] int startObjIdx;
        [SerializeField] int endObjIdx;
        [SerializeField] MultiViewCamera mvc;
        [SerializeField] TargetRenderer target;

        private int curObjIdx;
        private int curCamIdx;
        private bool firstUpdate;
        private bool captureEnd;
        private MultiViewImageSet mvis;

        private void Awake()
        {
            curObjIdx = startObjIdx;
            curCamIdx = 0;
            LoadTarget();
            mvc.SpawnCameras(target);
            firstUpdate = true;
            captureEnd = false;
            RefreshImgSet();
        }

        public void PreCapture()
        {
            Debug.Log(string.Format("Prepareing capture shaded object {0} with camera no.{1}.", curObjIdx, curCamIdx));
            mvc.PrepareCapture(target, curCamIdx);
            StartCoroutine(CaptureAfterRendering());
        }

        public void Capture()
        {
            var data = mvc.Capture(curCamIdx);
            mvis[curCamIdx].SetTexture(MeshRenderMode.Shaded, data.texture);
            Debug.Log(string.Format("End capture shaded object {0} with camera no.{1}.", curObjIdx, curCamIdx));
        }

        private IEnumerator CaptureAfterRendering()
        {
            yield return new WaitForEndOfFrame();
            Capture();
            ToNextCapture();
        }

        private void LoadTarget()
        {
            //todo: Load target according to curObjIdx;
            target.renderMode = MeshRenderMode.Shaded;
        }

        private void RefreshImgSet()
        {
            mvis = new MultiViewImageSet();
            mvis.fileName = target.name;
            mvis.bounds = target.bounds;
            mvis.imageSets = new List<ImageSet>();
            foreach (var cam in mvc.cameras)
            {
                var imgset = new ImageSet();
                imgset.imageToWorld = cam.imageToWorld;
                imgset.worldToImage = cam.worldToImage;
                mvis.imageSets.Add(imgset);
            }
        }

        private void ToNextCapture()
        {
            if (curCamIdx < mvc.count - 1)
            {
                curCamIdx++;
            }
            else
            {
                curCamIdx = 0;
                ToNextObject();
            }
        }

        private void ToNextObject()
        {
            ImageSetIO.SaveImageSet(mvis);
            if (curObjIdx >= endObjIdx)
            {
                Debug.Log(string.Format("Objects {0} - {1} captured.", startObjIdx, endObjIdx));
                captureEnd = true;
                gameObject.SetActive(false);
            }
            else
            {
                //Next target
                curObjIdx++;
                LoadTarget();
                RefreshImgSet();
            }
        }

        private void Update()
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                return;
            }

            if (captureEnd)
            {
                return;
            }

            PreCapture();
        }
    }
}
