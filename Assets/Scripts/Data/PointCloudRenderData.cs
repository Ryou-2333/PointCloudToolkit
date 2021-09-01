using UnityEngine;

namespace PCToolkit.Data
{
    [CreateAssetMenu(fileName = "PointCloudRenderData", menuName = "PCTK/PointCloudRenderData", order = 2)]
    [PreferBinarySerialization]
    public class PointCloudRenderData : ScriptableObject
    {
        public Point[] pointCloudBuffer;
        [HideInInspector]
        ComputeBufferData[] _posrcBuffer;
        [HideInInspector]
        ComputeBufferDataPacked[] _mrdaBuffer;
        [HideInInspector]
        ComputeBufferData[] _normalBuffer;
        [HideInInspector]
        public ComputeBuffer _renderBuffer0;
        public ComputeBuffer _renderBuffer1;
        public ComputeBuffer _renderBuffer2;
        public const int elementSize = sizeof(float) * 4;

        public int pointCount 
        { 
            get 
            {
                if (pointCloudBuffer != null)
                    return pointCloudBuffer.Length;
                else
                    return 0;
            } 
        }
        public ComputeBuffer renderBuffer0
        {
            get
            {
                if (pointCount <= 0)
                {
                    return null;
                }

                if (_posrcBuffer == null)
                {
                    Initialize();
                }

                if (_renderBuffer0 == null)
                {
                    _renderBuffer0 = new ComputeBuffer(pointCount, elementSize);
                    _renderBuffer0.SetData(_posrcBuffer);
                }

                return _renderBuffer0;
            }
        }

        public ComputeBuffer renderBuffer1
        {
            get
            {
                if (_mrdaBuffer == null)
                {
                    Initialize();
                }
                if (_renderBuffer1 == null)
                {
                    _renderBuffer1 = new ComputeBuffer(pointCount, elementSize);
                    _renderBuffer1.SetData(_mrdaBuffer);
                }
                return _renderBuffer1;
            }
        }

        public ComputeBuffer renderBuffer2
        {
            get
            {
                if (_normalBuffer == null)
                {
                    Initialize();
                }
                if (_renderBuffer2 == null)
                {
                    _renderBuffer2 = new ComputeBuffer(pointCount, elementSize);
                    _renderBuffer2.SetData(_normalBuffer);
                }
                return _renderBuffer2;
            }
        }

        void ClearComputeBuffer()
        {
            if (_renderBuffer0 != null)
            {
                _renderBuffer0.Release();
                _renderBuffer0 = null;
            }

            if (_renderBuffer1 != null)
            {
                _renderBuffer1.Release();
                _renderBuffer1 = null;
            }

            if (_renderBuffer2 != null)
            {
                _renderBuffer2.Release();
                _renderBuffer2 = null;
            }
        }

        void ClearRenderData()
        {
            _mrdaBuffer = null;
            _posrcBuffer = null;
            _normalBuffer = null;
        }

        public void Refresh()
        {
            ClearRenderData();
            ClearComputeBuffer();
        }

        void OnDisable()
        {
            ClearComputeBuffer();
        }

        void OnDestroy()
        {
            ClearComputeBuffer();
        }

        public void Initialize()
        {
            _posrcBuffer = new ComputeBufferData[pointCount];
            _mrdaBuffer = new ComputeBufferDataPacked[pointCount];
            _normalBuffer = new ComputeBufferData[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                _posrcBuffer[i] = pointCloudBuffer[i].PackPosRC();
                _mrdaBuffer[i] = pointCloudBuffer[i].PackMRDA();
                _normalBuffer[i] = pointCloudBuffer[i].PackNormal();
            }
        }
    }
}
