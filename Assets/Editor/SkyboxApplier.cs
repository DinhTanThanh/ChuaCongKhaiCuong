using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor
{
    public static class SkyboxApplier
    {
        [MenuItem("Tools/Lighting/Apply Fantasy Kingdom Skybox (Bầu trời đẹp)")]
        public static void ApplyFKSkybox()
        {
            string skyboxMatPath = "Assets/Packet/env/FantasyKingdom/Art/URPHDRI/URPSkybBox.mat";
            Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyboxMatPath);

            if (skyMat == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Material Skybox tại: " + skyboxMatPath, "OK");
                return;
            }

            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();

            EditorUtility.DisplayDialog("Thành công!",
                "Đã áp dụng URPSkybBox làm bầu trời (Skybox) cho Scene hiện tại!", "Tuyệt vời");
        }

        [MenuItem("Tools/Lighting/Apply Nature Skybox")]
        public static void ApplyNatureSkybox()
        {
            string skyboxMatPath = "Assets/Packet/env/NatureStarterKit2/Materials/skybox.mat";
            Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyboxMatPath);

            if (skyMat == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Material Skybox tại: " + skyboxMatPath, "OK");
                return;
            }

            RenderSettings.skybox = skyMat;
            DynamicGI.UpdateEnvironment();

            EditorUtility.DisplayDialog("Thành công!",
                "Đã áp dụng Skybox Nature cho Scene hiện tại!", "Tuyệt vời");
        }
    }
}
