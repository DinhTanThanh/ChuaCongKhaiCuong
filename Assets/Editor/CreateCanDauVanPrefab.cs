using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class CreateCanDauVanPrefab
{
    private const string FolderPath = "Assets/Prefabs/CanDauVan";
    private const string FbxPath = FolderPath + "/CanDauVan_Model.fbx";
    private const string MatPath = FolderPath + "/CanDauVan_Mat.mat";
    private const string ControllerPath = FolderPath + "/CanDauVan_Controller.controller";
    private const string PrefabPath = "Assets/Prefabs/CanDauVan.prefab";
    private const string InnerPrefabPath = FolderPath + "/CanDauVan.prefab";

    [MenuItem("Tools/Wukong/Tạo Prefab Cân Đẩu Vân (Create Prefab)")]
    [InitializeOnLoadMethod]
    public static void GeneratePrefab()
    {
        if (!File.Exists(FbxPath))
        {
            Debug.LogWarning("[CreateCanDauVanPrefab] Không tìm thấy file FBX tại: " + FbxPath);
            return;
        }

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        // 1. Cấu hình ModelImporter cho FBX (Bật Loop Animation)
        ModelImporter importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
        if (importer != null)
        {
            bool needReimport = false;
            if (!importer.importAnimation)
            {
                importer.importAnimation = true;
                needReimport = true;
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }

            if (clips != null && clips.Length > 0)
            {
                foreach (var clip in clips)
                {
                    if (!clip.loopTime)
                    {
                        clip.loopTime = true;
                        clip.loopPose = true;
                        needReimport = true;
                    }
                }
                importer.clipAnimations = clips;
            }

            if (needReimport)
            {
                importer.SaveAndReimport();
            }
        }

        // 2. Tạo Material chuẩn màu vàng Kintoun Dragon Ball (URP Lit)
        Material cloudMat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (cloudMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");

            cloudMat = new Material(shader);
            // Sắc vàng kim Kintoun: #FFCE38
            Color goldenYellow = new Color(1.00f, 0.80f, 0.15f, 1.0f);
            if (cloudMat.HasProperty("_BaseColor"))
                cloudMat.SetColor("_BaseColor", goldenYellow);
            else if (cloudMat.HasProperty("_Color"))
                cloudMat.SetColor("_Color", goldenYellow);

            // Độ bóng thấp (matte hoạt hình)
            if (cloudMat.HasProperty("_Smoothness"))
                cloudMat.SetFloat("_Smoothness", 0.15f);

            // Ánh sáng phát quang ấm nhẹ
            if (cloudMat.HasProperty("_EmissionColor"))
            {
                cloudMat.EnableKeyword("_EMISSION");
                cloudMat.SetColor("_EmissionColor", new Color(0.92f, 0.44f, 0.04f, 1.0f) * 0.12f);
            }

            AssetDatabase.CreateAsset(cloudMat, MatPath);
        }

        // 3. Tạo Animator Controller và gắn Animation Clip loop
        // Xóa controller cũ nếu bị lỗi missing state machine
        bool recreateController = false;
        AnimatorController animCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (animCtrl != null)
        {
            if (animCtrl.layers.Length == 0 || animCtrl.layers[0].stateMachine == null)
            {
                AssetDatabase.DeleteAsset(ControllerPath);
                animCtrl = null;
                recreateController = true;
            }
        }

        if (animCtrl == null)
        {
            animCtrl = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            recreateController = true;
        }

        // Tìm clip animation trong FBX
        AnimationClip animClip = null;
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
        foreach (var obj in allAssets)
        {
            if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
            {
                animClip = clip;
                break;
            }
        }

        // Đảm bảo StateMachine được khởi tạo và lưu chuẩn xác
        if (recreateController || animCtrl.layers.Length == 0 || animCtrl.layers[0].stateMachine == null)
        {
            AnimatorStateMachine sm = new AnimatorStateMachine();
            sm.name = "Base Layer";
            sm.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(sm, animCtrl);

            AnimatorControllerLayer[] layers = animCtrl.layers;
            if (layers == null || layers.Length == 0)
            {
                animCtrl.AddLayer("Base Layer");
                layers = animCtrl.layers;
            }

            layers[0].stateMachine = sm;
            layers[0].defaultWeight = 1f;
            animCtrl.layers = layers;

            if (animClip != null)
            {
                var state = sm.AddState("SwayAndBob");
                state.motion = animClip;
                sm.defaultState = state;
            }

            EditorUtility.SetDirty(animCtrl);
            AssetDatabase.SaveAssets();
        }
        else if (animClip != null)
        {
            var sm = animCtrl.layers[0].stateMachine;
            if (sm.states.Length == 0)
            {
                var state = sm.AddState("SwayAndBob");
                state.motion = animClip;
                sm.defaultState = state;
            }
            else
            {
                sm.states[0].state.motion = animClip;
            }
            EditorUtility.SetDirty(animCtrl);
            AssetDatabase.SaveAssets();
        }

        // 4. Instantiate mô hình và đóng gói thành Prefab hoàn chỉnh
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null) return;

        GameObject instance = Object.Instantiate(fbxAsset);
        instance.name = "CanDauVan";

        // Gán Animator Controller và Avatar
        Animator fbxAnim = fbxAsset.GetComponent<Animator>();
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = animCtrl;
        if (fbxAnim != null && fbxAnim.avatar != null)
        {
            animator.avatar = fbxAnim.avatar;
        }
        animator.applyRootMotion = false;

        // Gán Material vàng hoạt hình cho SkinnedMeshRenderer
        SkinnedMeshRenderer[] renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in renderers)
        {
            smr.sharedMaterial = cloudMat;
        }

        MeshRenderer[] meshRenderers = instance.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            mr.sharedMaterial = cloudMat;
        }

        // Lưu ra 2 đường dẫn để tiện kéo thả:
        // A. Assets/Prefabs/CanDauVan.prefab (Trực tiếp trong thư mục Prefabs)
        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);

        // B. Assets/Prefabs/CanDauVan/CanDauVan.prefab (Bên trong thư mục riêng)
        PrefabUtility.SaveAsPrefabAsset(instance, InnerPrefabPath);

        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CreateCanDauVanPrefab] ✅ Đã tạo thành công Prefab Cân Đẩu Vân tại: " + PrefabPath);
    }
}
