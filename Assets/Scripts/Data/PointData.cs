﻿using System;
using UnityEngine;


namespace PCToolkit.Data
{
    [Serializable]
    public struct ComputeBufferData
    {
        public Vector3 raw;
        public uint packed;

        public ComputeBufferData(Vector3 raw, uint packed)
        {
            this.raw = raw;
            this.packed = packed;
        }

        public ComputeBufferData(float x, float y, float z, uint packed)
        {
            this.raw = new Vector3(x, y, z);
            this.packed = packed;
        }
    }

    [Serializable]
    public struct PCTColor
    {
        public float r;
        public float g;
        public float b;

        public PCTColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public PCTColor(Color c)
        {
            this.r = c.r;
            this.g = c.g;
            this.b = c.b;
        }

        const float kMaxBrightness = 16;
        public uint Pack()
        {
            var y = Mathf.Max(Mathf.Max(r, g), b);
            y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

            var rgb = new Vector3(r, g, b);
            rgb *= 255 * 255 / (y * kMaxBrightness);

            return ((uint)rgb.x) |
                   ((uint)rgb.y << 8) |
                   ((uint)rgb.z << 16) |
                   ((uint)y << 24);
        }

        public static PCTColor UnPack(uint packed)
        {
            uint brightness = (packed >> 24) & 0xff;
            float k = kMaxBrightness * brightness / (255 * 255);
            var r = ((packed) & 0xff) * k;
            var g = ((packed >> 8) & 0xff) * k;
            var b = ((packed >> 16) & 0xff) * k;

            return new PCTColor(r, g, b);
        }

        public uint packed { get { return Pack(); } }
        public static PCTColor black {get { return new PCTColor(0, 0, 0); } }
    }

    [Serializable]
    public class Point
    {
        public Vector3 position;
        public PCTColor rawColor;
        //public Vector3 normal;
        public float height;
        public float metallic;
        public float roughness;
        public PCTColor albedo;

        public Point(Vector3 pos)
        {
            position = pos;
            rawColor = PCTColor.black;
            height = 0;
            metallic = 0;
            roughness = 0;
            albedo = PCTColor.black;
        }

        public ComputeBufferData PackToPosRC()
        {
            return new ComputeBufferData(position, rawColor.packed);
        }

        public ComputeBufferData PackToHMRA()
        {
            return new ComputeBufferData(height, metallic, roughness, albedo.packed);
        }
    }
}