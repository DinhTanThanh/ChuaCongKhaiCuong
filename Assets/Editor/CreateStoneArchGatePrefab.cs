using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class CreateStoneArchGatePrefab
{
    private const string FolderPath = "Assets/Prefabs/StoneArchGate";
    private const string FbxPath = FolderPath + "/Stone_Arch_Gate_Model.fbx";
    private const string ControllerPath = FolderPath + "/Stone_Arch_Gate_Animator.controller";
    private const string PrefabPath = "Assets/Prefabs/Stone_Arch_Gate.prefab";
    private const string InnerPrefabPath = FolderPath + "/Stone_Arch_Gate.prefab";

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

    private static Material CreateOrUpdateGateMaterial(string matName, string prefix, float defaultMetallic)
    {
        string albedoPath = $"{FolderPath}/StoneGate_{prefix}_Albedo.png";
        string normalPath = $"{FolderPath}/StoneGate_{prefix}_Normal.png";
        string msPath = $"{FolderPath}/StoneGate_{prefix}_MetallicSmoothness.png";
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
                mat.SetFloat("_Metallic", 1.0f);
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
                mat.SetFloat("_Smoothness", 0.25f);
        }

        if (normalTex != null && mat.HasProperty("_BumpMap"))
        {
            mat.EnableKeyword("_NORMALMAP");
            mat.SetTexture("_BumpMap", normalTex);
            if (mat.HasProperty("_BumpScale"))
                mat.SetFloat("_BumpScale", 1.15f);
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

    private static AnimatorController CreateOrUpdateGateAnimatorController()
    {
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
        AnimationClip openClip = null;
        AnimationClip closeClip = null;
        AnimationClip demoClip = null;

        foreach (Object asset in allAssets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
            {
                if (clip.name.EndsWith("Open_Door"))
                    openClip = clip;
                else if (clip.name.EndsWith("Close_Door"))
                    closeClip = clip;
                else if (clip.name.EndsWith("Door_Open_Close_Demo"))
                    demoClip = clip;
            }
        }

        if (File.Exists(ControllerPath))
        {
            AssetDatabase.DeleteAsset(ControllerPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("isOpen", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Open", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Close", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorState closedState = sm.AddState("Closed_Idle", new Vector3(280, 120, 0));
        closedState.motion = openClip;
        closedState.speed = 0.0f;
        sm.defaultState = closedState;

        AnimatorState openState = sm.AddState("Open_Door", new Vector3(560, 40, 0));
        openState.motion = openClip;

        AnimatorState closeState = sm.AddState("Close_Door", new Vector3(560, 200, 0));
        closeState.motion = closeClip;

        if (demoClip != null)
        {
            AnimatorState demoState = sm.AddState("Door_Open_Close_Demo", new Vector3(280, 260, 0));
            demoState.motion = demoClip;
        }

        // Transitions via bool isOpen
        AnimatorStateTransition toOpenBool = closedState.AddTransition(openState);
        toOpenBool.AddCondition(AnimatorConditionMode.If, 0, "isOpen");
        toOpenBool.hasExitTime = false;
        toOpenBool.duration = 0.1f;

        AnimatorStateTransition toCloseBool = openState.AddTransition(closeState);
        toCloseBool.AddCondition(AnimatorConditionMode.IfNot, 0, "isOpen");
        toCloseBool.hasExitTime = false;
        toCloseBool.duration = 0.1f;

        AnimatorStateTransition reOpenBool = closeState.AddTransition(openState);
        reOpenBool.AddCondition(AnimatorConditionMode.If, 0, "isOpen");
        reOpenBool.hasExitTime = false;
        reOpenBool.duration = 0.1f;

        // Transitions via AnyState Triggers ("Open" / "Close")
        AnimatorStateTransition anyToOpen = sm.AddAnyStateTransition(openState);
        anyToOpen.AddCondition(AnimatorConditionMode.If, 0, "Open");
        anyToOpen.hasExitTime = false;
        anyToOpen.duration = 0.1f;

        AnimatorStateTransition anyToClose = sm.AddAnyStateTransition(closeState);
        anyToClose.AddCondition(AnimatorConditionMode.If, 0, "Close");
        anyToClose.hasExitTime = false;
        anyToClose.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        return controller;
    }

    [MenuItem("Tools/Environment/Tạo Prefab Cổng Vòm Đá Mở-Đóng Cửa (Create Stone_Arch_Gate Prefab)")]
    public static void GeneratePrefab()
    {
        if (!File.Exists(FbxPath))
            return;

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        Material matWall = CreateOrUpdateGateMaterial("Procedural_Rough_Stone_GateWall", "Wall", 0.0f);
        Material matArch = CreateOrUpdateGateMaterial("Procedural_Rough_Stone_ArchMoss", "Arch", 0.0f);
        Material matDoor = CreateOrUpdateGateMaterial("Procedural_Rough_Stone_DoorPlank", "Door", 0.0f);
        Material matStrap = CreateOrUpdateGateMaterial("Procedural_Rough_Stone_Strap", "Strap", 0.25f);

        AnimatorController controller = CreateOrUpdateGateAnimatorController();

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "Stone_Arch_Gate";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        Material[] gateMats = new Material[] { matWall, matArch, matDoor, matStrap };

        SkinnedMeshRenderer[] skinnedRenderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinnedRenderers)
        {
            int count = (smr.sharedMesh != null) ? smr.sharedMesh.subMeshCount : 4;
            if (count >= 4)
                smr.sharedMaterials = gateMats;
            else
                smr.sharedMaterial = matWall;
        }

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in meshRenderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            int count = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.subMeshCount : 4;
            if (count >= 4)
                mr.sharedMaterials = gateMats;
            else
                mr.sharedMaterial = matWall;
        }

        Animator animator = instance.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }

        if (instance.GetComponent<StoneArchGateDoor>() == null)
        {
            instance.AddComponent<StoneArchGateDoor>();
        }

        // Add accurate BoxColliders for Left Wall Pillar, Right Wall Pillar, Top Arch Lintel, and Swinging Doors!
        BoxCollider leftWallCol = instance.AddComponent<BoxCollider>();
        leftWallCol.center = new Vector3(-1.70f, 1.58f, 0.0f);
        leftWallCol.size = new Vector3(1.24f, 3.16f, 0.72f);

        BoxCollider rightWallCol = instance.AddComponent<BoxCollider>();
        rightWallCol.center = new Vector3(1.70f, 1.58f, 0.0f);
        rightWallCol.size = new Vector3(1.24f, 3.16f, 0.72f);

        BoxCollider topArchCol = instance.AddComponent<BoxCollider>();
        topArchCol.center = new Vector3(0.0f, 3.18f, 0.0f);
        topArchCol.size = new Vector3(2.16f, 0.48f, 0.74f);

        // Attach swinging door BoxColliders directly to Door_L and Door_R bones so collision moves when doors open!
        Transform[] allTransforms = instance.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "Door_L" && t.GetComponent<Collider>() == null)
            {
                BoxCollider dlCol = t.gameObject.AddComponent<BoxCollider>();
                dlCol.center = new Vector3(0.51f, 1.40f, 0.0f);
                dlCol.size = new Vector3(1.02f, 2.80f, 0.12f);
            }
            else if (t.name == "Door_R" && t.GetComponent<Collider>() == null)
            {
                BoxCollider drCol = t.gameObject.AddComponent<BoxCollider>();
                drCol.center = new Vector3(-0.51f, 1.40f, 0.0f);
                drCol.size = new Vector3(1.02f, 2.80f, 0.12f);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(instance, InnerPrefabPath);

        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
    }
}

