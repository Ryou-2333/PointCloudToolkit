using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PCToolkit.Rendering;
using PCToolkit.Data;
using UnityEditor;

namespace PCToolkit.Pipeline
{
    public class ShadedCapturePipeline : MonoBehaviour
    {
        [SerializeField] int startObjIdx;
        [SerializeField] int endObjIdx;
        [SerializeField] MultiViewCamera mvc;
        [SerializeField] GameObject ground;
        [SerializeField] string datasetPath = "Datasets/Megascan/001";
        [SerializeField] List<Material> groundMats;
        [SerializeField] Material onliyLighting;
        [SerializeField] Volume volume;
        [SerializeField] Light directional;
        [SerializeField] string variant;

        private TargetRenderer target;
        private int curObjIdx;
        private bool samplingLight = false;
        private int curCamIdx;
        private bool firstUpdate = false;
        private bool captureEnd;
        private MultiViewImageSet mvis;
        private LightingParams curLighting = null;

        private void Awake()
        {
            firstUpdate = true;
        }

        public void PreCapture()
        {
            if (rendering) return;
            Debug.Log(string.Format("Prepareing capture shaded object {0} with camera no.{1}.", curObjIdx, curCamIdx));
            mvc.PrepareCapture(target, curCamIdx);
            StartCoroutine(CaptureAfterRendering());
        }

        public void Capture()
        {
            var data = mvc.Capture(curCamIdx, target.name + "_" + target.renderMode.ToString());
            if (!samplingLight)
            {
                mvis[curCamIdx].SetTexture(MeshRenderMode.Shaded, data.texture);
                Debug.Log(string.Format("End capture shaded object {0} with camera no.{1}.", curObjIdx, curCamIdx));
            }
            else
            {
                mvis[curCamIdx].SetTexture(MeshRenderMode.OnlyLighting, data.texture);
                Debug.Log(string.Format("End capture shaded object {0} with camera no.{1}.", curObjIdx, curCamIdx));
            }
        }

        bool rendering = false;
        private IEnumerator CaptureAfterRendering()
        {
            rendering = true;
            yield return new WaitForEndOfFrame();
            Capture();
            ToNextCapture();
            rendering = false;
        }

        private void LoadTarget()
        {
            if (target != null)
            {
                Destroy(target.gameObject);
                Resources.UnloadUnusedAssets();
            }

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
            var targetPrefab = AssetDatabase.LoadAssetAtPath<TargetRenderer>(string.Format("Assets/{0}/{1}/{1}.prefab", datasetPath, curDirName));
            target = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
            //ground.transform.position = Vector3.up * target.bounds.min.y;
            target.gameObject.name = curDirName;
            target.renderMode = samplingLight ? MeshRenderMode.OnlyLighting : MeshRenderMode.Shaded;
            if (!samplingLight)
            {
                mvc.SpawnCameras(target);
                curLighting = LightingParams.GetRandomParams();
            }

            curLighting.ApplyToScene(samplingLight);
            RefreshImgSet(curLighting);
        }

        private void RefreshImgSet(LightingParams lighting)
        {
            mvis = new MultiViewImageSet();
            mvis.variantName = variant;
            mvis.fileName = target.name;
            mvis.bounds = target.bounds;
            mvis.imageSets = new List<ImageSet>();
            mvis.lightingParams = lighting;
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
                if (samplingLight)
                {
                    ToNextObject();
                }
                else
                {
                    ToLighting();
                }
            }
        }

        private void ToLighting()
        {
            samplingLight = true;
            ImageSetIO.SaveImageSet(mvis);
            LoadTarget();
        }

        private void ToNextObject()
        {
            samplingLight = false;
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
            }
        }

        private void Update()
        {
            if (firstUpdate)
            {
                firstUpdate = false;
                curObjIdx = startObjIdx;
                curCamIdx = 0;
                samplingLight = false;
                firstUpdate = false;
                captureEnd = false;
                LoadTarget();
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
