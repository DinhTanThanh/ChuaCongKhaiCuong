using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor
{
    public static class CircularRiverMapGenerator
    {
        private const int Resolution = 513;
        private const int TerrainWidth = 800;
        private const int TerrainLength = 800;
        private const int TerrainHeight = 90;
        private const float WaterHeightLevel = 0.12f;
        private const string OutputDir = "Assets/Prefabs/SampleMaps/CircularRiverWorld";

        private const float BaseRiverRadius = 550f; // Bán kính trung bình
        private const float RiverWidth = 180f; // Mở rộng độ rộng dòng sông lên 180m siêu đại
        private const float CastlePlainHeight = 0.18f;

        [MenuItem("Tools/Terrain/Circular River World/TẠO TRỌN BỘ 4 MAP (SÔNG KHÚC KHUỶU KHÉP KÍN + NÚI BAO QUANH)")]
        public static void GenerateAllCircularMaps()
        {
            GenerateQuadrant("Map_CircleRiver_SW", 0, 0, "Tây Nam");
            GenerateQuadrant("Map_CircleRiver_SE", 1, 0, "Đông Nam");
            GenerateQuadrant("Map_CircleRiver_NW", 0, 1, "Tây Bắc");
            GenerateQuadrant("Map_CircleRiver_NE", 1, 1, "Đông Bắc");

            EditorUtility.DisplayDialog("Thành công!",
                "Đã tạo hoàn tất 4 Mảnh Map ghép thành CON SÔNG TỰ NHIÊN UỐN LƯỢN KHÚC KHUỶU KHÉP KÍN!\n\n" +
                "Đặc điểm mới:\n" +
                "- Sông không còn là hình tròn cơ học mà uốn lượn khúc khuỷu, ngoằn ngoèo tự nhiên như sông thật.\n" +
                "- Bên ngoài bờ sông là những dãy núi non cao sừng sững, vách đá dựng đứng bao quanh tứ phía.\n" +
                "- Lòng chảo ở giữa vẫn là đại bình nguyên cỏ xanh phẳng lì để xây thành.\n\n" +
                "Tọa độ ghép trong Scene:\n" +
                "1. Map_CircleRiver_SW ➔ (0, 0)\n" +
                "2. Map_CircleRiver_SE ➔ (800, 0)\n" +
                "3. Map_CircleRiver_NW ➔ (0, 800)\n" +
                "4. Map_CircleRiver_NE ➔ (800, 800)", "Tuyệt vời");
        }

        private static void GenerateQuadrant(string sectorName, int gridX, int gridZ, string description)
        {
            string highlandStampPath = "Assets/Gaia User Data/Stamps/2K Highlands/Highlands 01.exr";
            string canyonStampPath = "Assets/Gaia User Data/Stamps/2K Canyons/Canyon 01.exr";

            ConfigureTextureReadable(highlandStampPath);
            ConfigureTextureReadable(canyonStampPath);

            Texture2D highlandTex = AssetDatabase.LoadAssetAtPath<Texture2D>(highlandStampPath);
            Texture2D canyonTex = AssetDatabase.LoadAssetAtPath<Texture2D>(canyonStampPath);

            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
                AssetDatabase.Refresh();
            }

            string terrainDataPath = $"{OutputDir}/{sectorName}_TerrainData.asset";
            TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainDataPath);
            if (terrainData == null)
            {
                terrainData = new TerrainData();
                AssetDatabase.CreateAsset(terrainData, terrainDataPath);
            }

            terrainData.heightmapResolution = Resolution;
            terrainData.size = new Vector3(TerrainWidth, TerrainHeight, TerrainLength);

            float[,] heights = new float[Resolution, Resolution];

            // Tâm của thung lũng kinh thành: (800, 800) trong hệ tọa độ thế giới
            Vector2 worldCenter = new Vector2(TerrainWidth, TerrainLength);

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float localX = ((float)x / (Resolution - 1)) * TerrainWidth;
                    float localZ = ((float)y / (Resolution - 1)) * TerrainLength;

                    float worldX = gridX * TerrainWidth + localX;
                    float worldZ = gridZ * TerrainLength + localZ;

                    Vector2 toPoint = new Vector2(worldX, worldZ) - worldCenter;
                    float distToCenter = toPoint.magnitude;

                    // Tính góc cực (Angle from 0 to 2*PI)
                    float angle = Mathf.Atan2(toPoint.y, toPoint.x);
                    if (angle < 0f) angle += Mathf.PI * 2f;

                    // 1. TẠO ĐỘ UỐN LƯỢN KHÚC KHUỶU CHO LÒNG SÔNG (Harmonic sine waves)
                    // Kết hợp nhiều bước sóng chu kỳ nguyên vẹn (4, 8, 12) để khi quay đủ 360 độ thì 2 đầu nối khít 100%
                    float meanderOffset = 75f * Mathf.Sin(angle * 4f)
                                        + 40f * Mathf.Cos(angle * 8f)
                                        + 20f * Mathf.Sin(angle * 12f + 1.2f);

                    float riverRadiusAtAngle = BaseRiverRadius + meanderOffset;
                    float distToRiver = Mathf.Abs(distToCenter - riverRadiusAtAngle);

                    float u = (float)x / (Resolution - 1);
                    float v = (float)y / (Resolution - 1);
                    float mountainVal = highlandTex != null ? highlandTex.GetPixelBilinear(u, v).r : 0f;
                    float canyonVal = canyonTex != null ? canyonTex.GetPixelBilinear(u, v).r : 0f;

                    float baseGround;

                    if (distToCenter < riverRadiusAtAngle)
                    {
                        // BÊN TRONG LÒNG SÔNG: ĐẢO TRUNG TÂM PHẲNG LÌ ĐỂ XÂY THÀNH
                        baseGround = CastlePlainHeight;
                    }
                    else
                    {
                        // BÊN NGOÀI LÒNG SÔNG: DÃY NÚI CAO SỪNG SỮNG BAO BỌC
                        float distOutsideRiver = distToCenter - (riverRadiusAtAngle + RiverWidth * 0.5f);
                        // Càng ra xa bờ sông thì núi càng dốc đứng và cao vút
                        float mountainRamp = Mathf.Clamp01(distOutsideRiver / 180f);
                        float mountainCombined = (mountainVal * 0.7f + canyonVal * 0.35f);

                        // Độ cao núi tăng vọt từ 0.18 lên tới 0.75 - 0.85
                        baseGround = CastlePlainHeight + mountainRamp * (0.15f + mountainCombined * 0.65f);
                    }

                    // 2. KHOÉT LÒNG SÔNG UỐN LƯỢN RỘNG LỚN
                    if (distToRiver < RiverWidth * 0.5f)
                    {
                        float t = distToRiver / (RiverWidth * 0.5f);
                        float riverBottom = WaterHeightLevel - 0.055f; // Đáy sông lõm sâu và phẳng rộng dưới nước
                        float carve = (1f - Mathf.Cos(t * Mathf.PI)) * 0.5f;
                        heights[y, x] = Mathf.Lerp(riverBottom, baseGround, carve);
                    }
                    else
                    {
                        heights[y, x] = baseGround;
                    }
                }
            }
            terrainData.SetHeights(0, 0, heights);

            // Gán 4 lớp texture: Cỏ xanh ở bình nguyên, Đất ở chân núi, Cát ở bờ sông, Đá vách núi dựng đứng
            ApplyTerrainTextures(terrainData);

            // Tạo GameObject Prefab
            GameObject rootGO = new GameObject(sectorName);
            GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
            terrainGO.name = "Terrain";
            terrainGO.transform.SetParent(rootGO.transform);
            terrainGO.transform.localPosition = Vector3.zero;

            // Mặt nước hồ / sông
            GameObject waterGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterGO.name = "RiverWater";
            waterGO.transform.SetParent(rootGO.transform);
            waterGO.transform.localPosition = new Vector3(TerrainWidth / 2f, WaterHeightLevel * TerrainHeight, TerrainLength / 2f);
            waterGO.transform.localScale = new Vector3(TerrainWidth / 10f, 1f, TerrainLength / 10f);

            string waterMatPath = "Assets/Packet/env/FantasyKingdom/Art/Materials/Water/WaterLake.mat";
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(waterMatPath);
            if (waterMat != null)
            {
                waterGO.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
            }

            string prefabPath = $"{OutputDir}/{sectorName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(rootGO, prefabPath);

            Object.DestroyImmediate(rootGO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var createdPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = createdPrefab;
            EditorGUIUtility.PingObject(createdPrefab);
        }

        private static void ApplyTerrainTextures(TerrainData terrainData)
        {
            string grassLayerPath = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Grass_A.terrainlayer";
            string dirtLayerPath = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Dirt_A.terrainlayer";
            string sandLayerPath = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Sand_A.terrainlayer";
            string rockLayerPath = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Rock_A.terrainlayer";

            var grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(grassLayerPath);
            var dirtLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(dirtLayerPath);
            var sandLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(sandLayerPath);
            var rockLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(rockLayerPath);

            if (grassLayer == null || rockLayer == null || sandLayer == null || dirtLayer == null) return;

            terrainData.terrainLayers = new TerrainLayer[] { grassLayer, dirtLayer, sandLayer, rockLayer };
            int alphaRes = terrainData.alphamapResolution;
            float[,,] alphaMaps = new float[alphaRes, alphaRes, 4];

            for (int y = 0; y < alphaRes; y++)
            {
                for (int x = 0; x < alphaRes; x++)
                {
                    float normX = (float)x / alphaRes;
                    float normY = (float)y / alphaRes;

                    float heightVal = terrainData.GetInterpolatedHeight(normX, normY) / TerrainHeight;
                    float steepness = terrainData.GetSteepness(normX, normY);

                    // Vách đá dốc đứng ở các dãy núi
                    float rockWeight = Mathf.Clamp01((steepness - 18f) / 14f);

                    // Bờ cát vàng ven 2 bên mép nước
                    float sandWeight = 0f;
                    if (heightVal < WaterHeightLevel + 0.035f && rockWeight < 0.5f)
                    {
                        sandWeight = Mathf.Clamp01(1f - (heightVal - WaterHeightLevel) / 0.035f);
                    }

                    // Đất chân núi
                    float dirtWeight = Mathf.Clamp01((steepness - 9f) / 14f) * (1f - rockWeight) * (1f - sandWeight);

                    // Cỏ xanh phủ toàn bộ bình nguyên
                    float grassWeight = Mathf.Max(0f, 1f - rockWeight - sandWeight - dirtWeight);

                    alphaMaps[y, x, 0] = grassWeight;
                    alphaMaps[y, x, 1] = dirtWeight;
                    alphaMaps[y, x, 2] = sandWeight;
                    alphaMaps[y, x, 3] = rockWeight;
                }
            }
            terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        private static void ConfigureTextureReadable(string path)
        {
            if (File.Exists(path))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
