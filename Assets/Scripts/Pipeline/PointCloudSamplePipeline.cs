using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCToolkit.Rendering;
using PCToolkit.Data;
using PCToolkit.Sampling;
using System;

namespace PCToolkit.Pipeline
{
    public class PointCloudSamplePipeline : MonoBehaviour
    {
        [SerializeField] int startObjIdx;
        [SerializeField] int endObjIdx;

        private int curObjIdx;
        private MultiViewImageSet mvis;
        bool endSample;
        bool firstUpdate;

        public string FormatFolderName()
        {
            //todo: return folderName according to curObjIdx.
            return "001";
        }

        public void Sample(string folderName)
        {
            try
            {
                Debug.Log(string.Format("Sampling point cloud data for object {0}", folderName));
                mvis = ImageSetIO.LoadImageSet(folderName);
                var sampler = new PointCloudSampler(mvis);
                var data = sampler.SamplePoints();
                PointCloudIO.SavePointCloud(data, folderName);
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Error sampling object {0}: {1}\n{2}", folderName, e.Message, e.StackTrace));
            }
            ToNextObject();
        }

        public void ToNextObject()
        {
            curObjIdx++;
            if (curObjIdx > endObjIdx)
            {
                endSample = true;
                Debug.Log(string.Format("End Sample point cloud data {0} - {1}", startObjIdx, endObjIdx));
                gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            curObjIdx = startObjIdx;
            endSample = false;
            firstUpdate = true;
        }

        private void Update()
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                return;
            }
            if (endSample) return;
            Sample(FormatFolderName());
        }
    }
}
