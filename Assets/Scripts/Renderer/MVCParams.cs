using System.Collections.Generic;
using UnityEngine;

namespace PCToolkit.Rendering
{
    [CreateAssetMenu(fileName = "MVCParams", menuName = "PCTK/Multi-view Camera Params", order = 0)]
    public class MVCParams : ScriptableObject
    {
        public CaptureCamera cameraPrefab;
        public int[] perLaryerCameras;
        public float distFactor = 2f;
    }
}

