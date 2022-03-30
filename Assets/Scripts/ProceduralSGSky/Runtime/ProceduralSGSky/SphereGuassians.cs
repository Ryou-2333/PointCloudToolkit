using UnityEngine;
using System;

[Serializable]
public class SphereGuassians
{
    public const int length = 128;
    public Vector4[] directions = new Vector4[length];
    public Vector4[] features = new Vector4[length];
}
