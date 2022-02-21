using UnityEngine;

[System.Serializable]
public class SphereGuassians : Object
{
    public const int length = 64;
    public Vector4[] directions = new Vector4[length];
    public Vector4[] features = new Vector4[length];

    public SphereGuassians()
    {
        for (int i = 0; i < length; i++)
        {
            directions[i] = Random.onUnitSphere;
            if (directions[i].y < -0.2f) directions[i].y = -directions[i].y;
            float a = Random.Range(0, 1.0f);
            float exp = Random.Range(3f, 10.0f);
            float s = Mathf.Pow(2, exp);
            features[i] = new Vector2(a, s);
        }
    }
}
