using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectTools.Editor
{
    public static class GaiaMapGenerator
    {
        private const int Resolution = 513;
        private const int TerrainWidth = 800;
        private const int TerrainLength = 800;
        private const int TerrainHeight = 90;
        private const float WaterHeightLevel = 0.12f;
        private const string OutputDir = "Assets/Prefabs/SampleMaps";

        // ==========================================
        // 1. MAP TRUNG TÂM (Diverse Kingdom)
        // ==========================================
        [MenuItem("Tools/Terrain/Kingdom Maps/1. Map Trung Tâm (Gốc: 0, 0)")]
        public static void GenerateDiverseKingdomMap()
        {
            GenerateSector("DiverseKingdom_Center", 0, 0, SectorType.Center);
        }

        // ==========================================
        // 2. MỞ RỘNG PHÍA BẮC (Thượng nguồn thác nước & Rừng núi)
        // ==========================================
        [MenuItem("Tools/Terrain/Kingdom Maps/2. Map Phía Bắc - Thượng Nguồn (0, +800)")]
        public static void GenerateNorthMap()
        {
            GenerateSector("DiverseKingdom_North", 0, 1, SectorType.North);
        }

        // ==========================================
        // 3. MỞ RỘNG PHÍA NAM (Hạ lưu cửa biển / Hồ lớn)
        // ==========================================
        [MenuItem("Tools/Terrain/Kingdom Maps/3. Map Phía Nam - Hạ Lưu Cửa Biển (0, -800)")]
        public static void GenerateSouthMap()
        {
            GenerateSector("DiverseKingdom_South", 0, -1, SectorType.South);
        }

        // ==========================================
        // 4. MỞ RỘNG PHÍA ĐÔNG (Đại bình nguyên xây đại đô thị)
        // ==========================================
        [MenuItem("Tools/Terrain/Kingdom Maps/4. Map Phía Đông - Đại Bình Nguyên (+800, 0)")]
        public static void GenerateEastMap()
        {
            GenerateSector("DiverseKingdom_East", 1, 0, SectorType.East);
        }

        // ==========================================
        // 5. MỞ RỘNG PHÍA TÂY (Dãy núi hiểm trở / Núi lửa)
        // ==========================================
        [MenuItem("Tools/Terrain/Kingdom Maps/5. Map Phía Tây - Dãy Núi Hiểm Trở (-800, 0)")]
        public static void GenerateWestMap()
        {
            GenerateSector("DiverseKingdom_West", -1, 0, SectorType.West);
        }

        // ==========================================
        // TẠO TẤT CẢ 5 MAP CÙNG LÚC
        // ==========================================
        [MenuItem("Tools/Terrain/Kingdom Maps/TẠO TOÀN BỘ 5 MAP MỞ RỘNG")]
        public static void GenerateAllKingdomMaps()
        {
            GenerateDiverseKingdomMap();
            GenerateNorthMap();
            GenerateSouthMap();
            GenerateEastMap();
            GenerateWestMap();

            EditorUtility.DisplayDialog("Hoàn tất!",
                "Đã tạo trọn bộ 5 Map ghép liền mạch!\nCác mảnh đất nằm tại Assets/Prefabs/SampleMaps/:\n" +
                "1. DiverseKingdom_Center (Vị trí 0, 0)\n" +
                "2. DiverseKingdom_North (Vị trí 0, 800)\n" +
                "3. DiverseKingdom_South (Vị trí 0, -800)\n" +
                "4. DiverseKingdom_East (Vị trí 800, 0)\n" +
                "5. DiverseKingdom_West (Vị trí -800, 0)\n\n" +
                "Chỉ cần kéo thả vào Scene, đặt đúng tọa độ là bản đồ tự động khớp khít 100%!", "Tuyệt vời");
        }

        private enum SectorType { Center, North, South, East, West }

        private static void GenerateSector(string sectorName, int gridX, int gridZ, SectorType type)
        {
            string highlandStampPath = "Assets/Gaia User Data/Stamps/2K Highlands/Highlands 01.exr";
            string canyonStampPath = "Assets/Gaia User Data/Stamps/2K Canyons/Canyon 01.exr";
            string meadowsStampPath = "Assets/Gaia User Data/Stamps/2K Meadows/Meadows 01.exr";

            ConfigureTextureReadable(highlandStampPath);
            ConfigureTextureReadable(canyonStampPath);
            ConfigureTextureReadable(meadowsStampPath);

            Texture2D highlandTex = AssetDatabase.LoadAssetAtPath<Texture2D>(highlandStampPath);
            Texture2D canyonTex = AssetDatabase.LoadAssetAtPath<Texture2D>(canyonStampPath);
            Texture2D meadowsTex = AssetDatabase.LoadAssetAtPath<Texture2D>(meadowsStampPath);

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

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float u = (float)x / (Resolution - 1);
                    float v = (float)y / (Resolution - 1);

                    // Tọa độ liên tục tổng thể theo chiều dọc
                    float globalV = v + gridZ;

                    // Đường sông uốn lượn liên tục khớp nhau qua các ô
                    float riverCenterX = 0.5f + 0.15f * Mathf.Sin(globalV * Mathf.PI * 2.5f);
                    float distToRiver = Mathf.Abs(u - riverCenterX);

                    float mountainVal = highlandTex != null ? highlandTex.GetPixelBilinear(u, v).r : 0f;
                    float canyonVal = canyonTex != null ? canyonTex.GetPixelBilinear(u, v).r : 0f;
                    float meadowsVal = meadowsTex != null ? meadowsTex.GetPixelBilinear(u, v).r : 0f;

                    float castlePlainHeight = 0.18f;
                    float baseGround = castlePlainHeight;

                    switch (type)
                    {
                        case SectorType.Center:
                        case SectorType.North:
                        case SectorType.South:
                            // Sông chảy xuyên qua 3 mảnh Bắc - Trung - Nam
                            float mountainArea = Mathf.Clamp01((riverCenterX - 0.08f - u) / 0.35f);
                            float mHeight = castlePlainHeight + mountainVal * 0.7f * mountainArea;
                            baseGround = Mathf.Lerp(castlePlainHeight, mHeight, mountainArea);

                            // Khoét lòng sông
                            float riverWidth = (type == SectorType.South) ? 0.12f : 0.07f; // Về phía Nam nở rộng thành cửa biển
                            if (distToRiver < riverWidth)
                            {
                                float t = distToRiver / riverWidth;
                                float riverCarve = (1f - Mathf.Cos(t * Mathf.PI)) * 0.5f;
                                heights[y, x] = Mathf.Lerp(WaterHeightLevel - 0.04f, baseGround, riverCarve);
                            }
                            else
                            {
                                heights[y, x] = baseGround;
                            }
                            break;

                        case SectorType.East:
                            // Phía Đông nối tiếp đồng bằng: Đất bằng phẳng mênh mông, rìa ngoài có đồi cỏ thoai thoải
                            float blendEast = Mathf.Clamp01((u - 0.2f) / 0.6f);
                            heights[y, x] = castlePlainHeight + meadowsVal * 0.15f * blendEast;
                            break;

                        case SectorType.West:
                            // Phía Tây nối tiếp dãy núi: Núi non hiểm trở cao vút
                            float blendWest = Mathf.Clamp01(1f - u);
                            heights[y, x] = castlePlainHeight + (mountainVal * 0.6f + canyonVal * 0.3f) * (0.5f + 0.5f * blendWest);
                            break;
                    }
                }
            }
            terrainData.SetHeights(0, 0, heights);

            // Gán Layers: Cỏ, Đất, Cát, Đá
            ApplyTerrainTextures(terrainData);

            // Tạo GameObject cha
            GameObject rootGO = new GameObject($"Map_{sectorName}");
            GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
            terrainGO.name = "Terrain";
            terrainGO.transform.SetParent(rootGO.transform);
            terrainGO.transform.localPosition = Vector3.zero;

            // Nếu là map có sông (Center, North, South) -> thêm mặt nước
            if (type == SectorType.Center || type == SectorType.North || type == SectorType.South)
            {
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
            }

            string prefabPath = $"{OutputDir}/Map_{sectorName}.prefab";
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

                    float rockWeight = Mathf.Clamp01((steepness - 20f) / 15f);
                    float sandWeight = 0f;
                    if (heightVal < WaterHeightLevel + 0.035f && rockWeight < 0.5f)
                    {
                        sandWeight = Mathf.Clamp01(1f - (heightVal - WaterHeightLevel) / 0.035f);
                    }

                    float dirtWeight = Mathf.Clamp01((steepness - 10f) / 15f) * (1f - rockWeight) * (1f - sandWeight);
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
