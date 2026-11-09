using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTools.Editor
{
    public static class CircularRiverGroundBeautifier
    {
        private const int TerrainWidth = 800;
        private const int TerrainLength = 800;
        private const int TerrainHeight = 90;
        private const float WaterHeightLevel = 0.12f;
        private const float BaseRiverRadius = 550f;
        private const float RiverWidth = 180f;
        private const float CastlePlainHeight = 0.18f;
        private const string OutputDir = "Assets/Prefabs/SampleMaps/CircularRiverWorld";
        private const string DetailPrefabsDir = OutputDir + "/DetailMeshes";
        private const string MarkerChildName = "Vegetation_And_Props";

        private static readonly (string name, int gridX, int gridZ)[] Sectors = new[]
        {
            ("Map_CircleRiver_SW", 0, 0),
            ("Map_CircleRiver_SE", 1, 0),
            ("Map_CircleRiver_NW", 0, 1),
            ("Map_CircleRiver_NE", 1, 1),
        };

        [InitializeOnLoadMethod]
        private static void RegisterAutoBeautify()
        {
            EditorApplication.update += TryAutoBeautifyWhenIdle;
        }

        private static void TryAutoBeautifyWhenIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EditorApplication.update -= TryAutoBeautifyWhenIdle;

            // Kiểm tra xem trong Scene hiện tại có các mảnh Map_CircleRiver_* chưa được thêm cỏ cây (chưa có Vegetation_And_Props) hay không
            bool needsUpgradeInScene = false;
            foreach (var (sectorName, _, _) in Sectors)
            {
                GameObject sceneObj = FindSectorInActiveScene(sectorName);
                if (sceneObj != null && sceneObj.transform.Find(MarkerChildName) == null)
                {
                    needsUpgradeInScene = true;
                    break;
                }
            }

            if (needsUpgradeInScene)
            {
                BeautifyAllCircularRiverMaps(showDialog: false);
            }
        }

        [MenuItem("Tools/Terrain/Circular River World/LÀM ĐẸP MẶT ĐẤT & THÊM CỎ CÂY HOA LÁ ĐÁ NƯỚC (NÂNG CẤP NGAY)")]
        public static void BeautifyFromMenu()
        {
            BeautifyAllCircularRiverMaps(showDialog: true);
        }

        public static void BeautifyAllCircularRiverMaps(bool showDialog = true)
        {
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }
            if (!Directory.Exists(DetailPrefabsDir))
            {
                Directory.CreateDirectory(DetailPrefabsDir);
            }

            Material urpTerrainMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Packet/env/FantasyKingdom/Terrain/URPTerrain_MAT.mat");
            TerrainLayer[] tunedLayers = LoadAndTuneTerrainLayers();
            DetailPrototype[] detailPrototypes = BuildGrassAndLeafDetailPrototypes();

            GameObject[] scatterGrassPrefabs = LoadPrefabList(new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_05.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_06.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_07.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_08.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_09.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_10.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_11.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_12.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_13.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_14.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_15.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_16.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Scatter_Grass/Scatter_Grass_17.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_05.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_06.prefab",
            });

            GameObject[] bushPrefabs = LoadPrefabList(new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/Bush_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/Bush_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_05.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_06.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_Cluster_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_Cluster_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_Cluster_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_Cluster_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_Cluster_05.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Bush_Cluster_06.prefab",
            });

            GameObject[] meadowTreePrefabs = LoadPrefabList(new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/BigHeroTree_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Big_Tree_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/TreeLargeStylized_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/TreeLargeStylized_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Large_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Large_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Round_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Round_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Round_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SmallTreeRound_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SmallTreeRound_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SmallTreeRound_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Thin_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Thin_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Thin_03.prefab",
            });

            GameObject[] mountainPinePrefabs = LoadPrefabList(new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/TreePine_VarA.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/TreePine_VarB.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/TreePine_VarC.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Pine_Cluster_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Pine_Cluster_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Pine_Cluster_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Trees and Bushes/SM_Env_Tree_Pine_Cluster_05.prefab",
            });

            GameObject[] rockPrefabs = LoadPrefabList(new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Rocks/Rocks_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Rocks/Rocks_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Rocks/Rocks_11.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Rocks/Rocks_12.prefab",
            });

            GameObject[] waterlilyPrefabs = LoadPrefabList(new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Water plants/SM_Env_Waterlily_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Water plants/SM_Env_Waterlily_02.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Water plants/SM_Env_Waterlily_03.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Water plants/SM_Env_Waterlily_04.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Water plants/SM_Env_Waterlily_05.prefab",
            });

            int upgradedCount = 0;
            foreach (var (sectorName, gridX, gridZ) in Sectors)
            {
                string terrainDataPath = $"{OutputDir}/{sectorName}_TerrainData.asset";
                TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainDataPath);

                GameObject sceneSector = FindSectorInActiveScene(sectorName);
                if (terrainData == null && sceneSector != null)
                {
                    Terrain t = sceneSector.GetComponentInChildren<Terrain>();
                    if (t != null) terrainData = t.terrainData;
                }

                if (terrainData != null)
                {
                    EnhanceTerrainData(terrainData, gridX, gridZ, tunedLayers, detailPrototypes);
                    EditorUtility.SetDirty(terrainData);
                }

                if (sceneSector != null)
                {
                    PopulateSectorInstance(
                        sceneSector, terrainData, gridX, gridZ, urpTerrainMat,
                        scatterGrassPrefabs, bushPrefabs, meadowTreePrefabs, mountainPinePrefabs, rockPrefabs, waterlilyPrefabs
                    );
                    EditorUtility.SetDirty(sceneSector);
                    upgradedCount++;
                }

                // Đồng bộ cập nhật cả vào file Prefab của Sector nếu có
                string prefabPath = $"{OutputDir}/{sectorName}.prefab";
                if (File.Exists(prefabPath) && terrainData != null)
                {
                    GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefabAsset != null)
                    {
                        GameObject tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                        PopulateSectorInstance(
                            tempInstance, terrainData, gridX, gridZ, urpTerrainMat,
                            scatterGrassPrefabs, bushPrefabs, meadowTreePrefabs, mountainPinePrefabs, rockPrefabs, waterlilyPrefabs
                        );
                        PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
                        Object.DestroyImmediate(tempInstance);
                    }
                }
            }

            if (SceneManager.GetActiveScene().IsValid())
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CircularRiverGroundBeautifier] Đã nâng cấp mặt đất, thảm cỏ 3D, cây cối, hoa lá, đá tảng & hoa súng cho {upgradedCount} mảnh bản đồ trong Scene!");

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Hoàn tất Làm Đẹp Mặt Đất & Sinh Thái!",
                    "Đã nâng cấp toàn diện cả 4 mảnh bản đồ (Map_CircleRiver_SW, SE, NW, NE):\n\n" +
                    "1. Vật liệu URP Terrain Lit & Splatmap tự nhiên (Đường mòn đất uốn lượn, thảo nguyên cỏ đa sắc, bãi cát ven sông, đá vách núi).\n" +
                    "2. Thảm cỏ 3D & cỏ lá GPU-Instanced phủ dày mượt mà trên toàn bộ mặt đất.\n" +
                    "3. Hàng ngàn cụm cỏ hoa dại 3D (Scatter_Grass), bụi cây (Bushes), rừng cây cổ thụ & thông núi (Trees), đá tảng rêu phong (Rocks) và hoa súng trên mặt sông (Waterlilies)!",
                    "Tuyệt vời!");
            }
        }

        private static GameObject FindSectorInActiveScene(string sectorName)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return null;

            foreach (GameObject root in activeScene.GetRootGameObjects())
            {
                if (root.name == sectorName) return root;
                Transform found = FindDeepChild(root.transform, sectorName);
                if (found != null) return found.gameObject;
            }
            return null;
        }

        private static Transform FindDeepChild(Transform parent, string targetName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == targetName) return child;
                Transform res = FindDeepChild(child, targetName);
                if (res != null) return res;
            }
            return null;
        }

        private static TerrainLayer[] LoadAndTuneTerrainLayers()
        {
            string grassLayerPath = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Grass_A.terrainlayer";
            string dirtLayerPath  = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Dirt_A.terrainlayer";
            string sandLayerPath  = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Sand_A.terrainlayer";
            string rockLayerPath  = "Assets/Packet/env/FantasyKingdom/Terrain/Materials/Blockout_Rock_A.terrainlayer";

            var grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(grassLayerPath);
            var dirtLayer  = AssetDatabase.LoadAssetAtPath<TerrainLayer>(dirtLayerPath);
            var sandLayer  = AssetDatabase.LoadAssetAtPath<TerrainLayer>(sandLayerPath);
            var rockLayer  = AssetDatabase.LoadAssetAtPath<TerrainLayer>(rockLayerPath);

            if (grassLayer != null)
            {
                grassLayer.tileSize = new Vector2(14f, 14f);
                grassLayer.normalScale = 1.15f;
                grassLayer.diffuseRemapMax = new Vector4(0.76f, 0.98f, 0.62f, 1f);
                EditorUtility.SetDirty(grassLayer);
            }
            if (dirtLayer != null)
            {
                dirtLayer.tileSize = new Vector2(12f, 12f);
                dirtLayer.normalScale = 1.2f;
                dirtLayer.diffuseRemapMax = new Vector4(0.88f, 0.78f, 0.64f, 1f);
                EditorUtility.SetDirty(dirtLayer);
            }
            if (sandLayer != null)
            {
                sandLayer.tileSize = new Vector2(10f, 10f);
                sandLayer.normalScale = 1.0f;
                sandLayer.diffuseRemapMax = new Vector4(0.96f, 0.90f, 0.74f, 1f);
                EditorUtility.SetDirty(sandLayer);
            }
            if (rockLayer != null)
            {
                rockLayer.tileSize = new Vector2(20f, 20f);
                rockLayer.normalScale = 1.35f;
                EditorUtility.SetDirty(rockLayer);
            }

            if (grassLayer == null || dirtLayer == null || sandLayer == null || rockLayer == null)
                return null;

            return new[] { grassLayer, dirtLayer, sandLayer, rockLayer };
        }

        private static DetailPrototype[] BuildGrassAndLeafDetailPrototypes()
        {
            List<DetailPrototype> list = new List<DetailPrototype>();

            // 1. Tạo các Single-Mesh Detail Prefabs hỗ trợ GPU Instancing từ Grass_A_A, Grass_A_B, Grass_A_C, Leaves_Cluster_01, Leaves_Cluster_02
            string[] sourceMeshPrefabs = new[]
            {
                "Assets/Packet/env/FantasyKingdom/Art/Meshes/Scatter/Grass_A/Grass_A_A.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Meshes/Scatter/Grass_A/Grass_A_B.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Meshes/Scatter/Grass_A/Grass_A_C.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_01.prefab",
                "Assets/Packet/env/FantasyKingdom/Art/Prefabs/Foliage/Leaves/Leaves_Cluster_02.prefab",
            };

            (float minW, float maxW, float minH, float maxH, Color healthy, Color dry)[] meshConfigs = new[]
            {
                (1.1f, 2.2f, 0.85f, 1.55f, new Color(0.78f, 0.98f, 0.58f, 1f), new Color(0.92f, 0.86f, 0.52f, 1f)),
                (1.0f, 2.0f, 0.80f, 1.45f, new Color(0.68f, 0.94f, 0.52f, 1f), new Color(0.88f, 0.82f, 0.48f, 1f)),
                (1.2f, 2.4f, 0.90f, 1.65f, new Color(0.82f, 1.00f, 0.62f, 1f), new Color(0.95f, 0.88f, 0.55f, 1f)),
                (1.0f, 1.8f, 0.75f, 1.25f, new Color(0.85f, 0.98f, 0.70f, 1f), new Color(0.92f, 0.84f, 0.58f, 1f)),
                (1.0f, 1.9f, 0.75f, 1.30f, new Color(0.80f, 0.96f, 0.65f, 1f), new Color(0.90f, 0.82f, 0.55f, 1f)),
            };

            for (int i = 0; i < sourceMeshPrefabs.Length; i++)
            {
                GameObject singleMeshPrefab = GetOrCreateSingleMeshDetailPrefab(sourceMeshPrefabs[i], $"DetailMesh_{i}_{Path.GetFileNameWithoutExtension(sourceMeshPrefabs[i])}");
                if (singleMeshPrefab != null)
                {
                    var cfg = meshConfigs[i];
                    DetailPrototype dp = new DetailPrototype
                    {
                        prototype = singleMeshPrefab,
                        usePrototypeMesh = true,
                        useInstancing = true,
                        renderMode = DetailRenderMode.VertexLit,
                        minWidth = cfg.minW,
                        maxWidth = cfg.maxW,
                        minHeight = cfg.minH,
                        maxHeight = cfg.maxH,
                        noiseSpread = 0.25f,
                        healthyColor = cfg.healthy,
                        dryColor = cfg.dry,
                    };
                    list.Add(dp);
                }
            }

            // 2. Bổ sung 2 lớp cỏ 2D Texture mềm mại từ NatureStarterKit2 nếu có
            string[] grassTexPaths = new[]
            {
                "Assets/Packet/env/NatureStarterKit2/Textures/grass01.tga",
                "Assets/Packet/env/NatureStarterKit2/Textures/grass02.tga",
            };
            foreach (string texPath in grassTexPaths)
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    DetailPrototype dpTex = new DetailPrototype
                    {
                        prototypeTexture = tex,
                        usePrototypeMesh = false,
                        renderMode = DetailRenderMode.GrassBillboard,
                        minWidth = 0.9f,
                        maxWidth = 1.6f,
                        minHeight = 0.65f,
                        maxHeight = 1.25f,
                        noiseSpread = 0.3f,
                        healthyColor = new Color(0.56f, 0.82f, 0.36f, 1f),
                        dryColor = new Color(0.78f, 0.74f, 0.38f, 1f),
                    };
                    list.Add(dpTex);
                }
            }

            return list.ToArray();
        }

        private static GameObject GetOrCreateSingleMeshDetailPrefab(string sourcePrefabPath, string detailName)
        {
            string targetPath = $"{DetailPrefabsDir}/{detailName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
            if (existing != null && existing.GetComponent<MeshFilter>() != null && existing.GetComponent<MeshRenderer>() != null)
            {
                Material mat = existing.GetComponent<MeshRenderer>().sharedMaterial;
                if (mat != null && !mat.enableInstancing)
                {
                    mat.enableInstancing = true;
                    EditorUtility.SetDirty(mat);
                }
                return existing;
            }

            GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (src == null) return null;

            MeshFilter srcMf = src.GetComponentInChildren<MeshFilter>();
            MeshRenderer srcMr = src.GetComponentInChildren<MeshRenderer>();
            if (srcMf == null || srcMf.sharedMesh == null || srcMr == null || srcMr.sharedMaterial == null)
                return null;

            if (!srcMr.sharedMaterial.enableInstancing)
            {
                srcMr.sharedMaterial.enableInstancing = true;
                EditorUtility.SetDirty(srcMr.sharedMaterial);
            }

            GameObject go = new GameObject(detailName);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = srcMf.sharedMesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = srcMr.sharedMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, targetPath);
            Object.DestroyImmediate(go);
            return saved;
        }

        private static void EnhanceTerrainData(TerrainData terrainData, int gridX, int gridZ, TerrainLayer[] tunedLayers, DetailPrototype[] detailPrototypes)
        {
            if (tunedLayers != null && tunedLayers.Length == 4)
            {
                terrainData.terrainLayers = tunedLayers;
            }

            Vector2 worldCenter = new Vector2(TerrainWidth, TerrainLength);

            // 1. Nâng cấp Splatmap (Alphamaps) với đường mòn đất tự nhiên, mảng thảo nguyên đa sắc, bờ cát và vách đá
            int alphaRes = terrainData.alphamapResolution;
            float[,,] alphaMaps = new float[alphaRes, alphaRes, 4];

            for (int y = 0; y < alphaRes; y++)
            {
                for (int x = 0; x < alphaRes; x++)
                {
                    float normX = (float)x / (alphaRes - 1);
                    float normY = (float)y / (alphaRes - 1);

                    float worldX = gridX * TerrainWidth + normX * TerrainWidth;
                    float worldZ = gridZ * TerrainLength + normY * TerrainLength;

                    float heightVal = terrainData.GetInterpolatedHeight(normX, normY) / TerrainHeight;
                    float steepness = terrainData.GetSteepness(normX, normY);

                    // Nhiễu tự nhiên đa tầng cho mặt đất
                    float nLarge = Mathf.PerlinNoise(worldX * 0.008f + 13.7f, worldZ * 0.008f + 29.1f);
                    float nMed   = Mathf.PerlinNoise(worldX * 0.025f + 71.3f, worldZ * 0.025f + 43.9f);
                    float nFine  = Mathf.PerlinNoise(worldX * 0.075f + 11.2f, worldZ * 0.075f + 87.5f);

                    // Tạo các đường mòn đất tự nhiên uốn lượn nhẹ trên thảo nguyên (ridge noise)
                    float trailWave = Mathf.Abs(Mathf.PerlinNoise(worldX * 0.0055f + 5.1f, worldZ * 0.0055f + 9.4f) - 0.5f);
                    float trailMask = Mathf.Clamp01(1f - (trailWave / 0.032f)) * 0.72f;

                    // Vách đá dốc + các mỏm đá chân đồi
                    float rockWeight = Mathf.Clamp01((steepness - 15f) / 13f);
                    if (heightVal > CastlePlainHeight + 0.02f && nLarge * 0.6f + nMed * 0.4f > 0.68f)
                    {
                        rockWeight = Mathf.Max(rockWeight, (nLarge - 0.55f) * 1.4f);
                    }
                    rockWeight = Mathf.Clamp01(rockWeight);

                    // Bờ cát vàng & sỏi ven sông
                    float sandWeight = 0f;
                    if (heightVal < WaterHeightLevel + 0.042f && rockWeight < 0.6f)
                    {
                        float shoreFactor = 1f - (heightVal - WaterHeightLevel) / 0.042f;
                        sandWeight = Mathf.Clamp01(shoreFactor + (nMed - 0.5f) * 0.25f) * (1f - rockWeight);
                    }

                    // Đất tự nhiên (chân núi + đường mòn thảo nguyên + mảng đất rừng dưới tán cây)
                    float slopeDirt = Mathf.Clamp01((steepness - 6f) / 14f);
                    float meadowSoilPatch = Mathf.Clamp01((nLarge * 0.65f + nMed * 0.35f - 0.56f) * 1.65f) * (0.45f + 0.35f * nFine);
                    float rawDirt = Mathf.Max(slopeDirt, Mathf.Max(trailMask, meadowSoilPatch));
                    float dirtWeight = Mathf.Clamp01(rawDirt) * (1f - rockWeight) * (1f - sandWeight);

                    // Cỏ xanh mướt phủ chủ đạo
                    float grassWeight = Mathf.Max(0f, 1f - rockWeight - sandWeight - dirtWeight);

                    alphaMaps[y, x, 0] = grassWeight;
                    alphaMaps[y, x, 1] = dirtWeight;
                    alphaMaps[y, x, 2] = sandWeight;
                    alphaMaps[y, x, 3] = rockWeight;
                }
            }
            terrainData.SetAlphamaps(0, 0, alphaMaps);

            // 2. Phủ thảm cỏ 3D GPU-Instanced (DetailPrototypes) trên seluruh vùng cỏ xanh
            if (detailPrototypes != null && detailPrototypes.Length > 0)
            {
                int detailRes = 512;
                terrainData.SetDetailResolution(detailRes, 16);
                terrainData.detailPrototypes = detailPrototypes;

                for (int layerIdx = 0; layerIdx < detailPrototypes.Length; layerIdx++)
                {
                    int[,] detailMap = new int[detailRes, detailRes];
                    float seedOffset = layerIdx * 37.3f;

                    for (int dy = 0; dy < detailRes; dy++)
                    {
                        for (int dx = 0; dx < detailRes; dx++)
                        {
                            float normX = (float)dx / (detailRes - 1);
                            float normY = (float)dy / (detailRes - 1);

                            float heightVal = terrainData.GetInterpolatedHeight(normX, normY) / TerrainHeight;
                            float steepness = terrainData.GetSteepness(normX, normY);

                            // Chỉ mọc cỏ ở nơi cao hơn mặt nước và không phải vách đá dựng đứng
                            if (heightVal <= WaterHeightLevel + 0.018f || steepness > 24f)
                            {
                                detailMap[dy, dx] = 0;
                                continue;
                            }

                            int ax = Mathf.Clamp(Mathf.RoundToInt(normX * (alphaRes - 1)), 0, alphaRes - 1);
                            int ay = Mathf.Clamp(Mathf.RoundToInt(normY * (alphaRes - 1)), 0, alphaRes - 1);
                            float grassW = alphaMaps[ay, ax, 0];

                            if (grassW < 0.32f)
                            {
                                detailMap[dy, dx] = 0;
                                continue;
                            }

                            float worldX = gridX * TerrainWidth + normX * TerrainWidth;
                            float worldZ = gridZ * TerrainLength + normY * TerrainLength;

                            float clumpNoise = Mathf.PerlinNoise(worldX * 0.035f + seedOffset, worldZ * 0.035f + seedOffset);
                            float fineNoise  = Mathf.PerlinNoise(worldX * 0.11f - seedOffset, worldZ * 0.11f + seedOffset * 0.5f);

                            float combined = (clumpNoise * 0.65f + fineNoise * 0.35f) * grassW;
                            if (combined > 0.30f)
                            {
                                int density = Mathf.Clamp(Mathf.RoundToInt((combined - 0.25f) * 14f), 1, 10);
                                detailMap[dy, dx] = density;
                            }
                        }
                    }

                    terrainData.SetDetailLayer(0, 0, layerIdx, detailMap);
                }
            }
        }

        private static void PopulateSectorInstance(
            GameObject sectorRoot,
            TerrainData terrainData,
            int gridX,
            int gridZ,
            Material urpTerrainMat,
            GameObject[] scatterGrassPrefabs,
            GameObject[] bushPrefabs,
            GameObject[] meadowTreePrefabs,
            GameObject[] mountainPinePrefabs,
            GameObject[] rockPrefabs,
            GameObject[] waterlilyPrefabs)
        {
            Terrain terrain = sectorRoot.GetComponentInChildren<Terrain>();
            if (terrain != null)
            {
                if (urpTerrainMat != null)
                {
                    terrain.materialTemplate = urpTerrainMat;
                }
                terrain.drawInstanced = true;
                terrain.detailObjectDistance = 250f;
                terrain.detailObjectDensity = 1.0f;
                terrain.treeDistance = 1200f;
                terrain.Flush();
            }

            if (terrainData == null) return;

            // Xóa container cũ nếu đã tồn tại để tái tạo sạch sẽ
            Transform oldContainer = sectorRoot.transform.Find(MarkerChildName);
            if (oldContainer != null)
            {
                Object.DestroyImmediate(oldContainer.gameObject);
            }

            GameObject container = new GameObject(MarkerChildName);
            container.transform.SetParent(sectorRoot.transform, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;

            Transform grassGroup = CreateSubGroup(container.transform, "Meadow_Grass_And_Flowers");
            Transform bushGroup  = CreateSubGroup(container.transform, "Bushes_And_Shrubs");
            Transform treeGroup  = CreateSubGroup(container.transform, "Trees_And_Groves");
            Transform rockGroup  = CreateSubGroup(container.transform, "Rocks_And_Boulders");
            Transform lilyGroup  = CreateSubGroup(container.transform, "River_Waterlilies");

            Vector2 worldCenter = new Vector2(TerrainWidth, TerrainLength);
            System.Random rng = new System.Random(10007 + gridX * 313 + gridZ * 997);

            // 1. THẢM CỤM CỎ 3D & HOA DẠI (Scatter_Grass_01..17 & Leaves_Cluster_01..06) — 420 cụm mỗi mảnh Map
            if (scatterGrassPrefabs.Length > 0)
            {
                int grassCount = 420;
                for (int i = 0; i < grassCount; i++)
                {
                    float lx = NextRange(rng, 12f, TerrainWidth - 12f);
                    float lz = NextRange(rng, 12f, TerrainLength - 12f);
                    float normX = lx / TerrainWidth;
                    float normZ = lz / TerrainLength;

                    float hNorm = terrainData.GetInterpolatedHeight(normX, normZ) / TerrainHeight;
                    float steep = terrainData.GetSteepness(normX, normZ);

                    if (hNorm <= WaterHeightLevel + 0.015f || steep > 22f)
                        continue;

                    float yPos = hNorm * TerrainHeight;
                    Vector3 normal = terrainData.GetInterpolatedNormal(normX, normZ);
                    GameObject prefab = scatterGrassPrefabs[rng.Next(scatterGrassPrefabs.Length)];
                    float scale = NextRange(rng, 0.85f, 1.55f);

                    SpawnProp(prefab, grassGroup, new Vector3(lx, yPos, lz), normal, NextRange(rng, 0f, 360f), scale, alignToNormal: true);
                }
            }

            // 2. BỤI CÂY HOA LÁ (Bushes & Shrubs) — 160 cụm mỗi mảnh Map
            if (bushPrefabs.Length > 0)
            {
                int bushCount = 160;
                for (int i = 0; i < bushCount; i++)
                {
                    float lx = NextRange(rng, 15f, TerrainWidth - 15f);
                    float lz = NextRange(rng, 15f, TerrainLength - 15f);
                    float normX = lx / TerrainWidth;
                    float normZ = lz / TerrainLength;

                    float hNorm = terrainData.GetInterpolatedHeight(normX, normZ) / TerrainHeight;
                    float steep = terrainData.GetSteepness(normX, normZ);

                    if (hNorm <= WaterHeightLevel + 0.012f || steep > 26f)
                        continue;

                    float worldX = gridX * TerrainWidth + lx;
                    float worldZ = gridZ * TerrainLength + lz;
                    float distToCenter = Vector2.Distance(new Vector2(worldX, worldZ), worldCenter);

                    // Giữ quảng trường trung tâm (bán kính 95m) thoáng đãng hơn
                    if (distToCenter < 95f && rng.NextDouble() < 0.75)
                        continue;

                    float yPos = hNorm * TerrainHeight;
                    Vector3 normal = terrainData.GetInterpolatedNormal(normX, normZ);
                    GameObject prefab = bushPrefabs[rng.Next(bushPrefabs.Length)];
                    float scale = NextRange(rng, 0.85f, 1.45f);

                    SpawnProp(prefab, bushGroup, new Vector3(lx, yPos, lz), normal, NextRange(rng, 0f, 360f), scale, alignToNormal: false);
                }
            }

            // 3. RỪNG CÂY CỔ THỤ VEN SÔNG, THẢO NGUYÊN & RỪNG THÔNG TRÊN NÚI — 180 cụm cây mỗi mảnh Map
            if (meadowTreePrefabs.Length > 0 || mountainPinePrefabs.Length > 0)
            {
                int treeAttempts = 240;
                for (int i = 0; i < treeAttempts; i++)
                {
                    float lx = NextRange(rng, 18f, TerrainWidth - 18f);
                    float lz = NextRange(rng, 18f, TerrainLength - 18f);
                    float normX = lx / TerrainWidth;
                    float normZ = lz / TerrainLength;

                    float hNorm = terrainData.GetInterpolatedHeight(normX, normZ) / TerrainHeight;
                    float steep = terrainData.GetSteepness(normX, normZ);

                    if (hNorm <= WaterHeightLevel + 0.018f || steep > 30f)
                        continue;

                    float worldX = gridX * TerrainWidth + lx;
                    float worldZ = gridZ * TerrainLength + lz;
                    float distToCenter = Vector2.Distance(new Vector2(worldX, worldZ), worldCenter);

                    // Chừa vùng lõi trung tâm bán kính 150m thoáng để người chơi xây thành trì, chỉ điểm xuyết vài cây cảnh
                    if (distToCenter < 150f && rng.NextDouble() < 0.88)
                        continue;

                    // Chọn rừng thông cho vùng núi cao bên ngoài sông, hoặc cây tán tròn/cổ thụ cho thảo nguyên & bờ sông
                    bool isMountainZone = hNorm > CastlePlainHeight + 0.045f;
                    GameObject[] pool = (isMountainZone && mountainPinePrefabs.Length > 0) ? mountainPinePrefabs : meadowTreePrefabs;
                    if (pool.Length == 0) continue;

                    float yPos = hNorm * TerrainHeight;
                    GameObject prefab = pool[rng.Next(pool.Length)];
                    float scale = isMountainZone ? NextRange(rng, 0.90f, 1.50f) : NextRange(rng, 0.85f, 1.35f);

                    SpawnProp(prefab, treeGroup, new Vector3(lx, yPos, lz), Vector3.up, NextRange(rng, 0f, 360f), scale, alignToNormal: false);
                }
            }

            // 4. ĐÁ TẢNG RÊU PHONG VEN BỜ SÔNG, CHÂN NÚI & THẢO NGUYÊN — 75 cụm đá mỗi mảnh Map
            if (rockPrefabs.Length > 0)
            {
                int rockAttempts = 110;
                for (int i = 0; i < rockAttempts; i++)
                {
                    float lx = NextRange(rng, 16f, TerrainWidth - 16f);
                    float lz = NextRange(rng, 16f, TerrainLength - 16f);
                    float normX = lx / TerrainWidth;
                    float normZ = lz / TerrainLength;

                    float hNorm = terrainData.GetInterpolatedHeight(normX, normZ) / TerrainHeight;
                    float steep = terrainData.GetSteepness(normX, normZ);

                    // Ưu tiên đặt đá ở ven bờ sông, chân núi hoặc rải rác ngoài rìa thảo nguyên
                    bool nearShore = Mathf.Abs(hNorm - WaterHeightLevel) < 0.032f;
                    bool onFoothill = hNorm > CastlePlainHeight + 0.01f && steep > 8f;

                    float worldX = gridX * TerrainWidth + lx;
                    float worldZ = gridZ * TerrainLength + lz;
                    float distToCenter = Vector2.Distance(new Vector2(worldX, worldZ), worldCenter);

                    if (!nearShore && !onFoothill && (distToCenter < 160f || rng.NextDouble() < 0.65))
                        continue;

                    float yPos = hNorm * TerrainHeight - 0.15f;
                    Vector3 normal = terrainData.GetInterpolatedNormal(normX, normZ);
                    GameObject prefab = rockPrefabs[rng.Next(rockPrefabs.Length)];
                    float scale = nearShore ? NextRange(rng, 0.65f, 1.35f) : NextRange(rng, 0.80f, 1.65f);

                    SpawnProp(prefab, rockGroup, new Vector3(lx, yPos, lz), normal, NextRange(rng, 0f, 360f), scale, alignToNormal: true);
                }
            }

            // 5. HOA SÚNG & BÈO NƯỚC TRÊN MẶT SÔNG UỐN LƯỢN — 90 cụm mỗi mảnh Map
            if (waterlilyPrefabs.Length > 0)
            {
                float waterY = WaterHeightLevel * TerrainHeight + 0.03f;
                int lilyAttempts = 220;
                for (int i = 0; i < lilyAttempts; i++)
                {
                    float lx = NextRange(rng, 15f, TerrainWidth - 15f);
                    float lz = NextRange(rng, 15f, TerrainLength - 15f);
                    float normX = lx / TerrainWidth;
                    float normZ = lz / TerrainLength;

                    float hNorm = terrainData.GetInterpolatedHeight(normX, normZ) / TerrainHeight;
                    // Đặt hoa súng ở vùng nước nông ven 2 bờ sông
                    if (hNorm < WaterHeightLevel - 0.004f && hNorm > WaterHeightLevel - 0.042f)
                    {
                        GameObject prefab = waterlilyPrefabs[rng.Next(waterlilyPrefabs.Length)];
                        float scale = NextRange(rng, 0.90f, 1.60f);
                        SpawnProp(prefab, lilyGroup, new Vector3(lx, waterY, lz), Vector3.up, NextRange(rng, 0f, 360f), scale, alignToNormal: false);
                    }
                }
            }
        }

        private static Transform CreateSubGroup(Transform parent, string groupName)
        {
            GameObject go = new GameObject(groupName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        private static void SpawnProp(GameObject prefab, Transform parent, Vector3 localPos, Vector3 normal, float yawDeg, float scale, bool alignToNormal)
        {
            if (prefab == null) return;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (inst == null)
            {
                inst = Object.Instantiate(prefab, parent);
            }
            inst.transform.localPosition = localPos;

            Quaternion rot = Quaternion.Euler(0f, yawDeg, 0f);
            if (alignToNormal)
            {
                Quaternion slopeRot = Quaternion.FromToRotation(Vector3.up, Vector3.Lerp(Vector3.up, normal, 0.55f));
                rot = slopeRot * rot;
            }
            inst.transform.localRotation = rot;
            inst.transform.localScale = Vector3.one * scale;
        }

        private static GameObject[] LoadPrefabList(string[] paths)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (string p in paths)
            {
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (asset != null) list.Add(asset);
            }
            return list.ToArray();
        }

        private static float NextRange(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }
    }
}

