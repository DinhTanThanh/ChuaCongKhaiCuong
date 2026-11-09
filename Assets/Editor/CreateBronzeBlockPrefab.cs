using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateBronzeBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/BronzeBlock";
    private const string FbxPath = FolderPath + "/Bronze_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Bronze_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Bronze_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Bronze_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Aged_Bronze.mat";
    private const string PrefabPath = "Assets/Prefabs/Bronze_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Bronze_Block.prefab";

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

    [MenuItem("Tools/Environment/Tạo Prefab Khối Đồng Đúc 2x2x2 (Create Bronze_Block Prefab)")]
    public static void GeneratePrefab()
    {
        if (!File.Exists(FbxPath))
        {
            return;
        }

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        // 1. Đảm bảo Normal Map được import chuẩn NormalMap
        TextureImporter normalImporter = AssetImporter.GetAtPath(NormalPath) as TextureImporter;
        if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
        }

        // 2. Đảm bảo MetallicSmoothness Map dùng Linear (Non-sRGB) và giữ kênh Alpha cho Smoothness
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

        // 3. Tạo hoặc cập nhật Material Procedural_Aged_Bronze (URP Lit)
        Material bronzeMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (bronzeMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            bronzeMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (bronzeMat.HasProperty("_BaseMap"))
                bronzeMat.SetTexture("_BaseMap", albedoTex);
            if (bronzeMat.HasProperty("_MainTex"))
                bronzeMat.SetTexture("_MainTex", albedoTex);
            if (bronzeMat.HasProperty("_BaseColor"))
                bronzeMat.SetColor("_BaseColor", Color.white);
            if (bronzeMat.HasProperty("_Color"))
                bronzeMat.SetColor("_Color", Color.white);
        }

        if (msTex != null && bronzeMat.HasProperty("_MetallicGlossMap"))
        {
            bronzeMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            bronzeMat.SetTexture("_MetallicGlossMap", msTex);
            if (bronzeMat.HasProperty("_Metallic"))
                bronzeMat.SetFloat("_Metallic", 1.0f);
            if (bronzeMat.HasProperty("_Smoothness"))
                bronzeMat.SetFloat("_Smoothness", 1.0f);
            if (bronzeMat.HasProperty("_GlossMapScale"))
                bronzeMat.SetFloat("_GlossMapScale", 1.0f);
            if (bronzeMat.HasProperty("_SmoothnessTextureChannel"))
                bronzeMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (bronzeMat.HasProperty("_Metallic"))
                bronzeMat.SetFloat("_Metallic", 0.9f);
            if (bronzeMat.HasProperty("_Smoothness"))
                bronzeMat.SetFloat("_Smoothness", 0.7f);
        }

        if (normalTex != null && bronzeMat.HasProperty("_BumpMap"))
        {
            bronzeMat.EnableKeyword("_NORMALMAP");
            bronzeMat.SetTexture("_BumpMap", normalTex);
            if (bronzeMat.HasProperty("_BumpScale"))
                bronzeMat.SetFloat("_BumpScale", 1.15f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(bronzeMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(bronzeMat);
        }

        // 4. Tạo Prefab Bronze_Block (2m x 2m x 2m) kèm BoxCollider chuẩn kích thước
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Bronze_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = bronzeMat;
        }

        // Gắn BoxCollider 2x2x2m
        if (instance.GetComponent<Collider>() == null)
        {
            BoxCollider boxCol = instance.AddComponent<BoxCollider>();
            boxCol.center = Vector3.zero;
            boxCol.size = new Vector3(2.0f, 2.0f, 2.0f);
        }

        // Lưu ra cả 2 đường dẫn:
        // - Assets/Prefabs/Bronze_Block.prefab
        // - Assets/Prefabs/BronzeBlock/Bronze_Block.prefab
        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(instance, InnerPrefabPath);

        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
    }
}

