using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Cinemachine;

namespace ProjectTools.Editor
{
    [InitializeOnLoad]
    public static class CameraPerspectiveSetupTool
    {
        static CameraPerspectiveSetupTool()
        {
            EditorApplication.delayCall += AutoSetupIfMissing;
        }

        private static void AutoSetupIfMissing()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (Object.FindFirstObjectByType<CameraPerspectiveManager>() == null)
            {
                SetupCameraSystem();
            }
        }

        [MenuItem("Tools/Camera/Setup Camera Perspective System (FPP - TPP)")]
        public static void SetupCameraSystem()
        {
            // 1. Tìm hoặc tạo GameObject [CameraSystem]
            CameraPerspectiveManager manager = Object.FindFirstObjectByType<CameraPerspectiveManager>();
            GameObject cameraSystemObj = null;

            if (manager == null)
            {
                cameraSystemObj = new GameObject("[CameraSystem]");
                Undo.RegisterCreatedObjectUndo(cameraSystemObj, "Create CameraSystem");
                manager = cameraSystemObj.AddComponent<CameraPerspectiveManager>();
            }
            else
            {
                cameraSystemObj = manager.gameObject;
            }

            SerializedObject serializedManager = new SerializedObject(manager);

            // 2. Tìm Main Camera và CinemachineBrain
            Camera mainCam = Camera.main;
            if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();

            CinemachineBrain brain = null;
            if (mainCam != null)
            {
                brain = mainCam.GetComponent<CinemachineBrain>();
                serializedManager.FindProperty("mainCamera").objectReferenceValue = mainCam;
            }

            if (brain != null)
            {
                serializedManager.FindProperty("cinemachineBrain").objectReferenceValue = brain;
            }

            // 3. Tìm CinemachineCamera (TPP)
            CinemachineCamera tppCam = Object.FindFirstObjectByType<CinemachineCamera>();
            if (tppCam != null)
            {
                serializedManager.FindProperty("tppCinemachineCamera").objectReferenceValue = tppCam;
            }

            // 4. Tìm nhân vật người chơi trong Scene (Wukong hoặc BaseController)
            PlayerCameraTarget cameraTarget = Object.FindFirstObjectByType<PlayerCameraTarget>();
            if (cameraTarget == null)
            {
                BaseController controller = Object.FindFirstObjectByType<BaseController>();
                GameObject playerObj = controller != null ? controller.gameObject : GameObject.Find("Wukong");
                if (playerObj == null)
                {
                    playerObj = GameObject.FindWithTag("Player");
                }

                if (playerObj != null)
                {
                    cameraTarget = playerObj.GetComponent<PlayerCameraTarget>();
                    if (cameraTarget == null)
                    {
                        cameraTarget = Undo.AddComponent<PlayerCameraTarget>(playerObj);
                    }
                }
            }

            if (cameraTarget != null)
            {
                serializedManager.FindProperty("currentTarget").objectReferenceValue = cameraTarget;
                EditorUtility.SetDirty(cameraTarget);
            }

            serializedManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
                EditorSceneManager.SaveScene(manager.gameObject.scene);
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"<color=green>[CameraPerspectiveSetupTool] Thiết lập hệ thống Camera FPP / TPP thành công!</color>\n" +
                      $"- Manager: {cameraSystemObj.name}\n" +
                      $"- Main Camera: {(mainCam != null ? mainCam.name : "None")}\n" +
                      $"- TPP CinemachineCamera: {(tppCam != null ? tppCam.name : "None")}\n" +
                      $"- Current Target: {(cameraTarget != null ? cameraTarget.gameObject.name : "Chưa có (sẽ tự động nhận diện khi Play)")}\n" +
                      $"=> Nhấn phím 'V' trong lúc chơi để chuyển góc nhìn!");
        }
    }
}
