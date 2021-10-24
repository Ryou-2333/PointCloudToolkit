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
        [SerializeField] Volume volume;
        [SerializeField] Light directional;
        [SerializeField] string variant;

        private TargetRenderer target;
        private int curObjIdx;
        private int curCamIdx;
        private bool firstUpdate;
        private bool captureEnd;
        private MultiViewImageSet mvis;

        private void Awake()
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            curObjIdx = startObjIdx;
            curCamIdx = 0;
            LoadTarget();
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

        public void ChangeLightAndDround()
        {
            float rndX = Random.Range(30, 60);
            float rndY = Random.Range(0, 360);
            directional.transform.rotation = Quaternion.Euler(rndX, rndY, 0);
            int rndIdx = Random.Range(0, groundMats.Count);
            ground.GetComponent<Renderer>().material = groundMats[rndIdx];
        }

        private void LoadTarget()
        {
            if (target != null)
            {
                Destroy(target.gameObject);
                ChangeLightAndDround();
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
            ground.transform.position = Vector3.up * target.bounds.min.y;
            target.gameObject.name = curDirName;
            target.renderMode = MeshRenderMode.Shaded;
            mvc.SpawnCameras(target);
        }

        private void RefreshImgSet()
        {
            mvis = new MultiViewImageSet();
            mvis.variantName = variant;
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
