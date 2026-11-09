using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateWoodBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/WoodBlock";
    private const string FbxPath = FolderPath + "/Wood_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Wood_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Wood_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Wood_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Timber_Wood.mat";
    private const string PrefabPath = "Assets/Prefabs/Wood_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Wood_Block.prefab";

    [InitializeOnLoadMethod]
    private static void RegisterAutoGenerate()
    {
        if (File.Exists(PrefabPath) && File.Exists(InnerPrefabPath))
            return;

        EditorApplication.update += TryAutoGenerateWhenIdle;
    }

    private static void TryAutoGenerateWhenIdle()
    {
        // Chờ tới khi Unity hoàn tất 100% việc import FBX/Texture và không còn bận
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (!File.Exists(FbxPath))
        {
            EditorApplication.update -= TryAutoGenerateWhenIdle;
            return;
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null)
            return; // Chờ thêm 1 nhịp cho tới khi Unity import xong FBX

        EditorApplication.update -= TryAutoGenerateWhenIdle;

        if (!File.Exists(PrefabPath) || !File.Exists(InnerPrefabPath))
        {
            GeneratePrefab();
        }
    }

    [MenuItem("Tools/Environment/Tạo Prefab Khối Gỗ 2x2x2 (Create Wood_Block Prefab)")]
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

        Material woodMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (woodMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            woodMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (woodMat.HasProperty("_BaseMap"))
                woodMat.SetTexture("_BaseMap", albedoTex);
            if (woodMat.HasProperty("_MainTex"))
                woodMat.SetTexture("_MainTex", albedoTex);
            if (woodMat.HasProperty("_BaseColor"))
                woodMat.SetColor("_BaseColor", Color.white);
            if (woodMat.HasProperty("_Color"))
                woodMat.SetColor("_Color", Color.white);
        }

        if (msTex != null && woodMat.HasProperty("_MetallicGlossMap"))
        {
            woodMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            woodMat.SetTexture("_MetallicGlossMap", msTex);
            if (woodMat.HasProperty("_Metallic"))
                woodMat.SetFloat("_Metallic", 0.0f);
            if (woodMat.HasProperty("_Smoothness"))
                woodMat.SetFloat("_Smoothness", 1.0f);
            if (woodMat.HasProperty("_GlossMapScale"))
                woodMat.SetFloat("_GlossMapScale", 1.0f);
            if (woodMat.HasProperty("_SmoothnessTextureChannel"))
                woodMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (woodMat.HasProperty("_Metallic"))
                woodMat.SetFloat("_Metallic", 0.0f);
            if (woodMat.HasProperty("_Smoothness"))
                woodMat.SetFloat("_Smoothness", 0.35f);
        }

        if (normalTex != null && woodMat.HasProperty("_BumpMap"))
        {
            woodMat.EnableKeyword("_NORMALMAP");
            woodMat.SetTexture("_BumpMap", normalTex);
            if (woodMat.HasProperty("_BumpScale"))
                woodMat.SetFloat("_BumpScale", 1.15f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(woodMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(woodMat);
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Wood_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = woodMat;
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

