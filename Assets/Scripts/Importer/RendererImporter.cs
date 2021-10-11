using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using PCToolkit.Rendering;

namespace PCToolkit.Pipeline
{
    public class RendererImporter : MonoBehaviour
    {
        static string datasetPath = "Datasets/Megascan/001";
        [MenuItem("PCToolkit/Change Textures settings")]
        static void ReimportMaskAndNormal()
        {
            DirectoryInfo root = new DirectoryInfo(Application.dataPath + "/" + datasetPath);
            var dirs = root.GetDirectories();
            foreach (var dir in dirs)
            {
                Debug.Log(string.Format("Start processing textures of {0} / {1}", dir.Name, dirs.Length));
                var files = dir.GetFiles();
                foreach (var file in files)
                {
                    var path = "Assets/" + datasetPath + "/" + dir.Name + "/" + file.Name;
                    if (path.EndsWith("_mask.png"))
                    {
                        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                        if (importer.sRGBTexture != false)
                        {
                            importer.sRGBTexture = false;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                            EditorUtility.SetDirty(importer);
                            importer.SaveAndReimport();
                        }
                    }
                    else if (path.EndsWith("_normal.jpg"))
                    {
                        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                        if (importer.sRGBTexture != false || importer.textureType != TextureImporterType.NormalMap)
                        {
                            importer.textureType = TextureImporterType.NormalMap;
                            importer.sRGBTexture = false;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                            EditorUtility.SetDirty(importer);
                            importer.SaveAndReimport();
                        }
                    }
                }
            }

            Debug.Log("Processing textures end.");
        }

