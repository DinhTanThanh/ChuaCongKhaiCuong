using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateStoneBlockPrefab
{
    private const string FolderPath = "Assets/Prefabs/StoneBlock";
    private const string FbxPath = FolderPath + "/Stone_Block_Model.fbx";
    private const string AlbedoPath = FolderPath + "/Stone_Block_Albedo.png";
    private const string NormalPath = FolderPath + "/Stone_Block_Normal.png";
    private const string MetallicSmoothnessPath = FolderPath + "/Stone_Block_MetallicSmoothness.png";
    private const string MatPath = FolderPath + "/Procedural_Rough_Stone.mat";
    private const string PrefabPath = "Assets/Prefabs/Stone_Block.prefab";
    private const string InnerPrefabPath = FolderPath + "/Stone_Block.prefab";

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

    [MenuItem("Tools/Environment/Tạo Prefab Khối Đá Xây Thành 2x2x2 (Create Stone_Block Prefab)")]
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

        // 1. Đảm bảo Normal Map được import đúng chuẩn NormalMap trong Unity
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

        // 3. Tạo hoặc cập nhật Material Procedural_Rough_Stone (URP Lit)
        Material stoneMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        bool isNewMat = false;
        if (stoneMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            stoneMat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (stoneMat.HasProperty("_BaseMap"))
                stoneMat.SetTexture("_BaseMap", albedoTex);
            if (stoneMat.HasProperty("_MainTex"))
                stoneMat.SetTexture("_MainTex", albedoTex);
            if (stoneMat.HasProperty("_BaseColor"))
                stoneMat.SetColor("_BaseColor", Color.white);
            if (stoneMat.HasProperty("_Color"))
                stoneMat.SetColor("_Color", Color.white);
        }

        if (msTex != null && stoneMat.HasProperty("_MetallicGlossMap"))
        {
            stoneMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            stoneMat.SetTexture("_MetallicGlossMap", msTex);
            if (stoneMat.HasProperty("_Metallic"))
                stoneMat.SetFloat("_Metallic", 0.0f);
            if (stoneMat.HasProperty("_Smoothness"))
                stoneMat.SetFloat("_Smoothness", 1.0f);
            if (stoneMat.HasProperty("_GlossMapScale"))
                stoneMat.SetFloat("_GlossMapScale", 1.0f);
            if (stoneMat.HasProperty("_SmoothnessTextureChannel"))
                stoneMat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (stoneMat.HasProperty("_Smoothness"))
                stoneMat.SetFloat("_Smoothness", 0.15f);
            if (stoneMat.HasProperty("_Glossiness"))
                stoneMat.SetFloat("_Glossiness", 0.15f);
        }

        if (normalTex != null && stoneMat.HasProperty("_BumpMap"))
        {
            stoneMat.EnableKeyword("_NORMALMAP");
            stoneMat.SetTexture("_BumpMap", normalTex);
            if (stoneMat.HasProperty("_BumpScale"))
                stoneMat.SetFloat("_BumpScale", 1.25f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(stoneMat, MatPath);
        }
        else
        {
            EditorUtility.SetDirty(stoneMat);
        }

        // 4. Tạo Prefab Stone_Block (2m x 2m x 2m) kèm BoxCollider chuẩn kích thước
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Stone_Block";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = stoneMat;
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
