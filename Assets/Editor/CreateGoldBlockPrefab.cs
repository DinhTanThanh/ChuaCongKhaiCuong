using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateGoldBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/GoldBlock";
    private const string FbxPath = FolderPath + "/Gold_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Gold_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Gold_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Gold_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Royal_Gold.mat";
    private const string PrefabPath = "Assets/Prefabs/Gold_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Gold_Block.prefab";

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

    [MenuItem("Tools/Environment/Tạo Prefab Khối Vàng Hoàng Gia 2x2x2 (Create Gold_Block Prefab)")]
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

        Material goldMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (goldMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            goldMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (goldMat.HasProperty("_BaseMap"))
                goldMat.SetTexture("_BaseMap", albedoTex);
            if (goldMat.HasProperty("_MainTex"))
                goldMat.SetTexture("_MainTex", albedoTex);
            if (goldMat.HasProperty("_BaseColor"))
                goldMat.SetColor("_BaseColor", Color.white);
            if (goldMat.HasProperty("_Color"))
                goldMat.SetColor("_Color", Color.white);
        }

        if (msTex != null && goldMat.HasProperty("_MetallicGlossMap"))
        {
            goldMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            goldMat.SetTexture("_MetallicGlossMap", msTex);
            if (goldMat.HasProperty("_Metallic"))
                goldMat.SetFloat("_Metallic", 1.0f);
            if (goldMat.HasProperty("_Smoothness"))
                goldMat.SetFloat("_Smoothness", 1.0f);
            if (goldMat.HasProperty("_GlossMapScale"))
                goldMat.SetFloat("_GlossMapScale", 1.0f);
            if (goldMat.HasProperty("_SmoothnessTextureChannel"))
                goldMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (goldMat.HasProperty("_Metallic"))
                goldMat.SetFloat("_Metallic", 1.0f);
            if (goldMat.HasProperty("_Smoothness"))
                goldMat.SetFloat("_Smoothness", 0.85f);
        }

        if (normalTex != null && goldMat.HasProperty("_BumpMap"))
        {
            goldMat.EnableKeyword("_NORMALMAP");
            goldMat.SetTexture("_BumpMap", normalTex);
            if (goldMat.HasProperty("_BumpScale"))
                goldMat.SetFloat("_BumpScale", 1.15f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(goldMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(goldMat);
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Gold_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = goldMat;
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

