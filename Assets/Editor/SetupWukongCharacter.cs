using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public static class SetupWukongCharacter
{
    // BẢN PREFAB CHÍNH CỦA DỰ ÁN THEO YÊU CẦU CỦA USER:
    public const string MAIN_PREFAB_PATH = "Assets/Packet/Char/Sun_Wukong_Character.prefab";

    [MenuItem("Tools/Setup Sun Wukong Character (Sync Scene with Main Prefab)")]
    [InitializeOnLoadMethod]
    public static void Setup()
    {
        if (SessionState.GetBool("WukongSetupRunning", false)) return;

        try
        {
            SessionState.SetBool("WukongSetupRunning", true);

            string baseColorPath = "Assets/Sun_Wukong_Character/Textures/Wukong_BaseColor_4K.png";
            string normalPath = "Assets/Sun_Wukong_Character/Textures/Wukong_Normal_4K.png";
            string ormPath = "Assets/Sun_Wukong_Character/Textures/Wukong_ORM_4K.png";
            string metallicMapPath = "Assets/Sun_Wukong_Character/Textures/Wukong_MetallicSmoothness_4K.png";
            string fbxPath = "Assets/Sun_Wukong_Character/Sun_Wukong_Character.fbx";
            string matPath = "Assets/Sun_Wukong_Character/Sun_Wukong_PBR_Material.mat";
            string staffRedPath = "Assets/Sun_Wukong_Character/Mat_Staff_Red.mat";
            string staffGoldPath = "Assets/Sun_Wukong_Character/Mat_Staff_Gold.mat";
            string animPath = "Assets/Sun_Wukong_Character/Sun_Wukong_Idle.anim";
            string controllerPath = "Assets/Sun_Wukong_Character/Sun_Wukong_AnimatorController.controller";

            if (!File.Exists(fbxPath))
            {
                return;
            }
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceSynchronousImport);

            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(baseColorPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            Texture2D orm = AssetDatabase.LoadAssetAtPath<Texture2D>(ormPath);
            Texture2D metallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicMapPath);

            // 1. Vật liệu PBR nhân vật
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (baseColor != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", baseColor);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", baseColor);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

            if (normal != null)
            {
                if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normal);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 1.0f);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (metallicMap != null)
            {
                if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", metallicMap);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.EnableKeyword("_METALLICGLOSSMAP");
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 1.0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1.0f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 1.0f);
                if (mat.HasProperty("_SmoothnessTextureChannel")) mat.SetFloat("_SmoothnessTextureChannel", 0.0f);
            }
            else if (orm != null)
            {
                if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", orm);
                mat.EnableKeyword("_METALLICGLOSSMAP");
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.4f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.4f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.3f);
            }

            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0.0f);
            mat.doubleSidedGI = true;
            EditorUtility.SetDirty(mat);

            // 1b. Vật liệu Gậy Như Ý
            Shader litShader = mat.shader;

            Material staffRedMat = AssetDatabase.LoadAssetAtPath<Material>(staffRedPath);
            if (staffRedMat == null)
            {
                staffRedMat = new Material(litShader);
                AssetDatabase.CreateAsset(staffRedMat, staffRedPath);
            }
            if (staffRedMat.HasProperty("_BaseColor")) staffRedMat.SetColor("_BaseColor", new Color(0.55f, 0.08f, 0.08f, 1.0f));
            if (staffRedMat.HasProperty("_Color")) staffRedMat.SetColor("_Color", new Color(0.55f, 0.08f, 0.08f, 1.0f));
            if (staffRedMat.HasProperty("_Metallic")) staffRedMat.SetFloat("_Metallic", 0.25f);
            if (staffRedMat.HasProperty("_Smoothness")) staffRedMat.SetFloat("_Smoothness", 0.75f);
            EditorUtility.SetDirty(staffRedMat);

            Material staffGoldMat = AssetDatabase.LoadAssetAtPath<Material>(staffGoldPath);
            if (staffGoldMat == null)
            {
                staffGoldMat = new Material(litShader);
                AssetDatabase.CreateAsset(staffGoldMat, staffGoldPath);
            }
            if (staffGoldMat.HasProperty("_BaseColor")) staffGoldMat.SetColor("_BaseColor", new Color(0.85f, 0.70f, 0.22f, 1.0f));
            if (staffGoldMat.HasProperty("_Color")) staffGoldMat.SetColor("_Color", new Color(0.85f, 0.70f, 0.22f, 1.0f));
            if (staffGoldMat.HasProperty("_Metallic")) staffGoldMat.SetFloat("_Metallic", 0.95f);
            if (staffGoldMat.HasProperty("_Smoothness")) staffGoldMat.SetFloat("_Smoothness", 0.85f);
            EditorUtility.SetDirty(staffGoldMat);

            AssetDatabase.SaveAssets();

            // 2. ModelImporter Remap
            ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer != null)
            {
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach (var sub in subAssets)
                {
                    if (sub is Material subMat)
                    {
                        var sourceID = new AssetImporter.SourceAssetIdentifier(subMat);
                        if (subMat.name.Contains("Staff_Red")) importer.AddRemap(sourceID, staffRedMat);
                        else if (subMat.name.Contains("Staff_Gold")) importer.AddRemap(sourceID, staffGoldMat);
                        else importer.AddRemap(sourceID, mat);
                    }
                }
            }

            // 3. Trích xuất AnimationClip Idle từ FBX mới
            Object[] fbxAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            AnimationClip fbxClip = fbxAssets.OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));

            AnimationClip standaloneClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (fbxClip != null)
            {
                if (standaloneClip == null)
                {
                    standaloneClip = new AnimationClip();
                    EditorUtility.CopySerialized(fbxClip, standaloneClip);
                    standaloneClip.name = "Sun_Wukong_Idle";
                    var settings = AnimationUtility.GetAnimationClipSettings(standaloneClip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(standaloneClip, settings);
                    AssetDatabase.CreateAsset(standaloneClip, animPath);
                    Debug.Log("[SetupWukongCharacter] Đã tạo file AnimationClip Idle mới: " + animPath);
                }
                else
                {
                    EditorUtility.CopySerialized(fbxClip, standaloneClip);
                    standaloneClip.name = "Sun_Wukong_Idle";
                    var settings = AnimationUtility.GetAnimationClipSettings(standaloneClip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(standaloneClip, settings);
                    EditorUtility.SetDirty(standaloneClip);
                }
            }

            // 4. Cập nhật AnimatorController
            UnityEditor.Animations.AnimatorController controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
            if (controller == null || controller.layers.Length == 0 || controller.layers[0].stateMachine == null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
                controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            var rootStateMachine = controller.layers[0].stateMachine;
            var states = rootStateMachine.states;
            UnityEditor.Animations.AnimatorState idleState = null;
            foreach (var s in states)
            {
                if (s.state != null && s.state.name == "Idle")
                {
                    idleState = s.state;
                    break;
                }
            }
            if (idleState == null)
            {
                idleState = rootStateMachine.AddState("Idle");
            }
            rootStateMachine.defaultState = idleState;
            if (standaloneClip != null) idleState.motion = standaloneClip;
            else if (fbxClip != null) idleState.motion = fbxClip;

            EditorUtility.SetDirty(controller);

            // KIỂM TRA BẢN PREFAB CHÍNH:
            // Nếu Prefab chính đã có SkinnedMeshRenderer, CHÚNG TA KHÔNG GHI ĐÈ ĐỂ BẢO VỆ MỌI CHỈNH SỬA CỦA BẠN!
            // Chỉ cập nhật AnimatorController để Prefab chạy nhịp thở mới nhất!
            if (File.Exists(MAIN_PREFAB_PATH))
            {
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MAIN_PREFAB_PATH);
                if (existingPrefab != null)
                {
                    var existingSmr = existingPrefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    if (existingSmr != null && existingSmr.sharedMesh != null)
                    {
                        var anim = existingPrefab.GetComponent<Animator>();
                        if (anim != null && anim.runtimeAnimatorController != controller)
                        {
                            anim.runtimeAnimatorController = controller;
                            EditorUtility.SetDirty(existingPrefab);
                        }

                        SyncSceneInstanceWithMainPrefab(existingPrefab);
                        AssetDatabase.SaveAssets();
                        return;
                    }
                }
            }

            // 5. Nếu chưa có Prefab chính, tiến hành tạo mới từ FBX:
            GameObject fbxObj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxObj != null)
            {
                Mesh fbxMesh = null;
                var fbxSmr = fbxObj.GetComponentInChildren<SkinnedMeshRenderer>();
                if (fbxSmr != null) fbxMesh = fbxSmr.sharedMesh;

                GameObject instance = Object.Instantiate(fbxObj);
                instance.name = "Sun_Wukong_Character";

                SkinnedMeshRenderer smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    if (fbxMesh != null) smr.sharedMesh = fbxMesh;
                    smr.sharedMaterial = mat;
                    smr.quality = SkinQuality.Bone4;
                    smr.updateWhenOffscreen = true;
                }

                Animator anim = instance.GetComponent<Animator>();
                if (anim == null) anim = instance.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;
                anim.applyRootMotion = false;

                Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(fbxPath);
                if (avatar != null) anim.avatar = avatar;

                var staffRenderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var sr in staffRenderers)
                {
                    if (sr.name.Contains("Bang") || sr.name.Contains("Staff") || sr.name.Contains("Ruyi"))
                    {
                        sr.sharedMaterials = new Material[] { staffRedMat, staffGoldMat };
                    }
                }

                if (!Directory.Exists("Assets/Packet/Char"))
                {
                    Directory.CreateDirectory("Assets/Packet/Char");
                }

                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, MAIN_PREFAB_PATH);
                Object.DestroyImmediate(instance);
                Debug.Log("[SetupWukongCharacter] Đã tạo thành công Bản Prefab Chính: " + MAIN_PREFAB_PATH);

                if (savedPrefab != null)
                {
                    SyncSceneInstanceWithMainPrefab(savedPrefab);
                }
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            SessionState.SetBool("WukongSetupRunning", false);
        }
    }

    public static void SyncSceneInstanceWithMainPrefab(GameObject mainPrefab)
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        var roots = scene.GetRootGameObjects();
        GameObject sceneInstance = null;

        foreach (var r in roots)
        {
            if (r.name.Contains("Wukong"))
            {
                sceneInstance = r;
                break;
            }
        }

        Vector3 pos = sceneInstance != null ? sceneInstance.transform.position : new Vector3(3.5583f, 0f, -9.132f);
        Quaternion rot = sceneInstance != null ? sceneInstance.transform.rotation : Quaternion.identity;
        Vector3 scale = sceneInstance != null ? sceneInstance.transform.localScale : Vector3.one;
        Transform parent = sceneInstance != null ? sceneInstance.transform.parent : null;
        int siblingIndex = sceneInstance != null ? sceneInstance.transform.GetSiblingIndex() : 0;

        bool needsReconnect = false;
        if (sceneInstance == null)
        {
            needsReconnect = true;
        }
        else
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(sceneInstance);
            var status = PrefabUtility.GetPrefabInstanceStatus(sceneInstance);
            var smr = sceneInstance.GetComponentInChildren<SkinnedMeshRenderer>(true);

            if (source != mainPrefab || status != PrefabInstanceStatus.Connected || smr == null || smr.sharedMesh == null)
            {
                needsReconnect = true;
            }
        }

        if (needsReconnect)
        {
            if (sceneInstance != null)
            {
                Undo.DestroyObjectImmediate(sceneInstance);
            }

            GameObject newObj = PrefabUtility.InstantiatePrefab(mainPrefab, scene) as GameObject;
            if (newObj != null)
            {
                newObj.name = "Sun_Wukong_Character";
                newObj.transform.SetParent(parent);
                newObj.transform.position = pos;
                newObj.transform.rotation = rot;
                newObj.transform.localScale = scale;
                newObj.transform.SetSiblingIndex(siblingIndex);
                Undo.RegisterCreatedObjectUndo(newObj, "Connect Sun Wukong Main Prefab");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[SetupWukongCharacter] Đã liên kết thành công Scene với Bản Prefab Chính: " + MAIN_PREFAB_PATH);
            }
        }
    }

    [MenuItem("Tools/Advanced/Force Rebuild Main Prefab from Source FBX")]
    public static void ForceRebuildFromSource()
    {
        if (EditorUtility.DisplayDialog("Xác nhận làm mới Prefab chính",
            "Hành động này sẽ tái tạo lại toàn bộ file Assets/Packet/Char/Sun_Wukong_Character.prefab từ file gốc FBX. Bạn có chắc chắn muốn thực hiện không?",
            "Làm mới (Rebuild)", "Hủy"))
        {
            AssetDatabase.DeleteAsset(MAIN_PREFAB_PATH);
            Setup();
        }
    }
}
