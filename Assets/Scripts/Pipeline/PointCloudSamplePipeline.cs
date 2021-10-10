using UnityEngine;
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
            int digit = 4;
            var pow = curObjIdx / 10;
            while (pow > 0)
            {
                digit--;
                pow /= 10;
            }
            var curDirName = "";
            for (int i = 0; i < digit; i++)
            {
                curDirName += "0";
            }

            curDirName += curObjIdx;
            return curDirName;
        }

        public void Sample(string folderName)
        {
            try
            {
                Debug.Log(string.Format("Sampling point cloud data for object {0}", folderName));
                mvis = ImageSetIO.LoadImageSet(folderName);
                var sampler = new PointCloudSampler(mvis);
                var dataList = sampler.SamplePoints();
                for (int i = 0; i < dataList.Count; i++)
                {
                    PointCloudIO.SavePointCloud(dataList[i], folderName, i.ToString());
                }
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Error sampling object {0}: {1}\n{2}", folderName, e.Message, e.StackTrace));
            }
            ToNextObject();
        }

        public void ToNextObject()
        {
            GC.Collect();
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
