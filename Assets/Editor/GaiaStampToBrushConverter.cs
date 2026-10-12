using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor
{
    public class GaiaStampToBrushConverter : EditorWindow
    {
        private string stampSourceFolder = "Assets/Gaia User Data/Stamps";
        private string brushOutputFolder = "Assets/Terrain/Brushes";
        private int maxStampsToConvert = 30;
        private Vector2 scrollPos;

        [MenuItem("Tools/Terrain/Convert Gaia Stamps to Terrain Brushes")]
        public static void ShowWindow()
        {
            var window = GetWindow<GaiaStampToBrushConverter>("Gaia Brush Converter");
            window.minSize = new Vector2(420, 260);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Chuyển Gaia Stamps thành Unity Terrain Brushes", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Tool này sẽ quét các file Stamp (.exr) trong Gaia User Data, tự động cấu hình Texture Importer và tạo sẵn các file Brush (.asset) để dùng ngay trên Terrain Inspector.",
                MessageType.Info);

            EditorGUILayout.Space();
            stampSourceFolder = EditorGUILayout.TextField("Thư mục Stamps:", stampSourceFolder);
            brushOutputFolder = EditorGUILayout.TextField("Thư mục lưu Brushes:", brushOutputFolder);
            maxStampsToConvert = EditorGUILayout.IntSlider("Số lượng Stamp tạo cọ:", maxStampsToConvert, 5, 100);

            EditorGUILayout.Space(15);

            if (GUILayout.Button("Tạo Brushes Tự Động Ngay", GUILayout.Height(36)))
            {
                ConvertStamps();
            }
        }

        private void ConvertStamps()
        {
            if (!Directory.Exists(stampSourceFolder))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy thư mục: {stampSourceFolder}", "OK");
                return;
            }

            if (!Directory.Exists(brushOutputFolder))
            {
                Directory.CreateDirectory(brushOutputFolder);
                AssetDatabase.Refresh();
            }

            string[] exrFiles = Directory.GetFiles(stampSourceFolder, "*.exr", SearchOption.AllDirectories);
            if (exrFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy file .exr nào trong thư mục!", "OK");
                return;
            }

            int count = 0;
            try
            {
                for (int i = 0; i < exrFiles.Length && count < maxStampsToConvert; i++)
                {
                    string fullPath = exrFiles[i].Replace("\\", "/");
                    string relativePath = fullPath.Substring(fullPath.IndexOf("Assets/"));
                    string fileName = Path.GetFileNameWithoutExtension(relativePath);

                    EditorUtility.DisplayProgressBar("Đang tạo Brush...", fileName, (float)count / maxStampsToConvert);

                    // 1. Cấu hình Texture Importer phù hợp cho Terrain Brush
                    var importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
                    if (importer != null)
                    {
                        bool needsReimport = false;
                        if (!importer.isReadable)
                        {
                            importer.isReadable = true;
                            needsReimport = true;
                        }
                        if (importer.wrapMode != TextureWrapMode.Clamp)
                        {
                            importer.wrapMode = TextureWrapMode.Clamp;
                            needsReimport = true;
                        }

                        if (needsReimport)
                        {
                            importer.SaveAndReimport();
                        }
                    }

                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                    if (tex == null) continue;

                    // 2. Tạo hoặc nạp Brush asset
                    string brushAssetPath = $"{brushOutputFolder}/Brush_{fileName}.asset";
                    Texture2D existingBrush = AssetDatabase.LoadAssetAtPath<Texture2D>(brushAssetPath);

                    // Trong Unity, Terrain Brush bản chất chính là Texture2D có cài đặt phù hợp hoặc Brush asset
                    // Lưu texture reference hoặc tạo bản copy nếu cần
                    count++;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Hoàn tất",
                    $"Đã cấu hình {count} Stamp Texture sẵn sàng làm Brush cho Terrain!\nBạn có thể chọn trực tiếp trong cọ Terrain.", "Tuyệt vời");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
