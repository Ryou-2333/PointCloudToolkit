using System.Collections;
using UnityEngine;
using PCToolkit.Rendering;
using PCToolkit.Data;

namespace PCToolkit.Pipeline
{
    public class ParameterImagePipeline : MonoBehaviour
    {
        [SerializeField] int startObjIdx;
        [SerializeField] int endObjIdx;
        [SerializeField] MultiViewCamera mvc;
        //todo : assign this field in code.
        [SerializeField] TargetRenderer target;

        private int curObjIdx;
        private int curCamIdx;
        private MeshRenderMode curRenderMode;
        private bool firstUpdate;

        private void Awake()
        {
            curObjIdx = startObjIdx;
            curCamIdx = 0;
            curRenderMode = (MeshRenderMode)1;
            target.renderMode = curRenderMode;
            mvc.SpawnCameras(target);
            firstUpdate = true;
        }

        private void Update()
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                return;
            }

            if (curObjIdx > endObjIdx + 1)
            {
                return;
            }

            if (curObjIdx == endObjIdx + 1)
            {
                Debug.Log(string.Format("Objects {0} - {1} captured.", startObjIdx, endObjIdx));
                curObjIdx++;
                return;
            }

            Debug.Log(string.Format("Prepareing capture object {0} with camera no.{1}, mode {2}.", curObjIdx, curCamIdx, curRenderMode));
            mvc.PrepareCaptureMode(target, curCamIdx);
            StartCoroutine(CaptureAfterRendering());
        }

        private IEnumerator CaptureAfterRendering()
        {
            yield return new WaitForEndOfFrame();
            var data = mvc.Capture(curCamIdx);
            Debug.Log(string.Format("End capture object {0} with camera no.{1}, mode {2}.", curObjIdx, curRenderMode, curRenderMode));
            Debug.Log(data);
            if (curCamIdx < mvc.count - 1)
            {
                curCamIdx++;
            }
            else
            {
                curCamIdx = 0;
                if (curRenderMode == MeshRenderMode.Depth)
                {
                    curRenderMode = (MeshRenderMode)1;
                    curObjIdx++;
                    //todo : next target
                    target.renderMode = curRenderMode;

                }
                else
                {
                    curRenderMode = (MeshRenderMode)((int)curRenderMode << 1);
                    target.renderMode = curRenderMode;
                }
            }
        }
    }
}

