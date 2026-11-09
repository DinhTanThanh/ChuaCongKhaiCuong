using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateStoneArrowSlitWallPrefab
{
    private const string FolderPath = "Assets/Prefabs/StoneArrowSlitWall";
    private const string FbxPath = FolderPath + "/Stone_Arrow_Slit_Wall_Model.fbx";
    private const string PrefabPath = "Assets/Prefabs/Stone_Arrow_Slit_Wall.prefab";
    private const string InnerPrefabPath = FolderPath + "/Stone_Arrow_Slit_Wall.prefab";

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

    private static Material CreateOrUpdateSlitMaterial(string matName, string prefix, float defaultMetallic, float defaultSmoothness)
    {
        string albedoPath = $"{FolderPath}/StoneArrowSlit_{prefix}_Albedo.png";
        string normalPath = $"{FolderPath}/StoneArrowSlit_{prefix}_Normal.png";
        string msPath = $"{FolderPath}/StoneArrowSlit_{prefix}_MetallicSmoothness.png";
        string matPath = $"{FolderPath}/{matName}.mat";

        TextureImporter normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
        }

        TextureImporter msImporter = AssetImporter.GetAtPath(msPath) as TextureImporter;
        if (msImporter != null && (msImporter.sRGBTexture || msImporter.alphaSource != TextureImporterAlphaSource.FromInput))
        {
            msImporter.sRGBTexture = false;
            msImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            msImporter.SaveAndReimport();
        }

        Texture2D albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        Texture2D msTex = AssetDatabase.LoadAssetAtPath<Texture2D>(msPath);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        bool isNewMat = false;
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            isNewMat = true;
        }

        if (albedoTex != null)
        {
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", albedoTex);
            if (mat.HasProperty("_MainTex"))
                mat.SetTexture("_MainTex", albedoTex);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", Color.white);
        }

        if (msTex != null && mat.HasProperty("_MetallicGlossMap"))
        {
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            mat.SetTexture("_MetallicGlossMap", msTex);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", defaultMetallic);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 1.0f);
            if (mat.HasProperty("_GlossMapScale"))
                mat.SetFloat("_GlossMapScale", 1.0f);
            if (mat.HasProperty("_SmoothnessTextureChannel"))
                mat.SetFloat("_SmoothnessTextureChannel", 0.0f);
        }
        else
        {
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", defaultMetallic);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", defaultSmoothness);
        }

        if (normalTex != null && mat.HasProperty("_BumpMap"))
        {
            mat.EnableKeyword("_NORMALMAP");
            mat.SetTexture("_BumpMap", normalTex);
            if (mat.HasProperty("_BumpScale"))
                mat.SetFloat("_BumpScale", 1.0f);
        }

        if (isNewMat)
        {
            AssetDatabase.CreateAsset(mat, matPath);
        }
        else
        {
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    [MenuItem("Tools/Environment/Tạo Prefab Trụ Tường Đá Châu Mai (Create Stone_Arrow_Slit_Wall Prefab)")]
    public static void GeneratePrefab()
    {
        if (!File.Exists(FbxPath))
            return;

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        Material matBlocks  = CreateOrUpdateSlitMaterial("Procedural_Rough_Stone_SlitBlocks",  "Blocks",  0.0f,  0.18f);
        Material matDressed = CreateOrUpdateSlitMaterial("Procedural_Rough_Stone_SlitDressed", "Dressed", 0.0f,  0.26f);
        Material matMoss    = CreateOrUpdateSlitMaterial("Procedural_Rough_Stone_SlitMoss",    "Moss",    0.0f,  0.12f);
        Material matRecess  = CreateOrUpdateSlitMaterial("Procedural_Rough_Stone_SlitRecess",  "Recess",  0.0f,  0.10f);
        Material matScrolls = CreateOrUpdateSlitMaterial("Procedural_Rough_Stone_SlitScrolls", "Scrolls", 0.25f, 0.42f);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Stone_Arrow_Slit_Wall";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            int subMeshCount = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.subMeshCount : 5;
            if (subMeshCount >= 5)
            {
                mr.sharedMaterials = new Material[] { matBlocks, matDressed, matMoss, matRecess, matScrolls };
            }
            else if (subMeshCount == 4)
            {
                mr.sharedMaterials = new Material[] { matBlocks, matDressed, matMoss, matRecess };
            }
            else
            {
                mr.sharedMaterial = matBlocks;
            }
        }

        if (instance.GetComponent<Collider>() == null)
        {
            MeshFilter mf = instance.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                MeshCollider meshCol = instance.AddComponent<MeshCollider>();
                meshCol.sharedMesh = mf.sharedMesh;
            }
            else
            {
                BoxCollider boxCol = instance.AddComponent<BoxCollider>();
                boxCol.center = new Vector3(0.0f, 1.38f, 0.0f);
                boxCol.size = new Vector3(2.15f, 2.76f, 1.03f);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(instance, InnerPrefabPath);

        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        Debug.Log("[CreateStoneArrowSlitWallPrefab] Đã tạo thành công Prefab: " + PrefabPath);
    }
}