        [MenuItem("PCToolkit/Generate Target Renderers")]
        static void GenerateTargetRenderers()
        {
            DirectoryInfo root = new DirectoryInfo(Application.dataPath + "/" + datasetPath);
            var dirs = root.GetDirectories();
            Shader renderShader = Shader.Find("HDRP/Lit");
            Shader paramShader = Shader.Find("PCTK/BRDFInspec");
            var depthMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Depth.mat");
            foreach (var dir in dirs)
            {
                Debug.Log(string.Format("Start generating materials and rederers for {0}/{1}", dir.Name, dirs.Length));
                try
                {
                    var hasRenderMat = File.Exists(string.Format("{0}/{1}/{2}/{2}_render.mat", Application.dataPath, datasetPath, dir.Name));
                    var hasParamMat = File.Exists(string.Format("{0}/{1}/{2}/{2}_param.mat", Application.dataPath, datasetPath, dir.Name));
                    var hasPrefab = File.Exists(string.Format("{0}/{1}/{2}/{2}.prefab", Application.dataPath, datasetPath, dir.Name));
                    var albedoPath = string.Format("Assets/{0}/{1}/{1}_albedo.jpg", datasetPath, dir.Name);
                    var maskPath = string.Format("Assets/{0}/{1}/{1}_mask.png", datasetPath, dir.Name);
                    var normalPath = string.Format("Assets/{0}/{1}/{1}_normal.jpg", datasetPath, dir.Name);
                    var prefabPath = string.Format("Assets/{0}/{1}/{1}.prefab", datasetPath, dir.Name);
                    var shadingMatPath = string.Format("Assets/{0}/{1}/{1}_render.mat", datasetPath, dir.Name);
                    var paramMatPath = string.Format("Assets/{0}/{1}/{1}_param.mat", datasetPath, dir.Name);
                    var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
                    var maskMap = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
                    var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("Assets/{0}/{1}/{1}.fbx", datasetPath, dir.Name));

                    TargetRenderer targetRenderer = null;
                    Material shadingMat = null;
                    Material paramMat = null;

                    if (!hasRenderMat)
                    {
                        shadingMat = new Material(renderShader);

                        AssetDatabase.CreateAsset(shadingMat, shadingMatPath);
                        shadingMat.mainTexture = baseMap;
                        shadingMat.SetTexture("_MaskMap", maskMap);
                        shadingMat.SetTexture("_NormalMap", normalMap);
                        EditorUtility.SetDirty(shadingMat);
                    }
                    else
                    {
                        shadingMat = AssetDatabase.LoadAssetAtPath<Material>(shadingMatPath);
                    }

                    if (!hasParamMat)
                    {
                        paramMat = new Material(paramShader);
                        AssetDatabase.CreateAsset(paramMat, paramMatPath);
                        paramMat.SetTexture("_Albedo", baseMap);
                        paramMat.SetTexture("_Mask", maskMap);
                        paramMat.SetTexture("_NormalMap", normalMap);
                        paramMat.SetInt("_RenderMod", 1);
                        EditorUtility.SetDirty(paramMat);
                    }
                    else
                    {
                        paramMat = AssetDatabase.LoadAssetAtPath<Material>(paramMatPath);
                    }

                    AssetDatabase.SaveAssets();
                    if (!hasPrefab)
                    {
                        var go = new GameObject
                        {
                            name = dir.Name
                        };
                        targetRenderer = go.AddComponent<TargetRenderer>();
                        var modelGO = Instantiate(model, go.transform);
                        modelGO.name = model.name;
                        targetRenderer.meshes = modelGO.GetComponentsInChildren<MeshRenderer>();
                        PrefabUtility.SaveAsPrefabAsset(targetRenderer.gameObject, prefabPath);
                        DestroyImmediate(go);
                    }

                    using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
                    {

                        var go = editingScope.prefabContentsRoot;
                        targetRenderer = go.GetComponent<TargetRenderer>();
                        if (targetRenderer == null)
                        {
                            targetRenderer = go.AddComponent<TargetRenderer>();
                        }

                        if (targetRenderer.meshes == null || targetRenderer.meshes.Length == 0)
                        {
                            targetRenderer.meshes = targetRenderer.gameObject.GetComponentsInChildren<MeshRenderer>();
                        }

                        if (targetRenderer.meshes == null || targetRenderer.meshes.Length == 0)
                        {
                            var childCount = go.transform.childCount;
                            for (int i = childCount - 1; i >= 0; i--)
                            {
                                DestroyImmediate(go.transform.GetChild(i).gameObject);
                            }

                            var modelGO = Instantiate(model, go.transform);
                            modelGO.name = model.name;
                            targetRenderer.meshes = modelGO.GetComponentsInChildren<MeshRenderer>();
                        }

                        targetRenderer.depthMaterial = depthMat;
                        targetRenderer.shadingMaterial = shadingMat;
                        targetRenderer.paramMaterial = paramMat;
                        var bounds = new Bounds();
                        {
                            //Caculate bounds
                            Vector3 min = targetRenderer.meshes[0].bounds.min;
                            Vector3 max = targetRenderer.meshes[0].bounds.max;
                            foreach (var mesh in targetRenderer.meshes)
                            {
                                min = Vector3.Min(min, mesh.bounds.min);
                                max = Vector3.Max(max, mesh.bounds.max);
                            }

                            bounds.SetMinMax(min, max);
                        }

                        var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                        if (maxSize < 0.61f || maxSize > 1f)
                        {
                            //Adjust scale
                            var scale = 0.61f / maxSize;
                            var modelTrans = go.transform.GetChild(0);
                            modelTrans.localScale = Vector3.one * scale;
                            {
                                //Caculate new bounds
                                Vector3 min = targetRenderer.meshes[0].bounds.min;
                                Vector3 max = targetRenderer.meshes[0].bounds.max;
                                foreach (var mesh in targetRenderer.meshes)
                                {
                                    min = Vector3.Min(min, mesh.bounds.min);
                                    max = Vector3.Max(max, mesh.bounds.max);
                                }

                                bounds.SetMinMax(min, max);
                            }
                        }

                        targetRenderer.bounds = bounds;
                        PrefabUtility.SaveAsPrefabAsset(targetRenderer.gameObject, prefabPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("Generate material or renderer failed of {0}. Error: {1}\nStack Trace: {2}", dir.Name, e.Message, e.StackTrace));
                }
            }

            Debug.Log("Generating materials and renderers end.");
        }
    }
}

