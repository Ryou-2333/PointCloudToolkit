using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "LightingParams", menuName = "PCTK/Lighting Params", order = 0)]
public class LightingParams : ScriptableObject
{
    public Vector3 DLDirection;
    public float DLIntensity;
    public Vector3[] SGColors = new Vector3[SphereGuassians.length];
    public Vector3[] SGDirections = new Vector3[SphereGuassians.length];
    public float[] SGSharpnesses = new float[SphereGuassians.length];

    public static LightingParams GetRandomParams()
    {
        LightingParams lp = new LightingParams();
        Random.InitState(System.DateTime.Now.Millisecond);
        var t = Random.Range(0f, 1f);
        if (t > 0.5f)
        {
            lp.GenerateInDoor();
        }
        else
        {
            lp.GenerateOutdoor();
        }

        return lp;
    }

    public static float Luminance(Vector3 color)
    {
        return Vector3.Dot(color, new Vector3(0.21f, 0.71f, 0.07f));
    }

    public void ApplyToScene(bool mono)
    {
        var dl = GameObject.Find("DirectionalLight");
        dl.transform.rotation = Quaternion.Euler(DLDirection);
        var hdLight = dl.GetComponent<HDAdditionalLightData>();
        hdLight.intensity = DLIntensity;
        var sgs = GetSG(mono);
        SGSkyContext.Instance.SGs = sgs;
    }

    public SphereGuassians GetSG(bool mono)
    {
        var sgs = new SphereGuassians();
        for (int i = 0; i < SphereGuassians.length; i++)
        {
            sgs.directions[i] = SGDirections[i];
            if (mono)
            {
                var lum = Luminance(SGColors[i]);
                sgs.features[i] = Vector3.one * lum;
            }
            else
            {
                sgs.features[i] = SGColors[i];
            }

            sgs.features[i].w = SGSharpnesses[i];
        }

        return sgs;
    }

    private void GenerateInDoor()
    {
        float rndX = Random.Range(30, 60);
        float rndY = Random.Range(0, 360);
        DLDirection = new Vector3(rndX, rndY, 0);
        DLIntensity = Random.Range(0, 30);

        for (int i = 0; i < SphereGuassians.length; i++)
        {
            SGDirections[i] = Random.onUnitSphere;
            float s = Random.Range(0.1f, 0.8f);
            float v = Random.Range(0.3f, 0.9f);
            var c = Random.ColorHSV(0f, 0.6f, s, s + 0.1f, v, v + 0.1f);
            SGColors[i] = new Vector3(c.r, c.g, c.b);
            float exp = Random.Range(4f, 10.0f);
            SGSharpnesses[i] = Mathf.Pow(2, exp);
        }
    }

    private void GenerateOutdoor()
    {
        float rndX = Random.Range(30, 60);
        float rndY = Random.Range(0, 360);
        DLDirection = new Vector3(rndX, rndY, 0);
        DLIntensity = Random.Range(50, 250);

        var skyColor = Random.ColorHSV(0.516f, 0.622f, 0, 0.9f, 1f, 1.5f);
        SGDirections[0] = Vector3.up;
        SGColors[0] = new Vector3(skyColor.r, skyColor.g, skyColor.b);
        SGSharpnesses[0] = Random.Range(1.7f, 1.9f);

        var gv = Random.Range(0.4f, 1f);
        var gs = Random.Range(0f, 0.6f);
        if (gs < 0.3f)
        {
            gs = gs / 6f;
        }
        var groundColor = Random.ColorHSV(0.1f, 0.3f, gs, gs + 0.01f, gv, gv + 0.01f);
        SGDirections[1] = Vector3.down;
        SGColors[1] = new Vector4(groundColor.r, groundColor.g, groundColor.b);
        SGSharpnesses[1] = Random.Range(1.1f, 1.3f);

        int cloudCount = Mathf.FloorToInt(Random.Range(0f, 50f));
        if (cloudCount < 20) cloudCount = 0;
        for (int i = 2; i < cloudCount + 2; i++)
        {
            var dir = Random.insideUnitCircle.normalized;
            var y = Random.Range(0.35f, 1.0f);
            SGDirections[i] = new Vector3(dir.x, y, dir.y).normalized;
            if (SGDirections[i].y < 0f) SGDirections[i].y = -SGDirections[i].y;
            var c = Random.ColorHSV(0.06f, 0.11f, 0, 0.5f, 0.97f, 1f);
            SGColors[i] = new Vector4(c.r, c.g, c.b);
            float exp = Random.Range(4f, 10.0f);
            SGSharpnesses[i] = Mathf.Pow(2, exp);
        }

        int groundCount = Mathf.FloorToInt(Random.Range(0f, 50f));
        for (int i = cloudCount + 2; i < cloudCount + groundCount + 2; i++)
        {
            var dir = Random.insideUnitCircle.normalized;
            var y = Random.Range(-0.1f, 0.1f);
            SGDirections[i] = new Vector3(dir.x, y, dir.y).normalized;
            if (SGDirections[i].y < 0f) SGDirections[i].y = -SGDirections[i].y;
            var c = Random.ColorHSV(0.14f, 0.34f, 0.7f, 0.9f, 0.25f, 0.38f);
            SGColors[i] = new Vector4(c.r, c.g, c.b);
            float exp = Random.Range(6f, 10.0f);
            SGSharpnesses[i] = Mathf.Pow(2, exp);
        }
    }
}
