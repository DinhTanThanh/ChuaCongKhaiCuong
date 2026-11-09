using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateBrickBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/BrickBlock";
    private const string FbxPath = FolderPath + "/Brick_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Brick_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Brick_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Brick_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Fired_Brick.mat";
    private const string PrefabPath = "Assets/Prefabs/Brick_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Brick_Block.prefab";

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

    [MenuItem("Tools/Environment/Tạo Prefab Khối Gạch Nung 2x2x2 (Create Brick_Block Prefab)")]
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

        Material brickMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (brickMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            brickMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (brickMat.HasProperty("_BaseMap"))
                brickMat.SetTexture("_BaseMap", albedoTex);
            if (brickMat.HasProperty("_MainTex"))
                brickMat.SetTexture("_MainTex", albedoTex);
            if (brickMat.HasProperty("_BaseColor"))
                brickMat.SetColor("_BaseColor", Color.white);
            if (brickMat.HasProperty("_Color"))
                brickMat.SetColor("_Color", Color.white);
        }

        if (msTex != null && brickMat.HasProperty("_MetallicGlossMap"))
        {
            brickMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            brickMat.SetTexture("_MetallicGlossMap", msTex);
            if (brickMat.HasProperty("_Metallic"))
                brickMat.SetFloat("_Metallic", 0.0f);
            if (brickMat.HasProperty("_Smoothness"))
                brickMat.SetFloat("_Smoothness", 1.0f);
            if (brickMat.HasProperty("_GlossMapScale"))
                brickMat.SetFloat("_GlossMapScale", 1.0f);
            if (brickMat.HasProperty("_SmoothnessTextureChannel"))
                brickMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (brickMat.HasProperty("_Metallic"))
                brickMat.SetFloat("_Metallic", 0.0f);
            if (brickMat.HasProperty("_Smoothness"))
                brickMat.SetFloat("_Smoothness", 0.25f);
        }

        if (normalTex != null && brickMat.HasProperty("_BumpMap"))
        {
            brickMat.EnableKeyword("_NORMALMAP");
            brickMat.SetTexture("_BumpMap", normalTex);
            if (brickMat.HasProperty("_BumpScale"))
                brickMat.SetFloat("_BumpScale", 1.3f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(brickMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(brickMat);
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Brick_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = brickMat;
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

