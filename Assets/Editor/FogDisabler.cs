using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectTools.Editor
{
    public static class FogDisabler
    {
        [MenuItem("Tools/Lighting/Toggle Fog (Bật hoặc Tắt Sương Mù)")]
        public static void ToggleFog()
        {
            int modifiedCount = 0;
            string[] rendererGuids = AssetDatabase.FindAssets("t:ScriptableRendererData", new[] { "Assets/Packet/env/FantasyKingdom/URP/Settings", "Assets/Settings" });

            bool targetState = false; // Mặc định tắt sương mù cho trong trẻo

            foreach (var guid in rendererGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
                if (rendererData == null) continue;

                var prop = typeof(ScriptableRendererData).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop == null) continue;

                var features = prop.GetValue(rendererData) as List<ScriptableRendererFeature>;
                if (features == null) continue;

                bool changed = false;
                foreach (var feature in features)
                {
                    if (feature != null && (feature.name.Contains("Fog") || feature.name.Contains("HeightFog") || feature.name.Contains("CubeMapFog")))
                    {
                        // Đảo trạng thái hoặc tắt
                        targetState = !feature.isActive;
                        feature.SetActive(targetState);
                        EditorUtility.SetDirty(feature);
                        changed = true;
                        modifiedCount++;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(rendererData);
                }
            }

            // Tắt cả Fog trong RenderSettings của Scene
            RenderSettings.fog = targetState;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string status = targetState ? "ĐÃ BẬT" : "ĐÃ TẮT HOÀN TOÀN";
            EditorUtility.DisplayDialog("Cập nhật Sương Mù",
                $"{status} hiệu ứng sương mù ({modifiedCount} tính năng HeightFog & CubeMapFog)!\n\n" +
                (targetState ? "Cảnh quan có lớp sương mờ." : "Khung cảnh trong vắt, nét căng, nhìn rõ tận chân trời!"), "Tuyệt vời");
        }

        [MenuItem("Tools/Lighting/Clear Fog Completely (Tắt vĩnh viễn Sương Mù)")]
        public static void ClearFogCompletely()
        {
            string[] rendererGuids = AssetDatabase.FindAssets("t:ScriptableRendererData", new[] { "Assets/Packet/env/FantasyKingdom/URP/Settings", "Assets/Settings" });

            int count = 0;
            foreach (var guid in rendererGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
                if (rendererData == null) continue;

                var prop = typeof(ScriptableRendererData).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop == null) continue;

                var features = prop.GetValue(rendererData) as List<ScriptableRendererFeature>;
                if (features == null) continue;

                bool changed = false;
                foreach (var feature in features)
                {
                    if (feature != null && (feature.name.Contains("Fog") || feature.name.Contains("HeightFog") || feature.name.Contains("CubeMapFog")))
                    {
                        feature.SetActive(false);
                        EditorUtility.SetDirty(feature);
                        changed = true;
                        count++;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(rendererData);
                }
            }

            RenderSettings.fog = false;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Thành công!",
                $"Đã tắt hoàn toàn {count} bộ lọc sương mù (HeightFog & CubeMapFog) trong URP Renderer!\nMàn hình camera sẽ không còn bị màng sương mờ mờ nữa.", "OK");
        }
    }
}
