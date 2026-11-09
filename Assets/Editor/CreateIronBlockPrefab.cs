using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateIronBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/IronBlock";
    private const string FbxPath = FolderPath + "/Iron_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Iron_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Iron_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Iron_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Forged_Iron.mat";
    private const string PrefabPath = "Assets/Prefabs/Iron_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Iron_Block.prefab";

    [InitializeOnLoadMethod]
    private static void RegisterAutoGenerate()
    {
        if (File.Exists(PrefabPath) && File.Exists(InnerPrefabPath))
            return;

        EditorApplication.update += TryAutoGenerateWhenIdle;
    }

    private static void TryAutoGenerateWhenIdle()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (!File.Exists(FbxPath))
        {
            EditorApplication.update -= TryAutoGenerateWhenIdle;
            return;
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null)
            return;

        EditorApplication.update -= TryAutoGenerateWhenIdle;

        if (!File.Exists(PrefabPath) || !File.Exists(InnerPrefabPath))
        {
            GeneratePrefab();
        }
    }

    [MenuItem("Tools/Environment/Tạo Prefab Khối Sắt Rèn 2x2x2 (Create Iron_Block Prefab)")]
    public static void GeneratePrefab()
    {
        if (!File.Exists(FbxPath))
            return;

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        TextureImporter normalImporter = AssetImporter.GetAtPath(NormalPath) as TextureImporter;
        if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
        }

        TextureImporter msImporter = AssetImporter.GetAtPath(MetallicSmoothnessPath) as TextureImporter;
        if (msImporter != null && (msImporter.sRGBTexture || msImporter.alphaSource != TextureImporterAlphaSource.FromInput))
        {
            msImporter.sRGBTexture = false;
            msImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            msImporter.SaveAndReimport();
        }

        Texture2D albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
        Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath);
        Texture2D msTex = AssetDatabase.LoadAssetAtPath<Texture2D>(MetallicSmoothnessPath);

        Material ironMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (ironMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            ironMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (ironMat.HasProperty("_BaseMap"))
                ironMat.SetTexture("_BaseMap", albedoTex);
            if (ironMat.HasProperty("_MainTex"))
                ironMat.SetTexture("_MainTex", albedoTex);
            if (ironMat.HasProperty("_BaseColor"))
                ironMat.SetColor("_BaseColor", Color.white);
            if (ironMat.HasProperty("_Color"))
                ironMat.SetColor("_Color", Color.white);
        }

        if (msTex != null && ironMat.HasProperty("_MetallicGlossMap"))
        {
            ironMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            ironMat.SetTexture("_MetallicGlossMap", msTex);
            if (ironMat.HasProperty("_Metallic"))
                ironMat.SetFloat("_Metallic", 1.0f);
            if (ironMat.HasProperty("_Smoothness"))
                ironMat.SetFloat("_Smoothness", 1.0f);
            if (ironMat.HasProperty("_GlossMapScale"))
                ironMat.SetFloat("_GlossMapScale", 1.0f);
            if (ironMat.HasProperty("_SmoothnessTextureChannel"))
                ironMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (ironMat.HasProperty("_Metallic"))
                ironMat.SetFloat("_Metallic", 0.9f);
            if (ironMat.HasProperty("_Smoothness"))
                ironMat.SetFloat("_Smoothness", 0.72f);
        }

        if (normalTex != null && ironMat.HasProperty("_BumpMap"))
        {
            ironMat.EnableKeyword("_NORMALMAP");
            ironMat.SetTexture("_BumpMap", normalTex);
            if (ironMat.HasProperty("_BumpScale"))
                ironMat.SetFloat("_BumpScale", 1.2f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(ironMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(ironMat);
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Iron_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = ironMat;
        }

        if (instance.GetComponent<Collider>() == null)
        {
            BoxCollider boxCol = instance.AddComponent<BoxCollider>();
            boxCol.center = Vector3.zero;
            boxCol.size = new Vector3(2.0f, 2.0f, 2.0f);
        }

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(instance, InnerPrefabPath);

        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
    }
}

