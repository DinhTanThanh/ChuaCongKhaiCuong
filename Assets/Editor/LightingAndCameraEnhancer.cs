using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectTools.Editor
{
    public static class LightingAndCameraEnhancer
    {
        [MenuItem("Tools/Lighting/Fix Camera Dark & Blurry (Làm sáng & Nét màn hình)")]
        public static void FixLightingAndCamera()
        {
            // 1. Tối ưu Directional Light (Mặt trời)
            Light mainLight = null;
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    mainLight = l;
                    break;
                }
            }

            if (mainLight != null)
            {
                Undo.RecordObject(mainLight, "Enhance Directional Light");
                mainLight.intensity = 1.6f;
                mainLight.color = new Color(1f, 0.98f, 0.92f); // Ánh nắng ấm áp
                mainLight.shadows = LightShadows.Soft;
                mainLight.shadowStrength = 0.75f;
                mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

                // Gán mặt trời vào RenderSettings
                RenderSettings.sun = mainLight;
                EditorUtility.SetDirty(mainLight);
            }

            // 2. Tinh chỉnh RenderSettings (Ánh sáng môi trường từ Skybox)
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.35f; // Tăng sáng môi trường
            RenderSettings.reflectionIntensity = 1.0f;
            RenderSettings.fog = false; // Tắt sương mù che mờ nếu có

            // 3. Tinh chỉnh Post-Processing trên Global Volume
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            foreach (var vol in volumes)
            {
                if (vol.sharedProfile == null && vol.profile == null) continue;
                VolumeProfile profile = vol.sharedProfile != null ? vol.sharedProfile : vol.profile;

                Undo.RecordObject(profile, "Adjust Volume Profile");

                // a. Tắt hoặc chỉnh Depth Of Field (Nguyên nhân chính gây MỜ CẢNH VẬT)
                if (profile.TryGet<DepthOfField>(out var dof))
                {
                    dof.active = false; // Tắt DoF để nhìn nét căng từ gần tới xa
                }

                // b. Tắt Vignette (Viền tối 4 góc màn hình làm tối camera)
                if (profile.TryGet<Vignette>(out var vig))
                {
                    vig.active = false;
                }

                // c. Tinh chỉnh Tonemapping & Color Adjustments (Tăng sáng & Tươi tắn)
                if (!profile.TryGet<ColorAdjustments>(out var colorAdj))
                {
                    colorAdj = profile.Add<ColorAdjustments>(true);
                }
                colorAdj.active = true;
                colorAdj.postExposure.overrideState = true;
                colorAdj.postExposure.value = 0.5f; // Tăng sáng phơi sáng thêm +0.5 EV
                colorAdj.contrast.overrideState = true;
                colorAdj.contrast.value = 10f;       // Tăng độ tương phản cho trong trẻo
                colorAdj.saturation.overrideState = true;
                colorAdj.saturation.value = 10f;     // Tăng độ tươi tắn màu sắc

                // d. Bloom nhẹ nhàng rực rỡ
                if (profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.active = true;
                    bloom.intensity.overrideState = true;
                    bloom.intensity.value = 0.35f;
                    bloom.threshold.overrideState = true;
                    bloom.threshold.value = 0.95f;
                }

                EditorUtility.SetDirty(profile);
            }

            // 4. Kiểm tra Main Camera & Khử răng cưa SMAA / FXAA
            Camera cam = Camera.main;
            if (cam != null)
            {
                Undo.RecordObject(cam, "Enhance Camera");
                var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (camData != null)
                {
                    camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing; // SMAA nét căng
                    camData.antialiasingQuality = AntialiasingQuality.High;
                    EditorUtility.SetDirty(camData);
                }
            }

            DynamicGI.UpdateEnvironment();

            EditorUtility.DisplayDialog("Thành công!",
                "Đã tự động xử lý toàn bộ:\n" +
                "✓ Tắt Depth Of Field (Khắc phục hoàn toàn tình trạng mờ mờ xa xăm)\n" +
                "✓ Tắt Vignette (Bỏ viền đen tối 4 góc)\n" +
                "✓ Bật Post Exposure +0.5 EV (Cảnh sáng sủa, trong trẻo)\n" +
                "✓ Tăng ánh sáng mặt trời & Skybox Ambient lên 1.35x\n" +
                "✓ Khử răng cưa SMAA nét căng!", "Tuyệt vời");
        }
    }
}
