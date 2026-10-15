using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SetupWukongFlight
{
    [MenuItem("Tools/Wukong/Setup Cân Đẩu Vân (WukongFly)")]
    [InitializeOnLoadMethod]
    public static void SetupFlight()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.isLoaded) return;

        GameObject wukongObj = GameObject.Find("Wukong");
        if (wukongObj == null)
        {
            WukongController ctrl = Object.FindFirstObjectByType<WukongController>();
            if (ctrl != null) wukongObj = ctrl.gameObject;
        }

        if (wukongObj == null) return;

        bool isModified = false;

        Transform flyingTrans = wukongObj.transform.Find("Flying");
        GameObject flyingObj;
        if (flyingTrans == null)
        {
            flyingObj = new GameObject("Flying");
            flyingObj.transform.SetParent(wukongObj.transform);
            flyingObj.transform.localPosition = Vector3.zero;
            flyingObj.transform.localRotation = Quaternion.identity;
            flyingObj.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(flyingObj, "Create Flying GameObject");
            isModified = true;
        }
        else
        {
            flyingObj = flyingTrans.gameObject;
        }

        WukongFly fly = flyingObj.GetComponent<WukongFly>();
        if (fly == null)
        {
            fly = Undo.AddComponent<WukongFly>(flyingObj);
            isModified = true;
        }

        WukongController controller = wukongObj.GetComponent<WukongController>();
        if (controller != null)
        {
            var serializedCtrl = new SerializedObject(controller);
            var flyProp = serializedCtrl.FindProperty("wukongFly");
            if (flyProp != null && flyProp.objectReferenceValue != fly)
            {
                flyProp.objectReferenceValue = fly;
                serializedCtrl.ApplyModifiedProperties();
                isModified = true;
            }
        }

        Animator anim = wukongObj.GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            string path = AssetDatabase.GetAssetPath(anim.runtimeAnimatorController);
            if (!string.IsNullOrEmpty(path))
            {
                AnimatorController animCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (animCtrl != null)
                {
                    bool hasIsFlying = false;
                    foreach (var p in animCtrl.parameters)
                    {
                        if (p.name == "IsFlying")
                        {
                            hasIsFlying = true;
                            break;
                        }
                    }
                    if (!hasIsFlying)
                    {
                        animCtrl.AddParameter("IsFlying", AnimatorControllerParameterType.Bool);
                        EditorUtility.SetDirty(animCtrl);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        // Cấu hình Cân Đẩu Vân mới (CanDauVan.prefab)
        GameObject canDauVanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CanDauVan.prefab");
        if (canDauVanPrefab == null)
        {
            canDauVanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CanDauVan/CanDauVan.prefab");
        }

        // Kiểm tra xem Flying đã có CanDauVan chưa
        Transform existingCanDauVan = flyingObj.transform.Find("CanDauVan");
        Transform oldCloud = flyingObj.transform.Find("SomersaultCloud_VFX");

        if (existingCanDauVan == null && canDauVanPrefab != null)
        {
            GameObject newCloud = (GameObject)PrefabUtility.InstantiatePrefab(canDauVanPrefab, flyingObj.transform);
            newCloud.name = "CanDauVan";
            // Đặt vị trí dưới chân Wukong và xoay đuôi hướng ra sau
            newCloud.transform.localPosition = new Vector3(0f, -0.6f, 0.25f);
            newCloud.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            newCloud.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            existingCanDauVan = newCloud.transform;
            Undo.RegisterCreatedObjectUndo(newCloud, "Instantiate CanDauVan");
            isModified = true;
        }
        else if (existingCanDauVan != null)
        {
            existingCanDauVan.localPosition = new Vector3(0f, -0.6f, 0.25f);
            existingCanDauVan.localRotation = Quaternion.Euler(0f, -90f, 0f);
            existingCanDauVan.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            isModified = true;
        }

        // Nếu có mây cũ thì ẩn hoặc xóa mây cũ
        if (oldCloud != null && existingCanDauVan != null)
        {
            oldCloud.gameObject.SetActive(false);
            isModified = true;
        }

        // Gán cloudVfx vào WukongFly
        if (existingCanDauVan != null)
        {
            var serializedFly = new SerializedObject(fly);
            var cloudProp = serializedFly.FindProperty("cloudVfx");
            if (cloudProp != null && cloudProp.objectReferenceValue != existingCanDauVan.gameObject)
            {
                cloudProp.objectReferenceValue = existingCanDauVan.gameObject;
                serializedFly.ApplyModifiedProperties();
                isModified = true;
            }

            // Mặc định ẩn mây khi chưa bay
            if (existingCanDauVan.gameObject.activeSelf)
            {
                existingCanDauVan.gameObject.SetActive(false);
                isModified = true;
            }
        }

        if (isModified)
        {
            EditorUtility.SetDirty(flyingObj);
            EditorUtility.SetDirty(wukongObj);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SetupWukongFlight] ✅ Đã gắn thành công Cân Đẩu Vân mới (CanDauVan) cho Wukong!");
        }
    }
}
