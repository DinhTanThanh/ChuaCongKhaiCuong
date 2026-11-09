using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateDiamondBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/DiamondBlock";
    private const string FbxPath = FolderPath + "/Diamond_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Diamond_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Diamond_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Diamond_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Crystalline_Diamond.mat";
    private const string PrefabPath = "Assets/Prefabs/Diamond_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Diamond_Block.prefab";

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

    [MenuItem("Tools/Environment/Tạo Prefab Khối Kim Cương 2x2x2 (Create Diamond_Block Prefab)")]
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

        Material diamondMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (diamondMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            diamondMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (diamondMat.HasProperty("_BaseMap"))
                diamondMat.SetTexture("_BaseMap", albedoTex);
            if (diamondMat.HasProperty("_MainTex"))
                diamondMat.SetTexture("_MainTex", albedoTex);
            if (diamondMat.HasProperty("_BaseColor"))
                diamondMat.SetColor("_BaseColor", Color.white);
            if (diamondMat.HasProperty("_Color"))
                diamondMat.SetColor("_Color", Color.white);

            // Subtle internal crystalline emission glow so the Diamond tier stands out even in shadows
            if (diamondMat.HasProperty("_EmissionMap") && diamondMat.HasProperty("_EmissionColor"))
            {
                diamondMat.EnableKeyword("_EMISSION");
                diamondMat.SetTexture("_EmissionMap", albedoTex);
                diamondMat.SetColor("_EmissionColor", new Color(0.10f, 0.22f, 0.28f, 1.0f));
            }
        }

        if (msTex != null && diamondMat.HasProperty("_MetallicGlossMap"))
        {
            diamondMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            diamondMat.SetTexture("_MetallicGlossMap", msTex);
            if (diamondMat.HasProperty("_Metallic"))
                diamondMat.SetFloat("_Metallic", 1.0f);
            if (diamondMat.HasProperty("_Smoothness"))
                diamondMat.SetFloat("_Smoothness", 1.0f);
            if (diamondMat.HasProperty("_GlossMapScale"))
                diamondMat.SetFloat("_GlossMapScale", 1.0f);
            if (diamondMat.HasProperty("_SmoothnessTextureChannel"))
                diamondMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (diamondMat.HasProperty("_Metallic"))
                diamondMat.SetFloat("_Metallic", 0.45f);
            if (diamondMat.HasProperty("_Smoothness"))
                diamondMat.SetFloat("_Smoothness", 0.95f);
        }

        if (normalTex != null && diamondMat.HasProperty("_BumpMap"))
        {
            diamondMat.EnableKeyword("_NORMALMAP");
            diamondMat.SetTexture("_BumpMap", normalTex);
            if (diamondMat.HasProperty("_BumpScale"))
                diamondMat.SetFloat("_BumpScale", 1.1f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(diamondMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(diamondMat);
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Diamond_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = diamondMat;
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

