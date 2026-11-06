using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using LostCave.Audio;
using LostCave.Camera;
using LostCave.Player;
using LostCave.Obstacles;
using LostCave.Interaction;
using LostCave.Environment;
using LostCave.Managers;
using LostCave.UI;

namespace LostCave.Editor
{
    public class CaveGameBuilder : UnityEditor.Editor
    {
        private const string MAT_DIR = "Assets/Materials";
        private const string PREFAB_DIR = "Assets/Prefabs";
        private const string SCENE_DIR = "Assets/Scenes";
        private const string AUDIO_DIR = "Assets/Audio";
        private const string TEX_DIR = "Assets/Textures";

        private static Material matL1Rock;
        private static Material matL1Path;
        private static Material matL2Rock;
        private static Material matL2Path;
        private static Material matL3Rock;
        private static Material matL3Path;
        private static Material matRock;
        private static Material matPath;
        private static Material matTurquoise;
        private static Material matEmerald;
        private static Material matTorchWood;
        private static Material matTorchFlame;
        private static Material matGold;
        private static Material matWater;
        private static Material matBridge;
        private static Material matWarning;
        private static Material matPressurePlate;
        private static Material matSpikes;
        private static Material matGirlSkin;
        private static Material matGirlHair;
        private static Material matGirlTop;
        private static Material matGirlShorts;
        private static Material matGirlBoots;
        private static Material matGirlRibbon;
        private static Material matGirlSocks;
        private static Material matGirlEyes;
        private static Material matGirlBlush;

        [MenuItem("Lost Cave/PLAY GAME NOW (Starts Stage 1 Instantly)")]
        public static void PlayGameNow()
        {
            EditorSceneManager.OpenScene(SCENE_DIR + "/CaveLevel1.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Lost Cave/PLAY FROM MAIN MENU")]
        public static void PlayFromMainMenu()
        {
            EditorSceneManager.OpenScene(SCENE_DIR + "/MainMenu.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Lost Cave/Build Standalone Windows Game (.exe)")]
        public static void BuildWindowsExe()
        {
            string buildFolder = "Build";
            if (!Directory.Exists(buildFolder)) Directory.CreateDirectory(buildFolder);
            string exePath = buildFolder + "/TreasureHunt_LostCave.exe";

            string[] scenes = new string[] {
                SCENE_DIR + "/MainMenu.unity",
                SCENE_DIR + "/LevelSelection.unity",
                SCENE_DIR + "/Instructions.unity",
                SCENE_DIR + "/CaveLevel1.unity",
                SCENE_DIR + "/CaveLevel2.unity",
                SCENE_DIR + "/CaveLevel3.unity",
                SCENE_DIR + "/GameComplete.unity"
            };

            BuildPlayerOptions bpo = new BuildPlayerOptions();
            bpo.scenes = scenes;
            bpo.locationPathName = exePath;
            bpo.target = BuildTarget.StandaloneWindows64;
            bpo.options = BuildOptions.None;

            Debug.Log(">>> [Lost Cave] Building Windows Standalone Executable to: " + exePath);
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(bpo);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log(">>> [Lost Cave] Build Succeeded! Executable created at: " + exePath);
                EditorUtility.RevealInFinder(exePath);
            }
            else
            {
                Debug.LogError(">>> [Lost Cave] Build Failed: " + report.summary.totalErrors + " errors.");
            }
        }

        [MenuItem("Lost Cave/Build Entire Game")]
        public static void BuildEntireGame()
        {
            Debug.Log(">>> [Lost Cave] Starting Full Game Build with Long Winding Turn Paths...");
            EditorUtility.DisplayProgressBar("Lost Cave Builder", "Creating Materials...", 0.1f);

            CreateMaterials();

            EditorUtility.DisplayProgressBar("Lost Cave Builder", "Creating Prefabs...", 0.3f);
            CreatePrefabs();

            EditorUtility.DisplayProgressBar("Lost Cave Builder", "Building Scenes...", 0.6f);
            BuildAllScenes();

            EditorUtility.DisplayProgressBar("Lost Cave Builder", "Configuring Build Settings...", 0.9f);
            ConfigureBuildSettings();

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(">>> [Lost Cave] Full Game Build Completed Successfully!");
        }

        [MenuItem("Lost Cave/Open Scene/1. Main Menu")]
        public static void OpenMainMenu() => EditorSceneManager.OpenScene(SCENE_DIR + "/MainMenu.unity");

        [MenuItem("Lost Cave/Open Scene/2. Level Selection")]
        public static void OpenLevelSelect() => EditorSceneManager.OpenScene(SCENE_DIR + "/LevelSelection.unity");

        [MenuItem("Lost Cave/Open Scene/3. Instructions")]
        public static void OpenInstructions() => EditorSceneManager.OpenScene(SCENE_DIR + "/Instructions.unity");

        [MenuItem("Lost Cave/Open Scene/4. Stage 1 - Cave Entrance")]
        public static void OpenStage1() => EditorSceneManager.OpenScene(SCENE_DIR + "/CaveLevel1.unity");

        [MenuItem("Lost Cave/Open Scene/5. Stage 2 - Crystal Tunnels")]
        public static void OpenStage2() => EditorSceneManager.OpenScene(SCENE_DIR + "/CaveLevel2.unity");

        [MenuItem("Lost Cave/Open Scene/6. Stage 3 - Treasure Chamber")]
        public static void OpenStage3() => EditorSceneManager.OpenScene(SCENE_DIR + "/CaveLevel3.unity");

        [MenuItem("Lost Cave/Open Scene/7. Game Complete")]
        public static void OpenGameComplete() => EditorSceneManager.OpenScene(SCENE_DIR + "/GameComplete.unity");

        private static Shader GetLitShader()
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            return s;
        }

        private static Material CreateLitMaterial(string name, Color baseColor, float metallic = 0.1f, float smoothness = 0.2f, Color? emissionColor = null, Texture2D texture = null, Vector2? tiling = null)
        {
            string path = MAT_DIR + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(GetLitShader());
                mat.name = name;
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = GetLitShader();
            mat.color = baseColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (texture != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
                Vector2 tile = tiling ?? new Vector2(4f, 4f);
                if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", tile);
                if (mat.HasProperty("_MainTex")) mat.SetTextureScale("_MainTex", tile);
            }

            if (emissionColor.HasValue && emissionColor.Value != Color.black)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void GenerateProceduralTextures()
        {
            if (!Directory.Exists(TEX_DIR)) Directory.CreateDirectory(TEX_DIR);

            CreateNoiseTexture(TEX_DIR + "/Tex_CaveRock_L1.png", 256, 256, (x, y) =>
            {
                float n1 = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float n2 = Mathf.PerlinNoise(x * 0.12f + 23.4f, y * 0.12f + 45.1f);
                float n3 = Mathf.PerlinNoise(x * 0.28f + 81.2f, y * 0.28f + 12.8f);
                float rock = n1 * 0.55f + n2 * 0.32f + n3 * 0.13f;
                float crack = Mathf.Abs(Mathf.Sin(x * 0.06f + n1 * 5f) * Mathf.Cos(y * 0.06f + n2 * 5f));
                float crackFactor = crack < 0.12f ? Mathf.Lerp(0.35f, 1f, crack / 0.12f) : 1f;
                Color baseCol = new Color(0.48f, 0.44f, 0.38f);
                return baseCol * (0.65f + rock * 0.70f) * crackFactor;
            });

            CreateNoiseTexture(TEX_DIR + "/Tex_StonePath_L1.png", 256, 256, (x, y) =>
            {
                int row = y / 32;
                int shift = (row % 2 == 1) ? 16 : 0;
                int col = (x + shift) / 32;
                float cx = (x + shift) % 32;
                float cy = y % 32;
                float dist = Mathf.Min(Mathf.Min(cx, 32f - cx), Mathf.Min(cy, 32f - cy));
                float mortar = Mathf.Clamp01((dist - 2.5f) / 3f);
                float bump = Mathf.Sin((cx / 32f) * Mathf.PI) * Mathf.Sin((cy / 32f) * Mathf.PI);
                float stoneHash = Mathf.PerlinNoise(col * 3.7f, row * 4.9f);
                Color stoneCol = Color.Lerp(new Color(0.42f, 0.40f, 0.36f), new Color(0.58f, 0.54f, 0.48f), stoneHash);
                Color mortarCol = new Color(0.18f, 0.16f, 0.14f);
                return Color.Lerp(mortarCol, stoneCol * (0.75f + bump * 0.45f), mortar);
            });

            CreateNoiseTexture(TEX_DIR + "/Tex_CrystalRock_L2.png", 256, 256, (x, y) =>
            {
                float baseNoise = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                Color rockBase = new Color(0.08f, 0.10f, 0.14f) * (0.8f + baseNoise * 0.4f);
                float v1 = Mathf.PerlinNoise(x * 0.035f + 14f, y * 0.035f + 38f);
                float v2 = Mathf.PerlinNoise(x * 0.07f + 62f, y * 0.07f + 91f);
                float veinVal = Mathf.Abs(Mathf.Sin((x * 0.05f + v1 * 6f) + (y * 0.03f + v2 * 5f)));
                if (veinVal < 0.16f)
                {
                    float t = veinVal / 0.16f;
                    Color veinCore = new Color(0.2f, 1.0f, 0.95f);
                    Color veinEdge = new Color(0.03f, 0.45f, 0.65f);
                    Color veinCol = Color.Lerp(veinCore, veinEdge, t);
                    return Color.Lerp(veinCol, rockBase, t);
                }
                float glint = Mathf.PerlinNoise(x * 0.8f, y * 0.8f);
                if (glint > 0.84f) rockBase += new Color(0.15f, 0.75f, 0.85f);
                return rockBase;
            });

            CreateNoiseTexture(TEX_DIR + "/Tex_CrystalPath_L2.png", 256, 256, (x, y) =>
            {
                float tx = x % 32; float ty = y % 32;
                float edge = Mathf.Min(Mathf.Min(tx, 32f - tx), Mathf.Min(ty, 32f - ty));
                float border = Mathf.Clamp01((edge - 2f) / 2.5f);
                Color slateBase = new Color(0.14f, 0.18f, 0.22f);
                float grain = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.2f;
                Color tileCol = slateBase * (0.9f + grain);
                Color crystalBorder = new Color(0.1f, 0.85f, 0.95f);
                return Color.Lerp(crystalBorder, tileCol, border);
            });

            CreateNoiseTexture(TEX_DIR + "/Tex_TempleRock_L3.png", 256, 256, (x, y) =>
            {
                int row = y / 32;
                int shift = (row % 2 == 1) ? 32 : 0;
                int col = (x + shift) / 64;
                float bx = (x + shift) % 64;
                float by = y % 32;
                float edge = Mathf.Min(Mathf.Min(bx, 64f - bx), Mathf.Min(by, 32f - by));
                float mortar = Mathf.Clamp01((edge - 2.5f) / 2.5f);
                float brickHash = Mathf.PerlinNoise(col * 4.3f, row * 5.1f);
                Color brickCol = Color.Lerp(new Color(0.58f, 0.46f, 0.30f), new Color(0.48f, 0.36f, 0.22f), brickHash);
                float grain = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.2f;
                brickCol = brickCol * (0.9f + grain);
                float rx = Mathf.Abs(bx - 32f); float ry = Mathf.Abs(by - 16f);
                bool isRune = (rx < 14f && ry < 8f) && ((Mathf.Abs(rx - ry) < 1.8f) || (rx < 1.8f) || (ry < 1.8f));
                if (isRune && mortar > 0.5f) brickCol = new Color(0.95f, 0.80f, 0.28f);
                Color mortarCol = new Color(0.20f, 0.14f, 0.08f);
                return Color.Lerp(mortarCol, brickCol, mortar);
            });

            CreateNoiseTexture(TEX_DIR + "/Tex_TemplePath_L3.png", 256, 256, (x, y) =>
            {
                float tx = x % 64; float ty = y % 64;
                float borderDist = Mathf.Min(Mathf.Min(tx, 64f - tx), Mathf.Min(ty, 64f - ty));
                Color stoneBase = new Color(0.60f, 0.50f, 0.36f);
                if (borderDist < 2.5f) return new Color(0.22f, 0.16f, 0.10f); // mortar
                if (borderDist >= 6f && borderDist <= 9f) return new Color(0.95f, 0.80f, 0.25f); // gold inlay
                float polish = Mathf.Sin((tx / 64f) * Mathf.PI) * Mathf.Sin((ty / 64f) * Mathf.PI);
                return stoneBase * (0.85f + polish * 0.3f);
            });

            CreateNoiseTexture(TEX_DIR + "/Tex_GoldCoins.png", 256, 256, (x, y) =>
            {
                float minCoinDist = 999f;
                for (int gx = 0; gx < 8; gx++)
                {
                    for (int gy = 0; gy < 8; gy++)
                    {
                        float cx = gx * 32f + 16f + Mathf.Sin(gx * 3.1f + gy * 4.7f) * 6f;
                        float cy = gy * 32f + 16f + Mathf.Cos(gx * 5.3f + gy * 2.9f) * 6f;
                        float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        if (d < minCoinDist) minCoinDist = d;
                    }
                }
                float coinRadius = 14f;
                if (minCoinDist < coinRadius)
                {
                    float edgeT = minCoinDist / coinRadius;
                    float rim = (edgeT > 0.75f && edgeT < 0.95f) ? 0.35f : 0f;
                    return Color.Lerp(new Color(1.0f, 0.90f, 0.35f) + new Color(rim, rim, rim), new Color(0.55f, 0.40f, 0.10f), edgeT * 0.7f);
                }
                return new Color(0.35f, 0.24f, 0.05f);
            });

            AssetDatabase.Refresh();
        }

        private static void CreateNoiseTexture(string path, int width, int height, Func<int, int, Color> pixelFunc)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = pixelFunc(x, y);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static void CreateMaterials()
        {
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
            if (!Directory.Exists(TEX_DIR)) Directory.CreateDirectory(TEX_DIR);

            GenerateProceduralTextures();

            Texture2D texL1Rock = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_CaveRock_L1.png");
            Texture2D texL1Path = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_StonePath_L1.png");
            Texture2D texL2Rock = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_CrystalRock_L2.png");
            Texture2D texL2Path = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_CrystalPath_L2.png");
            Texture2D texL3Rock = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_TempleRock_L3.png");
            Texture2D texL3Path = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_TemplePath_L3.png");
            Texture2D texGold   = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + "/Tex_GoldCoins.png");

            // STAGE 1: Natural Earthy Limestone Cave
            matL1Rock = CreateLitMaterial("Mat_L1_CaveRock", new Color(0.85f, 0.80f, 0.75f), 0.05f, 0.15f, null, texL1Rock, new Vector2(4f, 4f));
            matL1Path = CreateLitMaterial("Mat_L1_StonePath", new Color(0.85f, 0.85f, 0.85f), 0.10f, 0.22f, null, texL1Path, new Vector2(4f, 4f));

            // STAGE 2: Bioluminescent Crystal Caverns (Glowing Turquoise & Emerald)
            matL2Rock = CreateLitMaterial("Mat_L2_CrystalRock", new Color(0.95f, 1.05f, 1.10f), 0.25f, 0.55f, new Color(0.08f, 0.45f, 0.55f) * 1.6f, texL2Rock, new Vector2(4f, 4f));
            matL2Path = CreateLitMaterial("Mat_L2_CrystalPath", new Color(0.95f, 1.05f, 1.10f), 0.30f, 0.60f, new Color(0.06f, 0.35f, 0.48f) * 1.4f, texL2Path, new Vector2(4f, 4f));

            // STAGE 3: Ancient Sandstone Temple & Royal Golden Tomb
            matL3Rock = CreateLitMaterial("Mat_L3_TempleRock", new Color(1.05f, 0.95f, 0.82f), 0.15f, 0.28f, new Color(0.25f, 0.18f, 0.06f) * 1.5f, texL3Rock, new Vector2(4f, 4f));
            matL3Path = CreateLitMaterial("Mat_L3_TemplePath", new Color(1.05f, 0.98f, 0.88f), 0.22f, 0.42f, null, texL3Path, new Vector2(4f, 4f));

            // General & Fallback Materials
            matRock = matL1Rock;
            matPath = matL1Path;
            matTurquoise = CreateLitMaterial("Mat_CrystalTurquoise", new Color(0.1f, 0.9f, 0.95f), 0.2f, 0.9f, new Color(0.1f, 1.2f, 1.4f) * 1.8f);
            matEmerald = CreateLitMaterial("Mat_CrystalEmerald", new Color(0.1f, 0.95f, 0.5f), 0.2f, 0.9f, new Color(0.1f, 1.5f, 0.6f) * 1.8f);
            matTorchWood = CreateLitMaterial("Mat_TorchWood", new Color(0.3f, 0.18f, 0.1f), 0.0f, 0.1f);
            matTorchFlame = CreateLitMaterial("Mat_TorchFlame", new Color(1.0f, 0.65f, 0.1f), 0.0f, 0.1f, new Color(1.8f, 0.9f, 0.1f) * 2.5f);
            matGold = CreateLitMaterial("Mat_TreasureGold", new Color(1.0f, 0.85f, 0.25f), 0.95f, 0.90f, new Color(0.45f, 0.32f, 0.08f) * 1.8f, texGold, new Vector2(3f, 3f));
            matWater = CreateLitMaterial("Mat_UndergroundWater", new Color(0.1f, 0.38f, 0.5f, 0.88f), 0.35f, 0.95f);
            matBridge = CreateLitMaterial("Mat_BridgeStone", new Color(0.55f, 0.50f, 0.45f), 0.1f, 0.2f, null, texL1Rock, new Vector2(2f, 2f));
            matWarning = CreateLitMaterial("Mat_RedWarning", new Color(1.0f, 0.12f, 0.12f, 0.75f), 0.0f, 0.5f, Color.red * 2.5f);
            matPressurePlate = CreateLitMaterial("Mat_PressurePlate", new Color(0.55f, 0.50f, 0.42f), 0.25f, 0.35f, null, texL1Path, new Vector2(1f, 1f));
            matSpikes = CreateLitMaterial("Mat_IronSpikes", new Color(0.2f, 0.22f, 0.25f), 0.88f, 0.75f);

            matGirlSkin = CreateLitMaterial("Mat_GirlSkin", new Color(0.98f, 0.82f, 0.72f), 0.1f, 0.35f);
            matGirlHair = CreateLitMaterial("Mat_GirlHair", new Color(0.42f, 0.22f, 0.10f), 0.2f, 0.6f);
            matGirlTop = CreateLitMaterial("Mat_GirlTop", new Color(0.96f, 0.32f, 0.54f), 0.15f, 0.45f);
            matGirlShorts = CreateLitMaterial("Mat_GirlShorts", new Color(0.20f, 0.28f, 0.44f), 0.05f, 0.25f);
            matGirlBoots = CreateLitMaterial("Mat_GirlBoots", new Color(0.32f, 0.18f, 0.10f), 0.1f, 0.35f);
            matGirlRibbon = CreateLitMaterial("Mat_GirlRibbon", new Color(1.0f, 0.20f, 0.65f), 0.1f, 0.8f, new Color(1.0f, 0.20f, 0.65f) * 1.5f);
            matGirlSocks = CreateLitMaterial("Mat_GirlSocks", new Color(0.96f, 0.96f, 0.98f), 0.05f, 0.1f);
            matGirlEyes = CreateLitMaterial("Mat_GirlEyes", new Color(0.2f, 0.6f, 1.0f), 0.3f, 0.9f, new Color(0.1f, 0.4f, 0.8f) * 1.5f);
            matGirlBlush = CreateLitMaterial("Mat_GirlBlush", new Color(1.0f, 0.55f, 0.65f), 0.05f, 0.2f);
        }

        private static AudioClip LoadAudio(string subPath)
        {
            string fullPath = AUDIO_DIR + "/" + subPath;
            return AssetDatabase.LoadAssetAtPath<AudioClip>(fullPath);
        }

        private static void CreatePrefabs()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);

            CreatePlayerPrefab();
            CreateTorchPrefab();
            CreateWallBrazierPrefab();
            CreateCrystalSconcePrefab();
            CreateCrystalPrefabs();
            CreateCollapsingTilePrefab();
            CreateFallingRockPrefab();
            CreatePressurePlatePrefab();
            CreateSwingingStonePrefab();
            CreateTreasureChestPrefab();
            CreateAerialEnergyRingPrefab();
            CreateCrystalLaserBarrierPrefab();
            CreateRollerCoasterCartPrefab();
            CreateCoasterObstaclePrefabs();
        }

        private static void CreatePlayerPrefab()
        {
            string path = PREFAB_DIR + "/Player.prefab";
            GameObject root = new GameObject("Player");
            root.tag = "Player";

            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.80f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.90f, 0f);

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            PlayerController pc = root.AddComponent<PlayerController>();
            pc.walkSpeed = 5.2f;
            pc.runSpeed = 8.5f;
            pc.jumpHeight = 1.85f;
            pc.gravity = -24f;

            pc.footstepWalkClip = LoadAudio("Player/FootstepWalk.wav");
            pc.footstepRunClip = LoadAudio("Player/FootstepRun.wav");
            pc.jumpClip = LoadAudio("Player/Jump.wav");
            pc.landClip = LoadAudio("Player/Land.wav");
            pc.hurtClip = LoadAudio("Obstacles/PlayerHit.wav");

            GameObject rigObj = new GameObject("VisualRig");
            rigObj.transform.SetParent(root.transform);
            rigObj.transform.localPosition = Vector3.zero;
            ExplorerVisualRig rig = rigObj.AddComponent<ExplorerVisualRig>();
            pc.visualRig = rig;

            // ==========================================
            // 1. HIPS & WAIST (Denim Shorts + Utility Belt + Side Satchel)
            // ==========================================
            GameObject hips = new GameObject("Hips");
            hips.transform.SetParent(rigObj.transform);
            hips.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            rig.hips = hips.transform;

            // Denim Shorts (Pelvis) - Feminine tapered cut
            GameObject shortsObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shortsObj.name = "DenimShorts";
            shortsObj.transform.SetParent(hips.transform);
            shortsObj.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            shortsObj.transform.localScale = new Vector3(0.36f, 0.22f, 0.26f);
            if (shortsObj.GetComponent<Collider>()) DestroyImmediate(shortsObj.GetComponent<Collider>());
            shortsObj.GetComponent<Renderer>().sharedMaterial = matGirlShorts;

            // Left Short Leg Cuff
            GameObject cuffL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuffL.name = "ShortCuff_L";
            cuffL.transform.SetParent(hips.transform);
            cuffL.transform.localPosition = new Vector3(-0.11f, -0.12f, 0f);
            cuffL.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);
            if (cuffL.GetComponent<Collider>()) DestroyImmediate(cuffL.GetComponent<Collider>());
            cuffL.GetComponent<Renderer>().sharedMaterial = matGirlShorts;

            // Right Short Leg Cuff
            GameObject cuffR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuffR.name = "ShortCuff_R";
            cuffR.transform.SetParent(hips.transform);
            cuffR.transform.localPosition = new Vector3(0.11f, -0.12f, 0f);
            cuffR.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);
            if (cuffR.GetComponent<Collider>()) DestroyImmediate(cuffR.GetComponent<Collider>());
            cuffR.GetComponent<Renderer>().sharedMaterial = matGirlShorts;

            // Leather Utility Belt
            GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            belt.name = "UtilityBelt";
            belt.transform.SetParent(hips.transform);
            belt.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            belt.transform.localScale = new Vector3(0.38f, 0.05f, 0.28f);
            if (belt.GetComponent<Collider>()) DestroyImmediate(belt.GetComponent<Collider>());
            belt.GetComponent<Renderer>().sharedMaterial = matGirlBoots;

            // Golden Belt Buckle
            GameObject buckle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buckle.name = "GoldBuckle";
            buckle.transform.SetParent(hips.transform);
            buckle.transform.localPosition = new Vector3(0f, 0.08f, 0.145f);
            buckle.transform.localScale = new Vector3(0.09f, 0.07f, 0.03f);
            if (buckle.GetComponent<Collider>()) DestroyImmediate(buckle.GetComponent<Collider>());
            buckle.GetComponent<Renderer>().sharedMaterial = matGold;

            // Cute Adventurer Side Hip Satchel (Mounted on right hip, leaving back completely unobstructed!)
            GameObject satchel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            satchel.name = "HipSatchel";
            satchel.transform.SetParent(hips.transform);
            satchel.transform.localPosition = new Vector3(0.20f, -0.02f, 0.02f);
            satchel.transform.localRotation = Quaternion.Euler(0f, -12f, -8f);
            satchel.transform.localScale = new Vector3(0.10f, 0.16f, 0.14f);
            if (satchel.GetComponent<Collider>()) DestroyImmediate(satchel.GetComponent<Collider>());
            satchel.GetComponent<Renderer>().sharedMaterial = matGirlBoots;
            rig.backpack = satchel.transform;

            GameObject satchelClasp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            satchelClasp.name = "SatchelGoldClasp";
            satchelClasp.transform.SetParent(satchel.transform);
            satchelClasp.transform.localPosition = new Vector3(0.52f, 0f, 0f);
            satchelClasp.transform.localScale = new Vector3(0.2f, 0.35f, 0.3f);
            if (satchelClasp.GetComponent<Collider>()) DestroyImmediate(satchelClasp.GetComponent<Collider>());
            satchelClasp.GetComponent<Renderer>().sharedMaterial = matGold;

            // ==========================================
            // 2. TORSO / SPINE (Named "Torso" - Crop Tank Top + Bare Midriff)
            // ==========================================
            GameObject spine = new GameObject("Torso");
            spine.transform.SetParent(hips.transform);
            spine.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            rig.spine = spine.transform;

            // Slender Feminine Bare Midriff
            GameObject midriff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            midriff.name = "MidriffSkin";
            midriff.transform.SetParent(spine.transform);
            midriff.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            midriff.transform.localScale = new Vector3(0.28f, 0.12f, 0.20f);
            if (midriff.GetComponent<Collider>()) DestroyImmediate(midriff.GetComponent<Collider>());
            midriff.GetComponent<Renderer>().sharedMaterial = matGirlSkin;

            // Coral-Pink Explorer Tank Top
            GameObject tankTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tankTop.name = "CoralPinkTankTop";
            tankTop.transform.SetParent(spine.transform);
            tankTop.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            tankTop.transform.localScale = new Vector3(0.33f, 0.22f, 0.23f);
            if (tankTop.GetComponent<Collider>()) DestroyImmediate(tankTop.GetComponent<Collider>());
            tankTop.GetComponent<Renderer>().sharedMaterial = matGirlTop;

            // Left Tank Top Shoulder Strap
            GameObject strapL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strapL.name = "Strap_L";
            strapL.transform.SetParent(spine.transform);
            strapL.transform.localPosition = new Vector3(-0.11f, 0.36f, 0f);
            strapL.transform.localScale = new Vector3(0.06f, 0.10f, 0.18f);
            if (strapL.GetComponent<Collider>()) DestroyImmediate(strapL.GetComponent<Collider>());
            strapL.GetComponent<Renderer>().sharedMaterial = matGirlTop;

            // Right Tank Top Shoulder Strap
            GameObject strapR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strapR.name = "Strap_R";
            strapR.transform.SetParent(spine.transform);
            strapR.transform.localPosition = new Vector3(0.11f, 0.36f, 0f);
            strapR.transform.localScale = new Vector3(0.06f, 0.10f, 0.18f);
            if (strapR.GetComponent<Collider>()) DestroyImmediate(strapR.GetComponent<Collider>());
            strapR.GetComponent<Renderer>().sharedMaterial = matGirlTop;

            // Back Halter Cross Accent (Visible from 3rd person behind!)
            GameObject halterBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            halterBack.name = "HalterBack";
            halterBack.transform.SetParent(spine.transform);
            halterBack.transform.localPosition = new Vector3(0f, 0.26f, -0.115f);
            halterBack.transform.localScale = new Vector3(0.18f, 0.14f, 0.04f);
            if (halterBack.GetComponent<Collider>()) DestroyImmediate(halterBack.GetComponent<Collider>());
            halterBack.GetComponent<Renderer>().sharedMaterial = matGirlTop;

            // Glowing Crystal Heart Pendant Necklace
            GameObject necklace = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            necklace.name = "CrystalHeartNecklace";
            necklace.transform.SetParent(spine.transform);
            necklace.transform.localPosition = new Vector3(0f, 0.28f, 0.13f);
            necklace.transform.localScale = new Vector3(0.09f, 0.10f, 0.06f);
            if (necklace.GetComponent<Collider>()) DestroyImmediate(necklace.GetComponent<Collider>());
            necklace.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            // ==========================================
            // 3. HEAD & GORGEOUS FLOWING PONYTAIL HAIRSTYLE
            // ==========================================
            GameObject head = new GameObject("Head");
            head.transform.SetParent(spine.transform);
            head.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            rig.head = head.transform;

            // Cute Girl Face (Peach Skin)
            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            face.name = "GirlFace";
            face.transform.SetParent(head.transform);
            face.transform.localPosition = Vector3.zero;
            face.transform.localScale = new Vector3(0.36f, 0.38f, 0.36f);
            if (face.GetComponent<Collider>()) DestroyImmediate(face.GetComponent<Collider>());
            face.GetComponent<Renderer>().sharedMaterial = matGirlSkin;

            // Sparkling Blue Eyes (Left & Right)
            GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "Eye_L";
            eyeL.transform.SetParent(head.transform);
            eyeL.transform.localPosition = new Vector3(-0.08f, 0.03f, 0.16f);
            eyeL.transform.localScale = new Vector3(0.06f, 0.07f, 0.04f);
            if (eyeL.GetComponent<Collider>()) DestroyImmediate(eyeL.GetComponent<Collider>());
            eyeL.GetComponent<Renderer>().sharedMaterial = matGirlEyes;

            GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeR.name = "Eye_R";
            eyeR.transform.SetParent(head.transform);
            eyeR.transform.localPosition = new Vector3(0.08f, 0.03f, 0.16f);
            eyeR.transform.localScale = new Vector3(0.06f, 0.07f, 0.04f);
            if (eyeR.GetComponent<Collider>()) DestroyImmediate(eyeR.GetComponent<Collider>());
            eyeR.GetComponent<Renderer>().sharedMaterial = matGirlEyes;

            // Rosy Blush Cheeks
            GameObject blushL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blushL.name = "Blush_L";
            blushL.transform.SetParent(head.transform);
            blushL.transform.localPosition = new Vector3(-0.13f, -0.04f, 0.13f);
            blushL.transform.localScale = new Vector3(0.06f, 0.04f, 0.03f);
            if (blushL.GetComponent<Collider>()) DestroyImmediate(blushL.GetComponent<Collider>());
            blushL.GetComponent<Renderer>().sharedMaterial = matGirlBlush;

            GameObject blushR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blushR.name = "Blush_R";
            blushR.transform.SetParent(head.transform);
            blushR.transform.localPosition = new Vector3(0.13f, -0.04f, 0.13f);
            blushR.transform.localScale = new Vector3(0.06f, 0.04f, 0.03f);
            if (blushR.GetComponent<Collider>()) DestroyImmediate(blushR.GetComponent<Collider>());
            blushR.GetComponent<Renderer>().sharedMaterial = matGirlBlush;

            // Sweet Pink Smile
            GameObject lips = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lips.name = "PinkLips";
            lips.transform.SetParent(head.transform);
            lips.transform.localPosition = new Vector3(0f, -0.09f, 0.17f);
            lips.transform.localScale = new Vector3(0.08f, 0.025f, 0.02f);
            if (lips.GetComponent<Collider>()) DestroyImmediate(lips.GetComponent<Collider>());
            lips.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            // Shiny Gold Hoop Earrings
            GameObject earringL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            earringL.name = "GoldEarring_L";
            earringL.transform.SetParent(head.transform);
            earringL.transform.localPosition = new Vector3(-0.19f, -0.04f, 0.01f);
            earringL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            earringL.transform.localScale = new Vector3(0.07f, 0.015f, 0.07f);
            if (earringL.GetComponent<Collider>()) DestroyImmediate(earringL.GetComponent<Collider>());
            earringL.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject earringR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            earringR.name = "GoldEarring_R";
            earringR.transform.SetParent(head.transform);
            earringR.transform.localPosition = new Vector3(0.19f, -0.04f, 0.01f);
            earringR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            earringR.transform.localScale = new Vector3(0.07f, 0.015f, 0.07f);
            if (earringR.GetComponent<Collider>()) DestroyImmediate(earringR.GetComponent<Collider>());
            earringR.GetComponent<Renderer>().sharedMaterial = matGold;

            // Brunette Hair Crown (Smooth, voluminous rounded hairstyle)
            GameObject hairCrown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hairCrown.name = "HairCrown";
            hairCrown.transform.SetParent(head.transform);
            hairCrown.transform.localPosition = new Vector3(0f, 0.06f, -0.03f);
            hairCrown.transform.localScale = new Vector3(0.39f, 0.38f, 0.40f);
            if (hairCrown.GetComponent<Collider>()) DestroyImmediate(hairCrown.GetComponent<Collider>());
            hairCrown.GetComponent<Renderer>().sharedMaterial = matGirlHair;

            // Side Bangs framing face
            GameObject bangL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bangL.name = "SideBang_L";
            bangL.transform.SetParent(head.transform);
            bangL.transform.localPosition = new Vector3(-0.17f, -0.06f, 0.09f);
            bangL.transform.localRotation = Quaternion.Euler(-15f, 20f, 15f);
            bangL.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
            if (bangL.GetComponent<Collider>()) DestroyImmediate(bangL.GetComponent<Collider>());
            bangL.GetComponent<Renderer>().sharedMaterial = matGirlHair;

            GameObject bangR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bangR.name = "SideBang_R";
            bangR.transform.SetParent(head.transform);
            bangR.transform.localPosition = new Vector3(0.17f, -0.06f, 0.09f);
            bangR.transform.localRotation = Quaternion.Euler(-15f, -20f, -15f);
            bangR.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
            if (bangR.GetComponent<Collider>()) DestroyImmediate(bangR.GetComponent<Collider>());
            bangR.GetComponent<Renderer>().sharedMaterial = matGirlHair;

            // -------------------------------------------------------------
            // PROMINENT MAGENTA HAIR BOW & RIBBON (Eye-catching from behind!)
            // -------------------------------------------------------------
            GameObject bowKnot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bowKnot.name = "BowKnot";
            bowKnot.transform.SetParent(head.transform);
            bowKnot.transform.localPosition = new Vector3(0f, 0.14f, -0.21f);
            bowKnot.transform.localScale = new Vector3(0.09f, 0.09f, 0.08f);
            if (bowKnot.GetComponent<Collider>()) DestroyImmediate(bowKnot.GetComponent<Collider>());
            bowKnot.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            GameObject bowWingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bowWingL.name = "BowWing_L";
            bowWingL.transform.SetParent(head.transform);
            bowWingL.transform.localPosition = new Vector3(-0.13f, 0.17f, -0.21f);
            bowWingL.transform.localRotation = Quaternion.Euler(15f, -10f, 30f);
            bowWingL.transform.localScale = new Vector3(0.18f, 0.10f, 0.05f);
            if (bowWingL.GetComponent<Collider>()) DestroyImmediate(bowWingL.GetComponent<Collider>());
            bowWingL.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            GameObject bowWingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bowWingR.name = "BowWing_R";
            bowWingR.transform.SetParent(head.transform);
            bowWingR.transform.localPosition = new Vector3(0.13f, 0.17f, -0.21f);
            bowWingR.transform.localRotation = Quaternion.Euler(15f, 10f, -30f);
            bowWingR.transform.localScale = new Vector3(0.18f, 0.10f, 0.05f);
            if (bowWingR.GetComponent<Collider>()) DestroyImmediate(bowWingR.GetComponent<Collider>());
            bowWingR.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            // -------------------------------------------------------------
            // LONG FLOWING CASCADING PONYTAIL (Extending far down the back!)
            // -------------------------------------------------------------
            // Upper Ponytail Segment (angled back and down)
            GameObject ponyUpper = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ponyUpper.name = "PonytailUpper";
            ponyUpper.transform.SetParent(head.transform);
            ponyUpper.transform.localPosition = new Vector3(0f, 0.02f, -0.27f);
            ponyUpper.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
            ponyUpper.transform.localScale = new Vector3(0.17f, 0.22f, 0.17f);
            if (ponyUpper.GetComponent<Collider>()) DestroyImmediate(ponyUpper.GetComponent<Collider>());
            ponyUpper.GetComponent<Renderer>().sharedMaterial = matGirlHair;

            // Mid Hair Ribbon Band
            GameObject ponyBandMid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ponyBandMid.name = "PonytailBandMid";
            ponyBandMid.transform.SetParent(head.transform);
            ponyBandMid.transform.localPosition = new Vector3(0f, -0.16f, -0.32f);
            ponyBandMid.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f);
            ponyBandMid.transform.localScale = new Vector3(0.17f, 0.04f, 0.17f);
            if (ponyBandMid.GetComponent<Collider>()) DestroyImmediate(ponyBandMid.GetComponent<Collider>());
            ponyBandMid.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            // Mid Ponytail Segment (cascading thick curls down past her neck)
            GameObject ponyMid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ponyMid.name = "PonytailMid";
            ponyMid.transform.SetParent(head.transform);
            ponyMid.transform.localPosition = new Vector3(0f, -0.34f, -0.34f);
            ponyMid.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            ponyMid.transform.localScale = new Vector3(0.16f, 0.24f, 0.16f);
            if (ponyMid.GetComponent<Collider>()) DestroyImmediate(ponyMid.GetComponent<Collider>());
            ponyMid.GetComponent<Renderer>().sharedMaterial = matGirlHair;

            // Ponytail Flowing Tail Tip (reaching down toward mid-back)
            GameObject ponyTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ponyTip.name = "PonytailTip";
            ponyTip.transform.SetParent(head.transform);
            ponyTip.transform.localPosition = new Vector3(0f, -0.54f, -0.33f);
            ponyTip.transform.localScale = new Vector3(0.15f, 0.22f, 0.13f);
            if (ponyTip.GetComponent<Collider>()) DestroyImmediate(ponyTip.GetComponent<Collider>());
            ponyTip.GetComponent<Renderer>().sharedMaterial = matGirlHair;

            // ==========================================
            // 4. ARMS & HANDS (Slender bare arms + Gold Bracelets)
            // ==========================================
            // Left Arm
            GameObject leftArm = new GameObject("LeftArm");
            leftArm.transform.SetParent(spine.transform);
            leftArm.transform.localPosition = new Vector3(-0.21f, 0.28f, 0f);
            rig.leftArm = leftArm.transform;

            GameObject armMeshL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armMeshL.name = "Arm_L";
            armMeshL.transform.SetParent(leftArm.transform);
            armMeshL.transform.localPosition = new Vector3(0f, -0.22f, 0f);
            armMeshL.transform.localScale = new Vector3(0.09f, 0.22f, 0.09f);
            if (armMeshL.GetComponent<Collider>()) DestroyImmediate(armMeshL.GetComponent<Collider>());
            armMeshL.GetComponent<Renderer>().sharedMaterial = matGirlSkin;

            GameObject braceletL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            braceletL.name = "GoldBracelet_L";
            braceletL.transform.SetParent(leftArm.transform);
            braceletL.transform.localPosition = new Vector3(0f, -0.38f, 0f);
            braceletL.transform.localScale = new Vector3(0.11f, 0.035f, 0.11f);
            if (braceletL.GetComponent<Collider>()) DestroyImmediate(braceletL.GetComponent<Collider>());
            braceletL.GetComponent<Renderer>().sharedMaterial = matGold;

            // Right Arm
            GameObject rightArm = new GameObject("RightArm");
            rightArm.transform.SetParent(spine.transform);
            rightArm.transform.localPosition = new Vector3(0.21f, 0.28f, 0f);
            rig.rightArm = rightArm.transform;

            GameObject armMeshR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armMeshR.name = "Arm_R";
            armMeshR.transform.SetParent(rightArm.transform);
            armMeshR.transform.localPosition = new Vector3(0f, -0.22f, 0f);
            armMeshR.transform.localScale = new Vector3(0.09f, 0.22f, 0.09f);
            if (armMeshR.GetComponent<Collider>()) DestroyImmediate(armMeshR.GetComponent<Collider>());
            armMeshR.GetComponent<Renderer>().sharedMaterial = matGirlSkin;

            GameObject braceletR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            braceletR.name = "GoldBracelet_R";
            braceletR.transform.SetParent(rightArm.transform);
            braceletR.transform.localPosition = new Vector3(0f, -0.38f, 0f);
            braceletR.transform.localScale = new Vector3(0.11f, 0.035f, 0.11f);
            if (braceletR.GetComponent<Collider>()) DestroyImmediate(braceletR.GetComponent<Collider>());
            braceletR.GetComponent<Renderer>().sharedMaterial = matGold;

            // ==========================================
            // 5. LEGS, SOCKS & EXPEDITION BOOTS
            // ==========================================
            // Left Leg (Slender Peach Leg)
            GameObject leftLeg = new GameObject("LeftLeg");
            leftLeg.transform.SetParent(hips.transform);
            leftLeg.transform.localPosition = new Vector3(-0.11f, -0.15f, 0f);
            rig.leftLeg = leftLeg.transform;

            GameObject legMeshL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legMeshL.name = "Leg_L";
            legMeshL.transform.SetParent(leftLeg.transform);
            legMeshL.transform.localPosition = new Vector3(0f, -0.24f, 0f);
            legMeshL.transform.localScale = new Vector3(0.12f, 0.24f, 0.12f);
            if (legMeshL.GetComponent<Collider>()) DestroyImmediate(legMeshL.GetComponent<Collider>());
            legMeshL.GetComponent<Renderer>().sharedMaterial = matGirlSkin;

            // White Crew Sock with Pink Trim
            GameObject sockL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sockL.name = "Sock_L";
            sockL.transform.SetParent(leftLeg.transform);
            sockL.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            sockL.transform.localScale = new Vector3(0.13f, 0.10f, 0.13f);
            if (sockL.GetComponent<Collider>()) DestroyImmediate(sockL.GetComponent<Collider>());
            sockL.GetComponent<Renderer>().sharedMaterial = matGirlSocks;

            GameObject sockTrimL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sockTrimL.name = "SockTrim_L";
            sockTrimL.transform.SetParent(leftLeg.transform);
            sockTrimL.transform.localPosition = new Vector3(0f, -0.40f, 0f);
            sockTrimL.transform.localScale = new Vector3(0.135f, 0.02f, 0.135f);
            if (sockTrimL.GetComponent<Collider>()) DestroyImmediate(sockTrimL.GetComponent<Collider>());
            sockTrimL.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            // Explorer Boot
            GameObject bootL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bootL.name = "Boot_L";
            bootL.transform.SetParent(leftLeg.transform);
            bootL.transform.localPosition = new Vector3(0f, -0.62f, 0.04f);
            bootL.transform.localScale = new Vector3(0.15f, 0.18f, 0.25f);
            if (bootL.GetComponent<Collider>()) DestroyImmediate(bootL.GetComponent<Collider>());
            bootL.GetComponent<Renderer>().sharedMaterial = matGirlBoots;

            // Right Leg (Slender Peach Leg)
            GameObject rightLeg = new GameObject("RightLeg");
            rightLeg.transform.SetParent(hips.transform);
            rightLeg.transform.localPosition = new Vector3(0.11f, -0.15f, 0f);
            rig.rightLeg = rightLeg.transform;

            GameObject legMeshR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            legMeshR.name = "Leg_R";
            legMeshR.transform.SetParent(rightLeg.transform);
            legMeshR.transform.localPosition = new Vector3(0f, -0.24f, 0f);
            legMeshR.transform.localScale = new Vector3(0.12f, 0.24f, 0.12f);
            if (legMeshR.GetComponent<Collider>()) DestroyImmediate(legMeshR.GetComponent<Collider>());
            legMeshR.GetComponent<Renderer>().sharedMaterial = matGirlSkin;

            // White Crew Sock with Pink Trim
            GameObject sockR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sockR.name = "Sock_R";
            sockR.transform.SetParent(rightLeg.transform);
            sockR.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            sockR.transform.localScale = new Vector3(0.13f, 0.10f, 0.13f);
            if (sockR.GetComponent<Collider>()) DestroyImmediate(sockR.GetComponent<Collider>());
            sockR.GetComponent<Renderer>().sharedMaterial = matGirlSocks;

            GameObject sockTrimR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sockTrimR.name = "SockTrim_R";
            sockTrimR.transform.SetParent(rightLeg.transform);
            sockTrimR.transform.localPosition = new Vector3(0f, -0.40f, 0f);
            sockTrimR.transform.localScale = new Vector3(0.135f, 0.02f, 0.135f);
            if (sockTrimR.GetComponent<Collider>()) DestroyImmediate(sockTrimR.GetComponent<Collider>());
            sockTrimR.GetComponent<Renderer>().sharedMaterial = matGirlRibbon;

            // Explorer Boot
            GameObject bootR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bootR.name = "Boot_R";
            bootR.transform.SetParent(rightLeg.transform);
            bootR.transform.localPosition = new Vector3(0f, -0.62f, 0.04f);
            bootR.transform.localScale = new Vector3(0.15f, 0.18f, 0.25f);
            if (bootR.GetComponent<Collider>()) DestroyImmediate(bootR.GetComponent<Collider>());
            bootR.GetComponent<Renderer>().sharedMaterial = matGirlBoots;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateTorchPrefab()
        {
            string path = PREFAB_DIR + "/Torch.prefab";
            GameObject root = new GameObject("Torch");

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.SetParent(root.transform);
            post.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            post.transform.localScale = new Vector3(0.12f, 0.75f, 0.12f);
            if (post.GetComponent<Collider>()) DestroyImmediate(post.GetComponent<Collider>());
            post.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "FlameMesh";
            flame.transform.SetParent(root.transform);
            flame.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            flame.transform.localScale = new Vector3(0.24f, 0.38f, 0.24f);
            if (flame.GetComponent<Collider>()) DestroyImmediate(flame.GetComponent<Collider>());
            flame.GetComponent<Renderer>().sharedMaterial = matTorchFlame;

            GameObject lightObj = new GameObject("TorchLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.65f, 0.2f);
            l.range = 10f;
            l.intensity = 4.0f;
            l.shadows = LightShadows.None;

            TorchFlicker flicker = root.AddComponent<TorchFlicker>();
            flicker.torchLight = l;
            flicker.flameTransform = flame.transform;

            AudioSource aSource = root.AddComponent<AudioSource>();
            AmbientAudioEmitter emitter = root.AddComponent<AmbientAudioEmitter>();
            SerializedObject so = new SerializedObject(emitter);
            so.FindProperty("soundClip").objectReferenceValue = LoadAudio("Environment/TorchCrackle.wav");
            so.FindProperty("volume").floatValue = 0.5f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateWallBrazierPrefab()
        {
            string path = PREFAB_DIR + "/WallBrazier.prefab";
            GameObject root = new GameObject("WallBrazier");

            GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "StoneBracket";
            bracket.transform.SetParent(root.transform);
            bracket.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            bracket.transform.localScale = new Vector3(0.35f, 0.35f, 0.7f);
            if (bracket.GetComponent<Collider>()) DestroyImmediate(bracket.GetComponent<Collider>());
            bracket.GetComponent<Renderer>().sharedMaterial = matL3Rock != null ? matL3Rock : matRock;

            GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bowl.name = "GoldBrazierBowl";
            bowl.transform.SetParent(root.transform);
            bowl.transform.localPosition = new Vector3(0f, 0.18f, 0.7f);
            bowl.transform.localScale = new Vector3(0.9f, 0.22f, 0.9f);
            if (bowl.GetComponent<Collider>()) DestroyImmediate(bowl.GetComponent<Collider>());
            bowl.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "BrazierFlame";
            flame.transform.SetParent(root.transform);
            flame.transform.localPosition = new Vector3(0f, 0.5f, 0.7f);
            flame.transform.localScale = new Vector3(0.5f, 0.65f, 0.5f);
            if (flame.GetComponent<Collider>()) DestroyImmediate(flame.GetComponent<Collider>());
            flame.GetComponent<Renderer>().sharedMaterial = matTorchFlame;

            GameObject lightObj = new GameObject("BrazierLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.7f, 0.7f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.82f, 0.25f);
            l.range = 12f;
            l.intensity = 4.5f;
            l.shadows = LightShadows.None;

            TorchFlicker flicker = root.AddComponent<TorchFlicker>();
            flicker.torchLight = l;
            flicker.flameTransform = flame.transform;

            root.AddComponent<AudioSource>();
            AmbientAudioEmitter emitter = root.AddComponent<AmbientAudioEmitter>();
            SerializedObject so = new SerializedObject(emitter);
            so.FindProperty("soundClip").objectReferenceValue = LoadAudio("Environment/TorchCrackle.wav");
            so.FindProperty("volume").floatValue = 0.45f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateCrystalSconcePrefab()
        {
            string path = PREFAB_DIR + "/CrystalSconce.prefab";
            GameObject root = new GameObject("CrystalSconce");

            GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlate.name = "MountPlate";
            basePlate.transform.SetParent(root.transform);
            basePlate.transform.localPosition = Vector3.zero;
            basePlate.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            basePlate.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            if (basePlate.GetComponent<Collider>()) DestroyImmediate(basePlate.GetComponent<Collider>());
            basePlate.GetComponent<Renderer>().sharedMaterial = matL2Rock != null ? matL2Rock : matRock;

            for (int i = 0; i < 3; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shard.name = "SconceShard_" + i;
                shard.transform.SetParent(root.transform);
                float tilt = -25f + i * 25f;
                shard.transform.localPosition = new Vector3((i - 1) * 0.16f, 0.35f, 0.25f);
                shard.transform.localRotation = Quaternion.Euler(tilt, (i - 1) * 20f, 0f);
                shard.transform.localScale = new Vector3(0.12f, 0.5f + i * 0.1f, 0.12f);
                if (shard.GetComponent<Collider>()) DestroyImmediate(shard.GetComponent<Collider>());
                shard.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? matTurquoise : matEmerald;
            }

            GameObject lightObj = new GameObject("SconceLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.4f, 0.4f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.1f, 0.95f, 1.0f);
            l.range = 10f;
            l.intensity = 3.8f;
            l.shadows = LightShadows.None;

            CrystalGlow glow = root.AddComponent<CrystalGlow>();
            glow.crystalLight = l;

            root.AddComponent<AudioSource>();
            AmbientAudioEmitter emitter = root.AddComponent<AmbientAudioEmitter>();
            SerializedObject so = new SerializedObject(emitter);
            so.FindProperty("soundClip").objectReferenceValue = LoadAudio("Environment/CrystalHum.wav");
            so.FindProperty("volume").floatValue = 0.4f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateCrystalPrefabs()
        {
            CreateSingleCrystalPrefab("CrystalCluster_Turquoise", matTurquoise, new Color(0.1f, 0.9f, 0.95f), "Environment/CrystalHum.wav");
            CreateSingleCrystalPrefab("CrystalCluster_Emerald", matEmerald, new Color(0.1f, 0.95f, 0.5f), "Environment/CrystalHum.wav");
        }

        private static void CreateSingleCrystalPrefab(string name, Material mat, Color lightColor, string soundPath)
        {
            string path = PREFAB_DIR + "/" + name + ".prefab";
            GameObject root = new GameObject(name);

            for (int i = 0; i < 4; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shard.name = "Shard_" + i;
                shard.transform.SetParent(root.transform);
                float angle = i * 90f + UnityEngine.Random.Range(-15f, 15f);
                float tilt = 12f + i * 4f;
                shard.transform.localPosition = Quaternion.Euler(0f, angle, 0f) * new Vector3(0.2f, 0.5f + i * 0.15f, 0f);
                shard.transform.localRotation = Quaternion.Euler(tilt, angle, 0f);
                shard.transform.localScale = new Vector3(0.15f, 0.6f + i * 0.2f, 0.15f);
                if (shard.GetComponent<Collider>()) DestroyImmediate(shard.GetComponent<Collider>());
                shard.GetComponent<Renderer>().sharedMaterial = mat;
            }

            GameObject lightObj = new GameObject("CrystalLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = lightColor;
            l.range = 8.0f;
            l.intensity = 2.8f;

            CrystalGlow glow = root.AddComponent<CrystalGlow>();
            glow.crystalLight = l;

            root.AddComponent<AudioSource>();
            AmbientAudioEmitter emitter = root.AddComponent<AmbientAudioEmitter>();
            SerializedObject so = new SerializedObject(emitter);
            so.FindProperty("soundClip").objectReferenceValue = LoadAudio(soundPath);
            so.FindProperty("volume").floatValue = 0.45f;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateCollapsingTilePrefab()
        {
            string path = PREFAB_DIR + "/CollapsingTile.prefab";
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "CollapsingTile";
            root.transform.localScale = new Vector3(2.2f, 0.35f, 2.0f);
            root.GetComponent<Renderer>().sharedMaterial = matBridge;

            CollapsingTile ct = root.AddComponent<CollapsingTile>();
            ct.crackClip = LoadAudio("Obstacles/BridgeCrack.wav");
            ct.collapseClip = LoadAudio("Obstacles/BridgeCollapse.wav");
            ct.collapseDelay = 0.85f;
            ct.respawnDelay = 5.0f;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateFallingRockPrefab()
        {
            string path = PREFAB_DIR + "/FallingRock.prefab";
            GameObject root = new GameObject("FallingRockObstacle");

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4.5f, 6.0f, 4.5f);
            col.center = new Vector3(0f, 3.0f, 0f);

            GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boulder.name = "RockBoulder";
            boulder.transform.SetParent(root.transform);
            boulder.transform.localPosition = new Vector3(0f, 7.5f, 0f);
            boulder.transform.localScale = new Vector3(1.7f, 1.5f, 1.7f);
            boulder.GetComponent<Renderer>().sharedMaterial = matRock;

            GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            warning.name = "WarningIndicator";
            warning.transform.SetParent(root.transform);
            warning.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            warning.transform.localScale = new Vector3(2.6f, 0.02f, 2.6f);
            if (warning.GetComponent<Collider>()) DestroyImmediate(warning.GetComponent<Collider>());
            warning.GetComponent<Renderer>().sharedMaterial = matWarning;

            FallingRock fr = root.AddComponent<FallingRock>();
            fr.rockMesh = boulder.transform;
            fr.groundWarningIndicator = warning;
            fr.fallingSound = LoadAudio("Obstacles/RockFalling.wav");
            fr.impactSound = LoadAudio("Obstacles/RockImpact.wav");
            fr.warningDelay = 1.0f;
            fr.fallGravity = 38f;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreatePressurePlatePrefab()
        {
            string path = PREFAB_DIR + "/PressurePlate.prefab";
            GameObject root = new GameObject("PressurePlateObstacle");

            GameObject baseTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseTile.name = "BaseFrame";
            baseTile.transform.SetParent(root.transform);
            baseTile.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            baseTile.transform.localScale = new Vector3(2.2f, 0.16f, 2.2f);
            baseTile.GetComponent<Renderer>().sharedMaterial = matPath;

            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "StepPlate";
            plate.transform.SetParent(root.transform);
            plate.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            plate.transform.localScale = new Vector3(1.9f, 0.08f, 1.9f);
            plate.GetComponent<Renderer>().sharedMaterial = matPressurePlate;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.4f, 0f);
            col.size = new Vector3(2.0f, 0.8f, 2.0f);

            GameObject spikesRack = new GameObject("SpikesRack");
            spikesRack.transform.SetParent(root.transform);
            spikesRack.transform.localPosition = new Vector3(0f, -0.6f, 0f);

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    spike.name = "Spike_" + x + "_" + z;
                    spike.transform.SetParent(spikesRack.transform);
                    spike.transform.localPosition = new Vector3(x * 0.6f, 0.5f, z * 0.6f);
                    spike.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
                    if (spike.GetComponent<Collider>()) DestroyImmediate(spike.GetComponent<Collider>());
                    spike.GetComponent<Renderer>().sharedMaterial = matSpikes;
                }
            }

            PressurePlate pp = root.AddComponent<PressurePlate>();
            pp.plateTransform = plate.transform;
            pp.trapSpikes = spikesRack.transform;
            pp.clickClip = LoadAudio("Obstacles/PressurePlateClick.wav");
            pp.spikesClip = LoadAudio("Obstacles/TrapSpikes.wav");

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateSwingingStonePrefab()
        {
            string path = PREFAB_DIR + "/SwingingStone.prefab";
            GameObject root = new GameObject("SwingingStoneObstacle");

            GameObject mount = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mount.name = "CeilingMount";
            mount.transform.SetParent(root.transform);
            mount.transform.localPosition = new Vector3(0f, 6.5f, 0f);
            mount.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
            if (mount.GetComponent<Collider>()) DestroyImmediate(mount.GetComponent<Collider>());
            mount.GetComponent<Renderer>().sharedMaterial = matRock;

            GameObject pivot = new GameObject("PendulumPivot");
            pivot.transform.SetParent(root.transform);
            pivot.transform.localPosition = new Vector3(0f, 6.5f, 0f);

            GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rope.name = "PendulumRope";
            rope.transform.SetParent(pivot.transform);
            rope.transform.localPosition = new Vector3(0f, -2.5f, 0f);
            rope.transform.localScale = new Vector3(0.08f, 2.5f, 0.08f);
            if (rope.GetComponent<Collider>()) DestroyImmediate(rope.GetComponent<Collider>());
            rope.GetComponent<Renderer>().sharedMaterial = matRock;

            GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boulder.name = "HeavyStoneBoulder";
            boulder.transform.SetParent(pivot.transform);
            boulder.transform.localPosition = new Vector3(0f, -5.2f, 0f);
            boulder.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
            boulder.GetComponent<Renderer>().sharedMaterial = matRock;

            SphereCollider sc = boulder.GetComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1.0f;

            SwingingStone ss = root.AddComponent<SwingingStone>();
            ss.pivot = pivot.transform;
            ss.stoneBoulder = boulder.transform;
            ss.whooshClip = LoadAudio("Obstacles/SwingingStoneWhoosh.wav");
            ss.hitClip = LoadAudio("Obstacles/PlayerHit.wav");
            ss.swingFrequency = 2.0f;
            ss.maxAngle = 55f;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateTreasureChestPrefab()
        {
            string path = PREFAB_DIR + "/TreasureChest.prefab";
            GameObject root = new GameObject("TreasureChest");

            GameObject chestBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chestBase.name = "ChestBase";
            chestBase.transform.SetParent(root.transform);
            chestBase.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            chestBase.transform.localScale = new Vector3(1.6f, 0.9f, 1.1f);
            chestBase.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "GoldBands";
            trim.transform.SetParent(chestBase.transform);
            trim.transform.localPosition = Vector3.zero;
            trim.transform.localScale = new Vector3(1.02f, 0.92f, 1.02f);
            if (trim.GetComponent<Collider>()) DestroyImmediate(trim.GetComponent<Collider>());
            trim.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject hinge = new GameObject("ChestLidHinge");
            hinge.transform.SetParent(root.transform);
            hinge.transform.localPosition = new Vector3(0f, 0.9f, 0.5f);

            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "LidMesh";
            lid.transform.SetParent(hinge.transform);
            lid.transform.localPosition = new Vector3(0f, 0.15f, -0.5f);
            lid.transform.localScale = new Vector3(1.65f, 0.35f, 1.15f);
            lid.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject goldMound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goldMound.name = "GoldPiles";
            goldMound.transform.SetParent(chestBase.transform);
            goldMound.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            goldMound.transform.localScale = new Vector3(0.9f, 0.5f, 0.8f);
            if (goldMound.GetComponent<Collider>()) DestroyImmediate(goldMound.GetComponent<Collider>());
            goldMound.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject lightObj = new GameObject("GoldGlowLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.85f, 0.3f);
            l.range = 8.0f;
            l.intensity = 0f;

            TreasureChest tc = root.AddComponent<TreasureChest>();
            tc.chestLid = hinge.transform;
            tc.chestInternalLight = l;
            tc.chestOpenClip = LoadAudio("UI/ChestOpen.wav");
            tc.victoryFanfareClip = LoadAudio("BackgroundMusic/VictoryFanfare.wav");
            tc.interactionDistance = 3.5f;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateAerialEnergyRingPrefab()
        {
            string path = PREFAB_DIR + "/AerialEnergyRing.prefab";
            GameObject root = new GameObject("AerialEnergyRing");

            SphereCollider sc = root.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 2.4f;

            AerialEnergyRing ring = root.AddComponent<AerialEnergyRing>();

            GameObject meshObj = new GameObject("RingMesh");
            meshObj.transform.SetParent(root.transform);
            meshObj.transform.localPosition = Vector3.zero;

            int segments = 12;
            float radius = 2.0f;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * (360f / segments) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;

                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "Segment_" + i;
                seg.transform.SetParent(meshObj.transform);
                seg.transform.localPosition = new Vector3(x, y, 0f);
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, i * (360f / segments));
                seg.transform.localScale = new Vector3(0.24f, 1.15f, 0.24f);
                if (seg.GetComponent<Collider>()) DestroyImmediate(seg.GetComponent<Collider>());
                seg.GetComponent<Renderer>().sharedMaterial = matTurquoise;
            }

            GameObject lightObj = new GameObject("RingGlowLight");
            lightObj.transform.SetParent(root.transform);
            lightObj.transform.localPosition = Vector3.zero;
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.1f, 0.95f, 1.0f);
            l.range = 8.0f;
            l.intensity = 3.5f;
            l.shadows = LightShadows.None;

            ring.ringMesh = meshObj.transform;
            ring.ringLight = l;
            ring.ringPassClip = LoadAudio("BackgroundMusic/VictoryFanfare.wav");

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateCrystalLaserBarrierPrefab()
        {
            string path = PREFAB_DIR + "/CrystalLaserBarrier.prefab";
            GameObject root = new GameObject("CrystalLaserBarrier");

            Rigidbody rootRb = root.AddComponent<Rigidbody>();
            rootRb.isKinematic = true;
            rootRb.useGravity = false;

            CrystalLaserBarrier barrier = root.AddComponent<CrystalLaserBarrier>();

            GameObject leftPylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPylon.name = "Pylon_Left";
            leftPylon.transform.SetParent(root.transform);
            leftPylon.transform.localPosition = new Vector3(-4.5f, 3.5f, 0f);
            leftPylon.transform.localScale = new Vector3(0.5f, 3.5f, 0.5f);
            leftPylon.GetComponent<Renderer>().sharedMaterial = matL2Rock;

            GameObject rightPylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPylon.name = "Pylon_Right";
            rightPylon.transform.SetParent(root.transform);
            rightPylon.transform.localPosition = new Vector3(4.5f, 3.5f, 0f);
            rightPylon.transform.localScale = new Vector3(0.5f, 3.5f, 0.5f);
            rightPylon.GetComponent<Renderer>().sharedMaterial = matL2Rock;

            GameObject beamRoot = new GameObject("LaserBeamContainer");
            beamRoot.transform.SetParent(root.transform);
            beamRoot.transform.localPosition = new Vector3(0f, 3.5f, 0f);

            GameObject beamCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamCyl.name = "LaserBeamVisual";
            beamCyl.transform.SetParent(beamRoot.transform);
            beamCyl.transform.localPosition = Vector3.zero;
            beamCyl.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            beamCyl.transform.localScale = new Vector3(0.16f, 4.4f, 0.16f);
            if (beamCyl.GetComponent<Collider>()) DestroyImmediate(beamCyl.GetComponent<Collider>());
            beamCyl.GetComponent<Renderer>().sharedMaterial = matWarning;

            GameObject lightObj = new GameObject("LaserLight");
            lightObj.transform.SetParent(beamRoot.transform);
            lightObj.transform.localPosition = Vector3.zero;
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = Color.red;
            l.range = 7.0f;
            l.intensity = 3.0f;
            l.shadows = LightShadows.None;

            BoxCollider bc = beamRoot.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(8.8f, 0.6f, 0.8f);

            barrier.beamTransform = beamRoot.transform;
            barrier.laserLight = l;
            barrier.sweepAxis = Vector3.up;
            barrier.sweepDistance = 2.0f;
            barrier.sweepSpeed = 2.2f;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateRollerCoasterCartPrefab()
        {
            string path = PREFAB_DIR + "/RollerCoasterCart.prefab";
            GameObject root = new GameObject("RollerCoasterCart");

            BoxCollider bc = root.AddComponent<BoxCollider>();
            bc.size = new Vector3(2.4f, 2.6f, 3.2f);
            bc.center = new Vector3(0f, 1.3f, 0f);
            bc.isTrigger = true;

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            RollerCoasterCart cart = root.AddComponent<RollerCoasterCart>();

            GameObject body = new GameObject("CartBody");
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "CartFloor";
            floor.transform.SetParent(body.transform);
            floor.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            floor.transform.localScale = new Vector3(1.8f, 0.25f, 2.6f);
            if (floor.GetComponent<Collider>()) DestroyImmediate(floor.GetComponent<Collider>());
            floor.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject fWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fWall.name = "FrontWall";
            fWall.transform.SetParent(body.transform);
            fWall.transform.localPosition = new Vector3(0f, 0.75f, 1.2f);
            fWall.transform.localScale = new Vector3(1.8f, 0.7f, 0.18f);
            if (fWall.GetComponent<Collider>()) DestroyImmediate(fWall.GetComponent<Collider>());
            fWall.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject bWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bWall.name = "BackWall";
            bWall.transform.SetParent(body.transform);
            bWall.transform.localPosition = new Vector3(0f, 0.75f, -1.2f);
            bWall.transform.localScale = new Vector3(1.8f, 0.7f, 0.18f);
            if (bWall.GetComponent<Collider>()) DestroyImmediate(bWall.GetComponent<Collider>());
            bWall.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject lWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lWall.name = "LeftWall";
            lWall.transform.SetParent(body.transform);
            lWall.transform.localPosition = new Vector3(-0.85f, 0.75f, 0f);
            lWall.transform.localScale = new Vector3(0.18f, 0.7f, 2.6f);
            if (lWall.GetComponent<Collider>()) DestroyImmediate(lWall.GetComponent<Collider>());
            lWall.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            GameObject rWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rWall.name = "RightWall";
            rWall.transform.SetParent(body.transform);
            rWall.transform.localPosition = new Vector3(0.85f, 0.75f, 0f);
            rWall.transform.localScale = new Vector3(0.18f, 0.7f, 2.6f);
            if (rWall.GetComponent<Collider>()) DestroyImmediate(rWall.GetComponent<Collider>());
            rWall.GetComponent<Renderer>().sharedMaterial = matTorchWood;

            // 4-Sided Golden Rim Border (OPEN COCKPIT - Never covers or hides the player!)
            GameObject rimF = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rimF.name = "Rim_Front";
            rimF.transform.SetParent(body.transform);
            rimF.transform.localPosition = new Vector3(0f, 1.12f, 1.25f);
            rimF.transform.localScale = new Vector3(1.88f, 0.08f, 0.12f);
            if (rimF.GetComponent<Collider>()) DestroyImmediate(rimF.GetComponent<Collider>());
            rimF.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject rimB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rimB.name = "Rim_Back";
            rimB.transform.SetParent(body.transform);
            rimB.transform.localPosition = new Vector3(0f, 1.12f, -1.25f);
            rimB.transform.localScale = new Vector3(1.88f, 0.08f, 0.12f);
            if (rimB.GetComponent<Collider>()) DestroyImmediate(rimB.GetComponent<Collider>());
            rimB.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject rimL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rimL.name = "Rim_Left";
            rimL.transform.SetParent(body.transform);
            rimL.transform.localPosition = new Vector3(-0.88f, 1.12f, 0f);
            rimL.transform.localScale = new Vector3(0.12f, 0.08f, 2.62f);
            if (rimL.GetComponent<Collider>()) DestroyImmediate(rimL.GetComponent<Collider>());
            rimL.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject rimR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rimR.name = "Rim_Right";
            rimR.transform.SetParent(body.transform);
            rimR.transform.localPosition = new Vector3(0.88f, 1.12f, 0f);
            rimR.transform.localScale = new Vector3(0.12f, 0.08f, 2.62f);
            if (rimR.GetComponent<Collider>()) DestroyImmediate(rimR.GetComponent<Collider>());
            rimR.GetComponent<Renderer>().sharedMaterial = matGold;

            // Front Safety Lap Bar for the player to hold onto
            GameObject grabBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grabBar.name = "SafetyGrabBar";
            grabBar.transform.SetParent(body.transform);
            grabBar.transform.localPosition = new Vector3(0f, 0.85f, 0.45f);
            grabBar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            grabBar.transform.localScale = new Vector3(0.08f, 0.75f, 0.08f);
            if (grabBar.GetComponent<Collider>()) DestroyImmediate(grabBar.GetComponent<Collider>());
            grabBar.GetComponent<Renderer>().sharedMaterial = matGold;

            float[] wx = new float[] { -0.92f, 0.92f };
            float[] wz = new float[] { -0.8f, 0.8f };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Wheel_" + i + "_" + j;
                    wheel.transform.SetParent(body.transform);
                    wheel.transform.localPosition = new Vector3(wx[i], 0.22f, wz[j]);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    wheel.transform.localScale = new Vector3(0.36f, 0.08f, 0.36f);
                    if (wheel.GetComponent<Collider>()) DestroyImmediate(wheel.GetComponent<Collider>());
                    wheel.GetComponent<Renderer>().sharedMaterial = matSpikes;
                }
            }

            GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lantern.name = "Headlight";
            lantern.transform.SetParent(body.transform);
            lantern.transform.localPosition = new Vector3(0f, 0.8f, 1.35f);
            lantern.transform.localScale = new Vector3(0.35f, 0.35f, 0.25f);
            if (lantern.GetComponent<Collider>()) DestroyImmediate(lantern.GetComponent<Collider>());
            lantern.GetComponent<Renderer>().sharedMaterial = matGold;

            GameObject lightObj = new GameObject("CoasterHeadlight");
            lightObj.transform.SetParent(body.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.8f, 1.5f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Spot;
            l.spotAngle = 65f;
            l.range = 25f;
            l.intensity = 4.0f;
            l.color = new Color(1.0f, 0.95f, 0.8f);
            l.shadows = LightShadows.None;

            GameObject seat = new GameObject("PassengerSeat");
            seat.transform.SetParent(body.transform);
            seat.transform.localPosition = new Vector3(0f, 0.25f, -0.05f);

            cart.passengerSeat = seat.transform;
            cart.cartBodyVisual = body.transform;
            cart.baseSpeed = 16.0f;
            cart.downhillBoost = 8.0f;
            cart.playerBoost = 6.0f;
            cart.jumpForce = 11.0f;
            cart.jumpGravity = -28.0f;
            cart.arrivalThreshold = 2.0f;
            cart.turnBankingSpeed = 12.0f;
            cart.leanAngle = 28.0f;
            cart.railRumbleClip = LoadAudio("Player/FootstepRun.wav");
            cart.jumpThrusterClip = LoadAudio("Player/Jump.wav");
            cart.screechBrakeClip = LoadAudio("Player/Land.wav");
            cart.hitClip = LoadAudio("Obstacles/PlayerHit.wav");

            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }

        private static void CreateCoasterObstaclePrefabs()
        {
            // A) BrokenTrackGap
            {
                string path = PREFAB_DIR + "/BrokenTrackGap.prefab";
                GameObject root = new GameObject("BrokenTrackGap");
                BoxCollider bc = root.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.size = new Vector3(5.5f, 4.0f, 8.0f);
                bc.center = new Vector3(0f, 1.0f, 0f);

                CoasterTrackObstacle cto = root.AddComponent<CoasterTrackObstacle>();
                cto.hazardType = CoasterHazardType.BrokenTrackGap;

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "WarningGapMarker";
                marker.transform.SetParent(root.transform);
                marker.transform.localPosition = new Vector3(0f, -0.4f, 0f);
                marker.transform.localScale = new Vector3(3.5f, 0.15f, 5.0f);
                if (marker.GetComponent<Collider>()) DestroyImmediate(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().sharedMaterial = matWarning;

                PrefabUtility.SaveAsPrefabAsset(root, path);
                DestroyImmediate(root);
            }

            // B) LowHangingArch
            {
                string path = PREFAB_DIR + "/LowHangingArch.prefab";
                GameObject root = new GameObject("LowHangingArch");
                BoxCollider bc = root.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.size = new Vector3(7.0f, 3.5f, 4.0f);
                bc.center = new Vector3(0f, 2.0f, 0f);

                CoasterTrackObstacle cto = root.AddComponent<CoasterTrackObstacle>();
                cto.hazardType = CoasterHazardType.LowHangingBeam;

                GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beam.name = "StoneBeam";
                beam.transform.SetParent(root.transform);
                beam.transform.localPosition = new Vector3(0f, 2.3f, 0f);
                beam.transform.localScale = new Vector3(6.5f, 0.8f, 1.2f);
                if (beam.GetComponent<Collider>()) DestroyImmediate(beam.GetComponent<Collider>());
                beam.GetComponent<Renderer>().sharedMaterial = matL3Rock;

                for (int i = -2; i <= 2; i++)
                {
                    GameObject st = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    st.name = "BeamSpike_" + i;
                    st.transform.SetParent(root.transform);
                    st.transform.localPosition = new Vector3(i * 1.1f, 1.8f, 0f);
                    st.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
                    if (st.GetComponent<Collider>()) DestroyImmediate(st.GetComponent<Collider>());
                    st.GetComponent<Renderer>().sharedMaterial = matSpikes;
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                DestroyImmediate(root);
            }

            // C) SideHazard_Left
            {
                string path = PREFAB_DIR + "/SideHazard_Left.prefab";
                GameObject root = new GameObject("SideHazard_Left");
                BoxCollider bc = root.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.size = new Vector3(3.5f, 4.0f, 4.5f);
                bc.center = new Vector3(-1.2f, 1.5f, 0f);

                CoasterTrackObstacle cto = root.AddComponent<CoasterTrackObstacle>();
                cto.hazardType = CoasterHazardType.LeftSideHazard;

                GameObject mace = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mace.name = "LeftMaceVisual";
                mace.transform.SetParent(root.transform);
                mace.transform.localPosition = new Vector3(-1.2f, 1.3f, 0f);
                mace.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                if (mace.GetComponent<Collider>()) DestroyImmediate(mace.GetComponent<Collider>());
                mace.GetComponent<Renderer>().sharedMaterial = matSpikes;

                GameObject chain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                chain.name = "MaceChain";
                chain.transform.SetParent(root.transform);
                chain.transform.localPosition = new Vector3(-1.2f, 3.5f, 0f);
                chain.transform.localScale = new Vector3(0.08f, 2.0f, 0.08f);
                if (chain.GetComponent<Collider>()) DestroyImmediate(chain.GetComponent<Collider>());
                chain.GetComponent<Renderer>().sharedMaterial = matSpikes;

                PrefabUtility.SaveAsPrefabAsset(root, path);
                DestroyImmediate(root);
            }

            // D) SideHazard_Right
            {
                string path = PREFAB_DIR + "/SideHazard_Right.prefab";
                GameObject root = new GameObject("SideHazard_Right");
                BoxCollider bc = root.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.size = new Vector3(3.5f, 4.0f, 4.5f);
                bc.center = new Vector3(1.2f, 1.5f, 0f);

                CoasterTrackObstacle cto = root.AddComponent<CoasterTrackObstacle>();
                cto.hazardType = CoasterHazardType.RightSideHazard;

                GameObject mace = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mace.name = "RightMaceVisual";
                mace.transform.SetParent(root.transform);
                mace.transform.localPosition = new Vector3(1.2f, 1.3f, 0f);
                mace.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                if (mace.GetComponent<Collider>()) DestroyImmediate(mace.GetComponent<Collider>());
                mace.GetComponent<Renderer>().sharedMaterial = matSpikes;

                GameObject chain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                chain.name = "MaceChain";
                chain.transform.SetParent(root.transform);
                chain.transform.localPosition = new Vector3(1.2f, 3.5f, 0f);
                chain.transform.localScale = new Vector3(0.08f, 2.0f, 0.08f);
                if (chain.GetComponent<Collider>()) DestroyImmediate(chain.GetComponent<Collider>());
                chain.GetComponent<Renderer>().sharedMaterial = matSpikes;

                PrefabUtility.SaveAsPrefabAsset(root, path);
                DestroyImmediate(root);
            }
        }

        private static void CreateCoasterTrack(List<Vector3> waypoints, Transform parent, Material railMat, Material sleeperMat)
        {
            GameObject trackRoot = new GameObject("CoasterTrackRails");
            if (parent != null) trackRoot.transform.SetParent(parent);

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Vector3 pA = waypoints[i];
                Vector3 pB = waypoints[i + 1];
                float segLen = Vector3.Distance(pA, pB);
                if (segLen < 0.1f) continue;

                Vector3 dir = (pB - pA).normalized;
                Vector3 mid = (pA + pB) * 0.5f;
                Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                Vector3 right = rot * Vector3.right;

                GameObject lRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lRail.name = "RailL_" + i;
                lRail.transform.SetParent(trackRoot.transform);
                lRail.transform.position = mid - right * 0.75f;
                lRail.transform.rotation = rot * Quaternion.Euler(90f, 0f, 0f);
                lRail.transform.localScale = new Vector3(0.12f, segLen * 0.5f, 0.12f);
                if (lRail.GetComponent<Collider>()) DestroyImmediate(lRail.GetComponent<Collider>());
                lRail.GetComponent<Renderer>().sharedMaterial = railMat;

                GameObject rRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rRail.name = "RailR_" + i;
                rRail.transform.SetParent(trackRoot.transform);
                rRail.transform.position = mid + right * 0.75f;
                rRail.transform.rotation = rot * Quaternion.Euler(90f, 0f, 0f);
                rRail.transform.localScale = new Vector3(0.12f, segLen * 0.5f, 0.12f);
                if (rRail.GetComponent<Collider>()) DestroyImmediate(rRail.GetComponent<Collider>());
                rRail.GetComponent<Renderer>().sharedMaterial = railMat;

                int ties = Mathf.Max(1, (int)(segLen / 2.0f));
                for (int t = 0; t < ties; t++)
                {
                    Vector3 tiePos = pA + dir * (t * 2.0f + 1.0f);
                    GameObject tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tie.name = "Tie_" + i + "_" + t;
                    tie.transform.SetParent(trackRoot.transform);
                    tie.transform.position = tiePos - Vector3.up * 0.08f;
                    tie.transform.rotation = rot;
                    tie.transform.localScale = new Vector3(1.85f, 0.15f, 0.4f);
                    if (tie.GetComponent<Collider>()) DestroyImmediate(tie.GetComponent<Collider>());
                    tie.GetComponent<Renderer>().sharedMaterial = sleeperMat;

                    if (tiePos.y > 1.2f)
                    {
                        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        pillar.name = "TrackPillar_" + i + "_" + t;
                        pillar.transform.SetParent(trackRoot.transform);
                        pillar.transform.position = new Vector3(tiePos.x, tiePos.y * 0.5f, tiePos.z);
                        pillar.transform.localScale = new Vector3(0.35f, tiePos.y * 0.5f, 0.35f);
                        if (pillar.GetComponent<Collider>()) DestroyImmediate(pillar.GetComponent<Collider>());
                        pillar.GetComponent<Renderer>().sharedMaterial = sleeperMat;
                    }
                }
            }
        }

        private static void BuildAllScenes()
        {
            if (!Directory.Exists(SCENE_DIR)) Directory.CreateDirectory(SCENE_DIR);

            BuildMainMenuScene();
            BuildLevelSelectScene();
            BuildInstructionsScene();
            BuildCaveLevel1Scene();
            BuildCaveLevel2Scene();
            BuildCaveLevel3Scene();
            BuildGameCompleteScene();

            // Reopen MainMenu so it is active and ready to play in the editor
            EditorSceneManager.OpenScene(SCENE_DIR + "/MainMenu.unity");
        }

        private static GameObject SetupSceneBase(string sceneName, Color? ambientColor = null, Color? fogColor = null, float fogDensity = 0.024f)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor ?? new Color(0.12f, 0.14f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor ?? new Color(0.04f, 0.07f, 0.10f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;

            GameObject root = new GameObject("--- SCENE ROOT ---");

            GameObject audioMgr = new GameObject("AudioManager");
            AudioManager am = audioMgr.AddComponent<AudioManager>();
            am.defaultCaveAmbience = LoadAudio("BackgroundMusic/CaveAmbience.wav");
            am.defaultBackgroundMusic = LoadAudio("BackgroundMusic/MysteriousMusic.wav");
            am.buttonClickSound = LoadAudio("UI/ButtonClick.wav");
            am.checkpointSound = LoadAudio("UI/Checkpoint.wav");
            am.playerHitSound = LoadAudio("Obstacles/PlayerHit.wav");

            return root;
        }

        private static void BuildMainMenuScene()
        {
            SetupSceneBase("MainMenu");

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
            camObj.transform.position = new Vector3(0f, 2.5f, -8f);
            camObj.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
            camObj.AddComponent<AudioListener>();

            CreateCorridor(new Vector3(0f, 0f, -12f), new Vector3(0f, 0f, 15f), 10f, 7f, true);
            SpawnPrefab("Torch", new Vector3(-3.8f, 2.5f, -3f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("Torch", new Vector3(3.8f, 2.5f, -3f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-3.2f, 0.2f, -1f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Emerald", new Vector3(3.2f, 0.2f, -1f), Quaternion.identity);

            GameObject canvasObj = CreateCanvas("MainMenuCanvas");
            MainMenuController mmc = canvasObj.AddComponent<MainMenuController>();

            CreateText(canvasObj.transform, "TitleText", "TREASURE HUNT\nLOST CAVE", 52, new Vector2(0f, 160f), new Vector2(850f, 170f), new Color(1.0f, 0.82f, 0.3f), FontStyle.Bold);

            mmc.startGameButton = CreateButton(canvasObj.transform, "StartButton", "START GAME", new Vector2(0f, 20f), new Vector2(280f, 54f), new Color(0.9f, 0.55f, 0.15f));
            mmc.levelSelectButton = CreateButton(canvasObj.transform, "LevelsButton", "LEVELS", new Vector2(0f, -44f), new Vector2(280f, 54f), new Color(0.18f, 0.45f, 0.5f));
            mmc.controlsButton = CreateButton(canvasObj.transform, "ControlsButton", "CONTROLS", new Vector2(0f, -108f), new Vector2(280f, 54f), new Color(0.18f, 0.45f, 0.5f));
            mmc.soundSettingsButton = CreateButton(canvasObj.transform, "SoundButton", "SOUND SETTINGS", new Vector2(0f, -172f), new Vector2(280f, 54f), new Color(0.18f, 0.45f, 0.5f));
            mmc.quitGameButton = CreateButton(canvasObj.transform, "QuitButton", "QUIT", new Vector2(0f, -236f), new Vector2(280f, 54f), new Color(0.6f, 0.2f, 0.2f));

            GameObject soundPanel = CreatePanel(canvasObj.transform, "SoundSettingsPanel", new Vector2(520f, 340f), new Color(0.1f, 0.12f, 0.16f, 0.96f));
            mmc.soundSettingsPanel = soundPanel;
            CreateText(soundPanel.transform, "SoundTitle", "SOUND SETTINGS", 30, new Vector2(0f, 120f), new Vector2(400f, 40f), Color.white, FontStyle.Bold);
            
            CreateText(soundPanel.transform, "MusicLabel", "MUSIC VOLUME", 22, new Vector2(-120f, 45f), new Vector2(200f, 32f), Color.white, FontStyle.Bold);
            mmc.musicSlider = CreateSlider(soundPanel.transform, "MusicSlider", new Vector2(80f, 45f), new Vector2(220f, 26f));

            CreateText(soundPanel.transform, "SFXLabel", "SFX VOLUME", 22, new Vector2(-120f, -15f), new Vector2(200f, 32f), Color.white, FontStyle.Bold);
            mmc.sfxSlider = CreateSlider(soundPanel.transform, "SFXSlider", new Vector2(80f, -15f), new Vector2(220f, 26f));

            mmc.closeSoundSettingsButton = CreateButton(soundPanel.transform, "CloseSoundBtn", "CLOSE", new Vector2(0f, -100f), new Vector2(200f, 46f), new Color(0.2f, 0.5f, 0.45f));
            soundPanel.SetActive(false);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/MainMenu.unity");
        }

        private static void BuildLevelSelectScene()
        {
            SetupSceneBase("LevelSelection");

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
            camObj.transform.position = new Vector3(0f, 2.5f, -8f);
            camObj.AddComponent<AudioListener>();

            CreateCorridor(new Vector3(0f, 0f, -12f), new Vector3(0f, 0f, 15f), 10f, 7f, true);
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-3.5f, 0.2f, -2f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Emerald", new Vector3(3.5f, 0.2f, -2f), Quaternion.identity);

            GameObject canvasObj = CreateCanvas("LevelSelectCanvas");
            LevelSelectController lsc = canvasObj.AddComponent<LevelSelectController>();

            CreateText(canvasObj.transform, "Title", "SELECT CAVE STAGE", 46, new Vector2(0f, 190f), new Vector2(750f, 80f), new Color(1.0f, 0.85f, 0.3f), FontStyle.Bold);

            lsc.level1Button = CreateButton(canvasObj.transform, "Lvl1Btn", "STAGE 1: CAVE ENTRANCE [EASY]\n(On Foot: Winding Path, 4 Turns & Abyss Bridge)", new Vector2(0f, 80f), new Vector2(540f, 70f), new Color(0.2f, 0.5f, 0.55f));
            lsc.level2Button = CreateButton(canvasObj.transform, "Lvl2Btn", "STAGE 2: CRYSTAL TUNNELS [MAX DIFFICULTY]\n(Aerial Flight: Crystal Wings, Rapid Laser Grids & Spires)", new Vector2(0f, -5f), new Vector2(540f, 70f), new Color(0.18f, 0.48f, 0.42f));
            lsc.level3Button = CreateButton(canvasObj.transform, "Lvl3Btn", "STAGE 3: TREASURE CHAMBER [MAX DIFFICULTY]\n(Roller Coaster: 20 Rapid Obstacles, Jump/Duck Combos & Plunge)", new Vector2(0f, -90f), new Vector2(540f, 70f), new Color(0.70f, 0.35f, 0.12f));

            lsc.backButton = CreateButton(canvasObj.transform, "BackBtn", "BACK TO MAIN MENU", new Vector2(0f, -200f), new Vector2(260f, 50f), new Color(0.5f, 0.2f, 0.2f));

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/LevelSelection.unity");
        }

        private static void BuildInstructionsScene()
        {
            SetupSceneBase("Instructions");

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.08f);
            camObj.transform.position = new Vector3(0f, 2.5f, -8f);
            camObj.AddComponent<AudioListener>();

            CreateCorridor(new Vector3(0f, 0f, -12f), new Vector3(0f, 0f, 15f), 10f, 7f, true);

            GameObject canvasObj = CreateCanvas("InstructionsCanvas");
            InstructionsController ic = canvasObj.AddComponent<InstructionsController>();

            CreateText(canvasObj.transform, "Title", "GAMEPLAY & CONTROLS", 44, new Vector2(0f, 260f), new Vector2(800f, 60f), new Color(1.0f, 0.85f, 0.3f), FontStyle.Bold);

            GameObject panel = CreatePanel(canvasObj.transform, "InstructionsPanel", new Vector2(1100f, 440f), new Color(0.06f, 0.09f, 0.13f, 0.96f));

            string[] keys = new string[] {
                "[ ON FOOT (EASY) ]",
                "[ FLIGHT (MEDIUM) ]",
                "[ COASTER (HARD) ]",
                "[ GENERAL CONTROLS ]"
            };

            string[] descs = new string[] {
                "[W,A,S,D] Move / Turn Corners  |  [SPACE] Jump Gaps  |  [SHIFT] Sprint",
                "[W] Cruise Forward  |  [S] Brake / Slow Down  |  [A,D] Steer & Bank  |  [SPACE] Ascend",
                "[SPACE] Rocket-Jump Gaps  |  [S] Duck & Brake  |  [A,D] Lean / Dodge Maces  |  [SHIFT] Boost",
                "[MOUSE] 360° Camera  |  [E] Open Golden Chest  |  [ESC] Pause Menu"
            };

            for (int i = 0; i < keys.Length; i++)
            {
                float y = 135f - i * 85f;

                GameObject keyPill = CreatePanel(panel.transform, "Pill_" + i, new Vector2(280f, 52f), new Color(0.12f, 0.22f, 0.32f, 0.95f));
                RectTransform pillRT = keyPill.GetComponent<RectTransform>();
                pillRT.anchoredPosition = new Vector2(-360f, y);

                CreateText(keyPill.transform, "KeyText_" + i, keys[i], 20, Vector2.zero, new Vector2(280f, 52f), new Color(0.2f, 1.0f, 1.0f), FontStyle.Bold);

                Text descText = CreateText(panel.transform, "Desc_" + i, descs[i], 20, new Vector2(170f, y), new Vector2(700f, 60f), Color.white, FontStyle.Bold);
                descText.alignment = TextAnchor.MiddleLeft;
            }

            ic.continueButton = CreateButton(canvasObj.transform, "ContinueBtn", "ENTER THE CAVE", new Vector2(170f, -270f), new Vector2(280f, 56f), new Color(0.92f, 0.55f, 0.15f));
            ic.backButton = CreateButton(canvasObj.transform, "BackBtn", "BACK", new Vector2(-170f, -270f), new Vector2(200f, 56f), new Color(0.45f, 0.2f, 0.2f));

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/Instructions.unity");
        }

        public enum TurnType
        {
            NorthToEast, // Moving +Z, turning Right (+X)
            NorthToWest, // Moving +Z, turning Left (-X)
            EastToNorth, // Moving +X, turning Left (+Z)
            EastToSouth, // Moving +X, turning Right (-Z)
            WestToNorth, // Moving -X, turning Right (+Z)
            WestToSouth, // Moving -X, turning Left (-Z)
            SouthToEast, // Moving -Z, turning Left (+X)
            SouthToWest  // Moving -Z, turning Right (-X)
        }

        // =========================================================================
        // STAGE 1: CAVE ENTRANCE - 190m WINDING LABYRINTH WITH 4 90° TURNS & ALL OBSTACLES
        // =========================================================================
        private static void BuildCaveLevel1Scene()
        {
            SetupSceneBase("CaveLevel1", new Color(0.20f, 0.15f, 0.10f), new Color(0.07f, 0.05f, 0.03f), 0.024f);

            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.currentLevelName = "Stage 1: Cave Entrance [EASY]";
            gm.currentObjective = "Follow the torchlit winding path, turn the corners, and conquer all 4 obstacles!";

            GameObject player = SpawnPrefab("Player", new Vector3(0f, 0.1f, 2f), Quaternion.identity);
            CreateThirdPersonCamera(player.transform);
            CreateGameplayHUDCanvas();

            // Segment 1 (Heading +Z from Z=0 to Z=26 at X=0)
            CreateZCorridor(0f, 0f, 26f, 8f, 6.5f, true, matL1Rock, matL1Path, "Torch", false);
            CreateWaterPool(new Vector3(-2.2f, 0.05f, 16f), new Vector3(2.5f, 0.1f, 4f));
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-3.2f, 0.1f, 10f), Quaternion.identity);

            // CORNER 1: 90° Turn to the Right (+X) at (0, 0, 30)
            CreateCorner(new Vector3(0f, 0f, 30f), TurnType.NorthToEast, 8f, 6.5f, matL1Rock, matL1Path, "Torch", false);

            // Segment 2 (Heading +X along Z=30, from X=4 to X=36) - Length 32m
            CreateXCorridor(30f, 4f, 14f, 8f, 6.5f, true, matL1Rock, matL1Path, "Torch", false);

            // OBSTACLE 1: Collapsing Stone Bridge across 12-meter Deep Abyss Gap
            CreateXCorridor(30f, 14f, 26f, 8f, 6.5f, false, matL1Rock, matL1Path, "Torch", false);
            CreateAbyssGap(new Vector3(20f, -4.0f, 30f), new Vector3(14f, 2f, 10f));
            for (int i = 0; i < 5; i++)
            {
                Vector3 tilePos = new Vector3(15.2f + i * 2.4f, 0.1f, 30f);
                SpawnPrefab("CollapsingTile", tilePos, Quaternion.Euler(0f, 90f, 0f));
            }

            // Safe landing & Checkpoint 1
            CreateXCorridor(30f, 26f, 36f, 8f, 6.5f, true, matL1Rock, matL1Path, "Torch", false);
            CreateCheckpoint(new Vector3(30f, 0.2f, 30f));

            // CORNER 2: 90° Turn to the Left (+Z) at (40, 0, 30)
            CreateCorner(new Vector3(40f, 0f, 30f), TurnType.EastToNorth, 8f, 6.5f, matL1Rock, matL1Path, "Torch", false);

            // Segment 3 (Heading +Z along X=40, from Z=34 to Z=66) - Length 32m
            CreateZCorridor(40f, 34f, 66f, 8f, 6.5f, true, matL1Rock, matL1Path, "Torch", false);

            // OBSTACLE 2: Falling Ceiling Rock Trap
            SpawnPrefab("FallingRock", new Vector3(40f, 0.1f, 50f), Quaternion.identity);

            // CORNER 3: 90° Turn to the Left (-X) at (40, 0, 70)
            CreateCorner(new Vector3(40f, 0f, 70f), TurnType.NorthToWest, 8f, 6.5f, matL1Rock, matL1Path, "Torch", false);

            // Segment 4 (Heading -X along Z=70, from X=36 down to X=4) - Length 32m
            CreateXCorridor(70f, 36f, 4f, 8f, 6.5f, true, matL1Rock, matL1Path, "Torch", false);

            // OBSTACLE 3: Hidden Pressure Plate & Erupting Iron Spike Trap
            SpawnPrefab("PressurePlate", new Vector3(20f, 0.05f, 70f), Quaternion.identity);

            // Safe stone Checkpoint 2
            CreateCheckpoint(new Vector3(10f, 0.2f, 70f));

            // CORNER 4: 90° Turn to the Right (+Z) at (0, 0, 70)
            CreateCorner(new Vector3(0f, 0f, 70f), TurnType.WestToNorth, 8f, 6.5f, matL1Rock, matL1Path, "Torch", false);

            // Segment 5 (Heading +Z along X=0, from Z=74 to Z=110) - Length 36m
            CreateZCorridor(0f, 74f, 110f, 8f, 6.5f, true, matL1Rock, matL1Path, "Torch", false);

            // OBSTACLE 4: Heavy Swinging Stone Boulder Pendulum
            SpawnPrefab("SwingingStone", new Vector3(0f, 0f, 92f), Quaternion.identity);

            // Level Exit Portal leading to Stage 2 at Z=106
            CreateLevelExit(new Vector3(0f, 0.2f, 106f), "CaveLevel2", "Proceeding to Stage 2: Crystal Tunnels...", matL1Rock);

            // Cave Back Wall closing Segment 5
            CreateEndWall(new Vector3(0f, 3.25f, 110.6f), new Vector3(8.5f, 6.5f, 1.2f), matL1Rock);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/CaveLevel1.unity");
        }

        // =========================================================================
        // STAGE 2: CRYSTAL TUNNELS - TRAVERSAL MODE: "FLYING" [MAX DIFFICULTY]
        // =========================================================================
        private static void BuildCaveLevel2Scene()
        {
            SetupSceneBase("CaveLevel2", new Color(0.04f, 0.16f, 0.22f), new Color(0.02f, 0.08f, 0.12f), 0.016f);

            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.currentLevelName = "Stage 2: Crystal Tunnels [MAX DIFFICULTY - AERIAL FLIGHT]";
            gm.currentObjective = "MAX DIFFICULTY! Weave through rapid laser grids, dodge mid-air crystal spires, and hit the Grand Portal!";

            GameObject player = SpawnPrefab("Player", new Vector3(0f, 4.0f, 2f), Quaternion.identity);
            PlayerFlightController flight = player.AddComponent<PlayerFlightController>();
            flight.normalFlightSpeed = 7.5f;
            flight.boostSpeed = 12.0f;
            flight.brakeSpeed = 4.0f;
            flight.idleSpeed = 4.5f;
            flight.yawSpeed = 75.0f;
            flight.liftSpeed = 6.0f;

            CreateThirdPersonCamera(player.transform);
            CreateGameplayHUDCanvas();

            // Segment 1 (Heading +Z along X=0, from Z=0 to Z=40) - Height 10m, Width 12m for Flight
            CreateZCorridor(0f, 0f, 40f, 12f, 10f, true, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("AerialEnergyRing", new Vector3(0f, 4.5f, 8f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(0f, 0f, 16f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-2.5f, 4.0f, 22f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(0f, 0f, 28f), Quaternion.identity);
            SpawnPrefab("AerialEnergyRing", new Vector3(2.5f, 5.8f, 35f), Quaternion.identity);

            // CORNER 1: 90° Turn to the Left (-X) at (0, 0, 48)
            CreateCorner(new Vector3(0f, 0f, 48f), TurnType.NorthToWest, 12f, 10f, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalCluster_Emerald", new Vector3(0f, 4.5f, 48f), Quaternion.identity);

            // Segment 2 (Heading -X along Z=48, from X=-6 to X=-54) - Length 48m
            CreateXCorridor(48f, -6f, -54f, 12f, 10f, true, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-14f, 0f, 48f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("AerialEnergyRing", new Vector3(-22f, 5.0f, 48f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-30f, 0f, 48f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-36f, 4.5f, 48f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-42f, 0f, 48f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("AerialEnergyRing", new Vector3(-50f, 5.5f, 48f), Quaternion.Euler(0f, 90f, 0f));

            // CORNER 2: 90° Turn to the Right (+Z) at (-62, 0, 48)
            CreateCorner(new Vector3(-62f, 0f, 48f), TurnType.WestToNorth, 12f, 10f, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalCluster_Emerald", new Vector3(-62f, 4.5f, 48f), Quaternion.identity);

            // Segment 3 (Heading +Z along X=-62, from Z=56 to Z=104) - Length 48m
            CreateZCorridor(-62f, 56f, 104f, 12f, 10f, true, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-62f, 0f, 62f), Quaternion.identity);
            SpawnPrefab("AerialEnergyRing", new Vector3(-62f, 4.5f, 70f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-62f, 0f, 78f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-62f, 4.8f, 86f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-62f, 0f, 94f), Quaternion.identity);
            SpawnPrefab("AerialEnergyRing", new Vector3(-62f, 6.0f, 100f), Quaternion.identity);

            // CORNER 3: 90° Turn to the Right (+X) at (-62, 0, 112)
            CreateCorner(new Vector3(-62f, 0f, 112f), TurnType.NorthToEast, 12f, 10f, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalCluster_Turquoise", new Vector3(-62f, 4.5f, 112f), Quaternion.identity);

            // Segment 4 (Heading +X along Z=112, from X=-54 to X=36) - Length 90m Intense Gauntlet!
            CreateXCorridor(112f, -54f, 36f, 12f, 10f, true, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-46f, 0f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("AerialEnergyRing", new Vector3(-38f, 5.0f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-28f, 0f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("CrystalCluster_Emerald", new Vector3(-20f, 4.5f, 112f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(-10f, 0f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("AerialEnergyRing", new Vector3(0f, 5.5f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("CrystalLaserBarrier", new Vector3(10f, 0f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("CrystalLaserBarrier", new Vector3(22f, 0f, 112f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("AerialEnergyRing", new Vector3(30f, 6.0f, 112f), Quaternion.Euler(0f, 90f, 0f));

            // CORNER 4: 90° Turn to the Left (+Z) at (44, 0, 112)
            CreateCorner(new Vector3(44f, 0f, 112f), TurnType.EastToNorth, 12f, 10f, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalCluster_Emerald", new Vector3(44f, 4.5f, 112f), Quaternion.identity);

            // Final Runway & Landing Approach (Heading +Z along X=44, from Z=120 to Z=160) - Length 40m
            CreateZCorridor(44f, 120f, 160f, 12f, 10f, true, matL2Rock, matL2Path, "CrystalSconce", false);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(44f, 0f, 126f), Quaternion.identity);
            SpawnPrefab("CrystalLaserBarrier", new Vector3(44f, 0f, 136f), Quaternion.identity);
            CreateCheckpoint(new Vector3(44f, 0.2f, 142f));

            CreateAerialLevelExit(new Vector3(44f, 0f, 156f), "CaveLevel3", "Stage Complete! Soaring into the Roller Coaster Chamber...", 12f, 10f);
            CreateEndWall(new Vector3(44f, 5.0f, 161.0f), new Vector3(12.5f, 10f, 1.2f), matL2Rock);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/CaveLevel2.unity");
        }

        // =========================================================================
        // STAGE 3: TREASURE CHAMBER - TRAVERSAL MODE: "ROLLER COASTER" [MAX DIFFICULTY]
        // =========================================================================
        private static void BuildCaveLevel3Scene()
        {
            SetupSceneBase("CaveLevel3", new Color(0.22f, 0.17f, 0.08f), new Color(0.10f, 0.06f, 0.02f), 0.016f);

            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.currentLevelName = "Stage 3: Treasure Chamber [MAX DIFFICULTY - ROLLER COASTER]";
            gm.currentObjective = "EXTREME DANGER! Chain rocket jumps [SPACE], duck crushing arches [S], and dodge swinging maces [A/D]!";

            // Spawn Player and Cart
            GameObject player = SpawnPrefab("Player", new Vector3(0f, 0.8f, 5f), Quaternion.identity);
            GameObject cartObj = SpawnPrefab("RollerCoasterCart", new Vector3(0f, 0.8f, 5f), Quaternion.identity);
            RollerCoasterCart cart = cartObj.GetComponent<RollerCoasterCart>();
            cart.baseSpeed = 16.0f;
            cart.downhillBoost = 8.0f;
            cart.playerBoost = 6.0f;
            cart.jumpForce = 11.0f;
            cart.jumpGravity = -28.0f;
            cart.arrivalThreshold = 2.0f;
            cart.turnBankingSpeed = 12.0f;
            cart.leanAngle = 28.0f;

            CreateThirdPersonCamera(cartObj.transform);
            CreateGameplayHUDCanvas();

            // Mathematically Aligned Track Waypoints (Strictly follow 10m-wide corridor centerlines and smooth corner arcs)
            List<Vector3> waypoints = new List<Vector3>()
            {
                // Segment 1 (Heading +Z along X=0, climbing slightly from Z=5 to Z=45)
                new Vector3(0f, 0.8f, 5f),
                new Vector3(0f, 1.0f, 15f),
                new Vector3(0f, 1.4f, 26f), // Obstacle 1: Low Arch
                new Vector3(0f, 1.8f, 38f), // Obstacle 2: Track Gap
                new Vector3(0f, 2.0f, 45f),

                // Corner 1: Turn North to East (+Z to +X) inside 10x10 corner at (0, 0, 50)
                new Vector3(0.5f, 2.1f, 47.0f),
                new Vector3(1.46f, 2.2f, 48.54f),
                new Vector3(3.0f, 2.3f, 49.5f),
                new Vector3(5.0f, 2.4f, 50.0f),

                // Segment 2 (Heading +X along Z=50, from X=5 to X=65)
                new Vector3(18f, 2.5f, 50f), // Obstacle 3: Track Gap
                new Vector3(28f, 2.6f, 50f), // Obstacle 4: Left Mace
                new Vector3(40f, 2.7f, 50f), // Obstacle 5: Low Arch
                new Vector3(52f, 2.9f, 50f), // Obstacle 6: Right Mace
                new Vector3(60f, 3.1f, 50f),
                new Vector3(65f, 3.2f, 50f),

                // Corner 2: Turn East to North (+X to +Z) inside 10x10 corner at (70, 0, 50)
                new Vector3(67.0f, 3.3f, 50.5f),
                new Vector3(68.54f, 3.4f, 51.46f),
                new Vector3(69.5f, 3.5f, 53.0f),
                new Vector3(70.0f, 3.6f, 55.0f),

                // Segment 3 (Heading +Z along X=70, from Z=55 to Z=115)
                new Vector3(70f, 3.8f, 68f),  // Obstacle 7: Left Mace
                new Vector3(70f, 4.0f, 80f),  // Obstacle 8: Track Gap
                new Vector3(70f, 4.2f, 92f),  // Obstacle 9: Low Arch
                new Vector3(70f, 4.3f, 104f), // Obstacle 10: Right Mace
                new Vector3(70f, 4.5f, 115f),

                // Corner 3: Turn North to West (+Z to -X) inside 10x10 corner at (70, 0, 120)
                new Vector3(69.5f, 4.6f, 117.0f),
                new Vector3(68.54f, 4.7f, 118.54f),
                new Vector3(67.0f, 4.8f, 119.5f),
                new Vector3(65.0f, 4.8f, 120.0f),

                // Segment 4 (Heading -X along Z=120, from X=65 down to X=-45) - 110m High-Speed Gauntlet
                new Vector3(48f, 5.0f, 120f), // Obstacle 11: Track Gap Jump
                new Vector3(36f, 5.2f, 120f), // Obstacle 12: Low Arch (JUMP + DUCK COMBO!)
                new Vector3(24f, 5.4f, 120f), // Obstacle 13: Left Mace
                new Vector3(10f, 5.5f, 120f), // Obstacle 14: Right Mace
                new Vector3(-6f, 5.5f, 120f), // Obstacle 15: Track Gap Jump
                new Vector3(-18f, 5.5f, 120f), // Obstacle 16: Low Arch
                new Vector3(-32f, 5.5f, 120f), // Obstacle 17: Left Mace
                new Vector3(-45f, 5.5f, 120f),

                // Corner 4: Turn West to North (-X to +Z) inside 10x10 corner at (-50, 0, 120)
                new Vector3(-47.0f, 5.4f, 120.5f),
                new Vector3(-48.54f, 5.3f, 121.46f),
                new Vector3(-49.5f, 5.2f, 123.0f),
                new Vector3(-50.0f, 5.1f, 125.0f),

                // Segment 5: Summit Mountain Climb (Heading +Z along X=-50, from Z=125 to Z=155)
                new Vector3(-50f, 5.0f, 135f), // Obstacle 18: Right Mace
                new Vector3(-50f, 4.7f, 146f), // Obstacle 19: Low Arch
                new Vector3(-50f, 4.6f, 155f),

                // Corner 5: Turn North to East (+Z to +X) inside 10x10 corner at (-50, 0, 160)
                new Vector3(-49.5f, 4.5f, 157.0f),
                new Vector3(-48.54f, 4.4f, 158.54f),
                new Vector3(-47.0f, 4.3f, 159.5f),
                new Vector3(-45.0f, 4.2f, 160.0f),

                // Segment 6 & Grand Plunge into Treasure Chamber (Heading +X along Z=160 towards X=0)
                new Vector3(-35f, 3.6f, 160f),
                new Vector3(-28f, 2.8f, 160f), // Obstacle 20: Thruster Jump over Plunge Abyss!
                new Vector3(-20f, 2.0f, 160f),
                new Vector3(-15f, 1.4f, 160f), // Grand Archway entrance
                new Vector3(-8f, 0.8f, 160f),
                new Vector3(-3f, 0.4f, 160f)   // Terminus halting right at the Golden Treasure Chest
            };

            cart.trackWaypoints = waypoints;

            // Generate twin golden rails, wooden cross-ties, and trestle support pillars
            CreateCoasterTrack(waypoints, null, matGold, matTorchWood);

            // Watertight Temple Corridors (Width 10m, Height 8m throughout)
            // Segment 1 (Heading +Z along X=0, from Z=-12 to Z=45) - 17m extra clearance behind cart spawn
            CreateZCorridor(0f, -12f, 45f, 10f, 8f, true, matL3Rock, matL3Path, "WallBrazier", true);
            CreateEndWall(new Vector3(0f, 4.0f, -12.5f), new Vector3(10.5f, 8.5f, 1.2f), matL3Rock);
            SpawnPrefab("LowHangingArch", new Vector3(0f, 1.4f, 26f), Quaternion.identity);
            SpawnPrefab("BrokenTrackGap", new Vector3(0f, 1.8f, 38f), Quaternion.identity);

            // CORNER 1: Turn to the Right (+X) at (0, 0, 50)
            CreateCorner(new Vector3(0f, 0f, 50f), TurnType.NorthToEast, 10f, 8f, matL3Rock, matL3Path, "WallBrazier", true);

            // Segment 2 (Heading +X along Z=50, from X=5 to X=65) - Length 60m
            CreateXCorridor(50f, 5f, 65f, 10f, 8f, true, matL3Rock, matL3Path, "WallBrazier", true);
            SpawnPrefab("BrokenTrackGap", new Vector3(18f, 2.5f, 50f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("SideHazard_Left", new Vector3(28f, 2.6f, 50f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("LowHangingArch", new Vector3(40f, 2.7f, 50f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("SideHazard_Right", new Vector3(52f, 2.9f, 50f), Quaternion.Euler(0f, 90f, 0f));

            // CORNER 2: Turn to the Left (+Z) at (70, 0, 50)
            CreateCorner(new Vector3(70f, 0f, 50f), TurnType.EastToNorth, 10f, 8f, matL3Rock, matL3Path, "WallBrazier", true);

            // Segment 3 (Heading +Z along X=70, from Z=55 to Z=115) - Length 60m
            CreateZCorridor(70f, 55f, 115f, 10f, 8f, true, matL3Rock, matL3Path, "WallBrazier", true);
            SpawnPrefab("SideHazard_Left", new Vector3(70f, 3.8f, 68f), Quaternion.identity);
            SpawnPrefab("BrokenTrackGap", new Vector3(70f, 4.0f, 80f), Quaternion.identity);
            SpawnPrefab("LowHangingArch", new Vector3(70f, 4.2f, 92f), Quaternion.identity);
            SpawnPrefab("SideHazard_Right", new Vector3(70f, 4.3f, 104f), Quaternion.identity);

            // CORNER 3: Turn to the Left (-X) at (70, 0, 120)
            CreateCorner(new Vector3(70f, 0f, 120f), TurnType.NorthToWest, 10f, 8f, matL3Rock, matL3Path, "WallBrazier", true);

            // Segment 4: The Extreme Gauntlet (Heading -X along Z=120, from X=65 down to X=-45) - Length 110m!
            CreateXCorridor(120f, 65f, -45f, 10f, 8f, true, matL3Rock, matL3Path, "WallBrazier", true);
            SpawnPrefab("BrokenTrackGap", new Vector3(48f, 5.0f, 120f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("LowHangingArch", new Vector3(36f, 5.2f, 120f), Quaternion.Euler(0f, -90f, 0f)); // JUMP + DUCK COMBO!
            SpawnPrefab("SideHazard_Left", new Vector3(24f, 5.4f, 120f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("SideHazard_Right", new Vector3(10f, 5.5f, 120f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("BrokenTrackGap", new Vector3(-6f, 5.5f, 120f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("LowHangingArch", new Vector3(-18f, 5.5f, 120f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("SideHazard_Left", new Vector3(-32f, 5.5f, 120f), Quaternion.Euler(0f, -90f, 0f));

            // CORNER 4: Turn to the Right (+Z) at (-50, 0, 120)
            CreateCorner(new Vector3(-50f, 0f, 120f), TurnType.WestToNorth, 10f, 8f, matL3Rock, matL3Path, "WallBrazier", true);

            // Segment 5: Summit Mountain Climb (Heading +Z along X=-50, from Z=125 to Z=155) - Length 30m
            CreateZCorridor(-50f, 125f, 155f, 10f, 8f, true, matL3Rock, matL3Path, "WallBrazier", true);
            SpawnPrefab("SideHazard_Right", new Vector3(-50f, 5.0f, 135f), Quaternion.identity);
            SpawnPrefab("LowHangingArch", new Vector3(-50f, 4.7f, 146f), Quaternion.identity);

            // CORNER 5: Turn to the Right (+X) at (-50, 0, 160)
            CreateCorner(new Vector3(-50f, 0f, 160f), TurnType.NorthToEast, 10f, 8f, matL3Rock, matL3Path, "WallBrazier", true);

            // Segment 6: Final Plunge Approach (Heading +X along Z=160, from X=-45 to X=-15) - Length 30m
            CreateXCorridor(160f, -45f, -15f, 10f, 8f, true, matL3Rock, matL3Path, "WallBrazier", true);
            SpawnPrefab("BrokenTrackGap", new Vector3(-28f, 2.8f, 160f), Quaternion.Euler(0f, 90f, 0f));

            // THE GRAND GOLDEN TREASURE CHAMBER CENTERED AT (0, 0, 160)
            Vector3 chamberPos = new Vector3(0f, 0f, 160f);
            CreateTreasureChamber(chamberPos, 30f, 10f, matL3Rock, matL3Path);

            // Royal Dais & Golden Treasure Chest placed at the terminus of the coaster!
            SpawnPrefab("TreasureChest", chamberPos + new Vector3(1.8f, 0.1f, 0f), Quaternion.Euler(0f, -90f, 0f));

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/CaveLevel3.unity");
        }

        private static void BuildGameCompleteScene()
        {
            SetupSceneBase("GameComplete", new Color(0.20f, 0.16f, 0.08f), new Color(0.08f, 0.06f, 0.02f), 0.015f);

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.02f);
            camObj.transform.position = new Vector3(0f, 3.2f, -5f);
            camObj.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            camObj.AddComponent<AudioListener>();

            CreateTreasureChamber(new Vector3(0f, 0f, 4f), 20f, 8f, matL3Rock, matL3Path);
            SpawnPrefab("TreasureChest", new Vector3(0f, 0.1f, 3.5f), Quaternion.identity);

            GameObject canvasObj = CreateCanvas("GameCompleteCanvas");
            GameCompleteUI gcUI = canvasObj.AddComponent<GameCompleteUI>();

            CreateText(canvasObj.transform, "VictoryTitle", "CONGRATULATIONS!\nYOU FOUND THE LOST TREASURE!", 50, new Vector2(0f, 170f), new Vector2(900f, 150f), new Color(1.0f, 0.85f, 0.2f), FontStyle.Bold);
            CreateText(canvasObj.transform, "Subtitle", "You braved the crumbling bridges, navigated treacherous turns,\nand claimed the ancient relics of the Lost Cave!", 24, new Vector2(0f, 50f), new Vector2(850f, 70f), Color.white, FontStyle.Bold);

            gcUI.playAgainButton = CreateButton(canvasObj.transform, "PlayAgainBtn", "PLAY AGAIN", new Vector2(0f, -40f), new Vector2(280f, 54f), new Color(0.9f, 0.6f, 0.15f));
            gcUI.levelSelectButton = CreateButton(canvasObj.transform, "LevelSelectBtn", "LEVEL SELECTION", new Vector2(0f, -110f), new Vector2(280f, 54f), new Color(0.2f, 0.5f, 0.5f));
            gcUI.mainMenuButton = CreateButton(canvasObj.transform, "MainMenuBtn", "MAIN MENU", new Vector2(0f, -180f), new Vector2(280f, 54f), new Color(0.5f, 0.2f, 0.2f));

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_DIR + "/GameComplete.unity");
        }

        // =========================================================================
        // PROCEDURAL SEAMLESS CORRIDOR & CORNER BUILDER (MATHEMATICALLY WATERTIGHT)
        // =========================================================================
        private static void CreateAncientColumn(Transform parent, Vector3 localPos, float height, Material colMat)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "TempleColumn";
            pillar.transform.SetParent(parent);
            pillar.transform.localPosition = localPos;
            pillar.transform.localScale = new Vector3(1.1f, height * 0.5f, 1.1f);
            pillar.GetComponent<Renderer>().sharedMaterial = colMat;

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "GoldCapital";
            cap.transform.SetParent(pillar.transform);
            cap.transform.localPosition = new Vector3(0f, 0.92f, 0f);
            cap.transform.localScale = new Vector3(1.4f, 0.16f, 1.4f);
            if (cap.GetComponent<Collider>()) DestroyImmediate(cap.GetComponent<Collider>());
            cap.GetComponent<Renderer>().sharedMaterial = matGold;
        }

        private static void CreateCrystalPool(Vector3 pos, Vector3 size)
        {
            GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pool.name = "CrystalWaterPool";
            pool.transform.position = pos;
            pool.transform.localScale = size;
            if (pool.GetComponent<Collider>()) DestroyImmediate(pool.GetComponent<Collider>());
            pool.GetComponent<Renderer>().sharedMaterial = matWater;

            GameObject lightObj = new GameObject("PoolLight");
            lightObj.transform.SetParent(pool.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.1f, 0.95f, 1.0f);
            l.range = 7.0f;
            l.intensity = 2.5f;
            l.shadows = LightShadows.None;
        }

        private static void CreateCorridor(Vector3 start, Vector3 end, float width = 8f, float height = 6.5f, bool hasFloor = true)
        {
            CreateZCorridor(start.x, start.z, end.z, width, height, hasFloor, matL1Rock, matL1Path, "Torch", false);
        }

        private static void CreateZCorridor(float x, float zStart, float zEnd, bool hasFloor)
        {
            CreateZCorridor(x, zStart, zEnd, 8f, 6.5f, hasFloor, matL1Rock, matL1Path, "Torch", false);
        }

        private static void CreateXCorridor(float z, float xStart, float xEnd, bool hasFloor)
        {
            CreateXCorridor(z, xStart, xEnd, 8f, 6.5f, hasFloor, matL1Rock, matL1Path, "Torch", false);
        }

        private static void CreateZCorridor(float x, float zStart, float zEnd, float width, float height, bool hasFloor)
        {
            CreateZCorridor(x, zStart, zEnd, width, height, hasFloor, matL1Rock, matL1Path, "Torch", false);
        }

        private static void CreateXCorridor(float z, float xStart, float xEnd, float width, float height, bool hasFloor)
        {
            CreateXCorridor(z, xStart, xEnd, width, height, hasFloor, matL1Rock, matL1Path, "Torch", false);
        }

        private static void CreateZCorridor(float x, float zStart, float zEnd, float width = 8f, float height = 6.5f, bool hasFloor = true, Material wallMat = null, Material floorMat = null, string lightPrefab = "Torch", bool addPillars = false)
        {
            float minZ = Mathf.Min(zStart, zEnd);
            float maxZ = Mathf.Max(zStart, zEnd);
            float length = maxZ - minZ;
            if (length < 0.1f) return;

            Material actualFloorMat = floorMat != null ? floorMat : (matL1Path != null ? matL1Path : matPath);
            Material actualWallMat = wallMat != null ? wallMat : (matL1Rock != null ? matL1Rock : matRock);

            float midZ = (minZ + maxZ) * 0.5f;
            GameObject corridor = new GameObject("ZCorridor_" + (int)x + "_" + (int)minZ + "_" + (int)maxZ);
            corridor.transform.position = new Vector3(x, 0f, midZ);

            if (hasFloor)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Floor";
                floor.transform.SetParent(corridor.transform);
                floor.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                floor.transform.localScale = new Vector3(width, 0.4f, length);
                floor.GetComponent<Renderer>().sharedMaterial = actualFloorMat;
            }

            GameObject ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "Ceiling";
            ceil.transform.SetParent(corridor.transform);
            ceil.transform.localPosition = new Vector3(0f, height, 0f);
            ceil.transform.localScale = new Vector3(width, 0.6f, length);
            ceil.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall_NegX";
            leftWall.transform.SetParent(corridor.transform);
            leftWall.transform.localPosition = new Vector3(-width * 0.5f, height * 0.5f, 0f);
            leftWall.transform.localScale = new Vector3(1.2f, height, length);
            leftWall.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall_PosX";
            rightWall.transform.SetParent(corridor.transform);
            rightWall.transform.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0f);
            rightWall.transform.localScale = new Vector3(1.2f, height, length);
            rightWall.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            // Stalactites
            int stCount = Mathf.Max(1, (int)(length / 7f));
            for (int s = 0; s < stCount; s++)
            {
                GameObject st = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                st.name = "Stalactite_" + s;
                st.transform.SetParent(corridor.transform);
                float xOff = ((s % 2 == 0) ? -1f : 1f) * UnityEngine.Random.Range(width * 0.36f, width * 0.45f);
                float zOff = (s - stCount * 0.5f) * (length / stCount);
                st.transform.localPosition = new Vector3(xOff, height - 0.7f, zOff);
                st.transform.localScale = new Vector3(0.28f, 0.85f, 0.28f);
                if (st.GetComponent<Collider>()) DestroyImmediate(st.GetComponent<Collider>());
                st.GetComponent<Renderer>().sharedMaterial = (lightPrefab == "CrystalSconce") ? ((s % 2 == 0) ? matTurquoise : matEmerald) : actualWallMat;
            }

            // Lights alternating sides
            int lightCount = (int)(length / 14f);
            for (int t = 0; t < lightCount; t++)
            {
                float tz = minZ + 7f + t * 14f;
                bool isLeft = (t % 2 == 0);
                float tx = isLeft ? (x - width * 0.5f + 0.4f) : (x + width * 0.5f - 0.4f);
                Quaternion rot = isLeft ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
                SpawnPrefab(lightPrefab, new Vector3(tx, 2.4f, tz), rot);

                if (addPillars)
                {
                    CreateAncientColumn(corridor.transform, new Vector3(isLeft ? (-width * 0.5f + 0.6f) : (width * 0.5f - 0.6f), height * 0.5f, tz - midZ), height, actualWallMat);
                }
            }
        }

        private static void CreateXCorridor(float z, float xStart, float xEnd, float width = 8f, float height = 6.5f, bool hasFloor = true, Material wallMat = null, Material floorMat = null, string lightPrefab = "Torch", bool addPillars = false)
        {
            float minX = Mathf.Min(xStart, xEnd);
            float maxX = Mathf.Max(xStart, xEnd);
            float length = maxX - minX;
            if (length < 0.1f) return;

            Material actualFloorMat = floorMat != null ? floorMat : (matL1Path != null ? matL1Path : matPath);
            Material actualWallMat = wallMat != null ? wallMat : (matL1Rock != null ? matL1Rock : matRock);

            float midX = (minX + maxX) * 0.5f;
            GameObject corridor = new GameObject("XCorridor_" + (int)z + "_" + (int)minX + "_" + (int)maxX);
            corridor.transform.position = new Vector3(midX, 0f, z);

            if (hasFloor)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Floor";
                floor.transform.SetParent(corridor.transform);
                floor.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                floor.transform.localScale = new Vector3(length, 0.4f, width);
                floor.GetComponent<Renderer>().sharedMaterial = actualFloorMat;
            }

            GameObject ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "Ceiling";
            ceil.transform.SetParent(corridor.transform);
            ceil.transform.localPosition = new Vector3(0f, height, 0f);
            ceil.transform.localScale = new Vector3(length, 0.6f, width);
            ceil.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            GameObject northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            northWall.name = "NorthWall_PosZ";
            northWall.transform.SetParent(corridor.transform);
            northWall.transform.localPosition = new Vector3(0f, height * 0.5f, width * 0.5f);
            northWall.transform.localScale = new Vector3(length, height, 1.2f);
            northWall.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWall.name = "SouthWall_NegZ";
            southWall.transform.SetParent(corridor.transform);
            southWall.transform.localPosition = new Vector3(0f, height * 0.5f, -width * 0.5f);
            southWall.transform.localScale = new Vector3(length, height, 1.2f);
            southWall.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            // Stalactites
            int stCount = Mathf.Max(1, (int)(length / 7f));
            for (int s = 0; s < stCount; s++)
            {
                GameObject st = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                st.name = "Stalactite_" + s;
                st.transform.SetParent(corridor.transform);
                float zOff = ((s % 2 == 0) ? -1f : 1f) * UnityEngine.Random.Range(width * 0.36f, width * 0.45f);
                float xOff = (s - stCount * 0.5f) * (length / stCount);
                st.transform.localPosition = new Vector3(xOff, height - 0.7f, zOff);
                st.transform.localScale = new Vector3(0.28f, 0.85f, 0.28f);
                if (st.GetComponent<Collider>()) DestroyImmediate(st.GetComponent<Collider>());
                st.GetComponent<Renderer>().sharedMaterial = (lightPrefab == "CrystalSconce") ? ((s % 2 == 0) ? matTurquoise : matEmerald) : actualWallMat;
            }

            // Lights alternating sides
            int lightCount = (int)(length / 14f);
            for (int t = 0; t < lightCount; t++)
            {
                float tx = minX + 7f + t * 14f;
                bool isNorth = (t % 2 == 0);
                float tz = isNorth ? (z + width * 0.5f - 0.4f) : (z - width * 0.5f + 0.4f);
                Quaternion rot = isNorth ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
                SpawnPrefab(lightPrefab, new Vector3(tx, 2.4f, tz), rot);

                if (addPillars)
                {
                    CreateAncientColumn(corridor.transform, new Vector3(tx - midX, height * 0.5f, isNorth ? (width * 0.5f - 0.6f) : (-width * 0.5f + 0.6f)), height, actualWallMat);
                }
            }
        }

        private static void CreateCorner(Vector3 center, TurnType turn, float width = 8f, float height = 6.5f, Material wallMat = null, Material floorMat = null, string lightPrefab = "Torch", bool addPillars = false)
        {
            Material actualFloorMat = floorMat != null ? floorMat : (matL1Path != null ? matL1Path : matPath);
            Material actualWallMat = wallMat != null ? wallMat : (matL1Rock != null ? matL1Rock : matRock);

            GameObject corner = new GameObject("Corner_" + turn + "_" + (int)center.x + "_" + (int)center.z);
            corner.transform.position = center;

            // Floor & Ceiling
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "CornerFloor";
            floor.transform.SetParent(corner.transform);
            floor.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            floor.transform.localScale = new Vector3(width, 0.4f, width);
            floor.GetComponent<Renderer>().sharedMaterial = actualFloorMat;

            GameObject ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "CornerCeiling";
            ceil.transform.SetParent(corner.transform);
            ceil.transform.localPosition = new Vector3(0f, height, 0f);
            ceil.transform.localScale = new Vector3(width, 0.6f, width);
            ceil.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            float hW = width * 0.5f;

            switch (turn)
            {
                case TurnType.NorthToEast: // +Z into +X (Turns Right)
                    CreateWall(corner.transform, "NorthBackWall", new Vector3(0f, height * 0.5f, hW), new Vector3(width + 1.2f, height, 1.2f), actualWallMat);
                    CreateWall(corner.transform, "WestOuterWall", new Vector3(-hW, height * 0.5f, 0f), new Vector3(1.2f, height, width + 1.2f), actualWallMat);
                    CreateCornerPillar(corner.transform, new Vector3(hW, height * 0.5f, -hW), height, actualWallMat);
                    SpawnPrefab(lightPrefab, center + new Vector3(0f, 2.5f, hW - 0.4f), Quaternion.Euler(0f, 180f, 0f));
                    if (lightPrefab == "CrystalSconce")
                        SpawnPrefab("CrystalCluster_Turquoise", center + new Vector3(-hW * 0.6f, 0.1f, hW * 0.6f), Quaternion.identity);
                    break;

                case TurnType.EastToNorth: // +X into +Z (Turns Left)
                    CreateWall(corner.transform, "EastBackWall", new Vector3(hW, height * 0.5f, 0f), new Vector3(1.2f, height, width + 1.2f), actualWallMat);
                    CreateWall(corner.transform, "SouthOuterWall", new Vector3(0f, height * 0.5f, -hW), new Vector3(width + 1.2f, height, 1.2f), actualWallMat);
                    CreateCornerPillar(corner.transform, new Vector3(-hW, height * 0.5f, hW), height, actualWallMat);
                    SpawnPrefab(lightPrefab, center + new Vector3(hW - 0.4f, 2.5f, 0f), Quaternion.Euler(0f, -90f, 0f));
                    if (lightPrefab == "CrystalSconce")
                        SpawnPrefab("CrystalCluster_Emerald", center + new Vector3(hW * 0.6f, 0.1f, -hW * 0.6f), Quaternion.identity);
                    break;

                case TurnType.NorthToWest: // +Z into -X (Turns Left)
                    CreateWall(corner.transform, "NorthBackWall", new Vector3(0f, height * 0.5f, hW), new Vector3(width + 1.2f, height, 1.2f), actualWallMat);
                    CreateWall(corner.transform, "EastOuterWall", new Vector3(hW, height * 0.5f, 0f), new Vector3(1.2f, height, width + 1.2f), actualWallMat);
                    CreateCornerPillar(corner.transform, new Vector3(-hW, height * 0.5f, -hW), height, actualWallMat);
                    SpawnPrefab(lightPrefab, center + new Vector3(0f, 2.5f, hW - 0.4f), Quaternion.Euler(0f, 180f, 0f));
                    if (lightPrefab == "CrystalSconce")
                        SpawnPrefab("CrystalCluster_Emerald", center + new Vector3(hW * 0.6f, 0.1f, hW * 0.6f), Quaternion.identity);
                    break;

                case TurnType.WestToNorth: // -X into +Z (Turns Right)
                    CreateWall(corner.transform, "WestBackWall", new Vector3(-hW, height * 0.5f, 0f), new Vector3(1.2f, height, width + 1.2f), actualWallMat);
                    CreateWall(corner.transform, "SouthOuterWall", new Vector3(0f, height * 0.5f, -hW), new Vector3(width + 1.2f, height, 1.2f), actualWallMat);
                    CreateCornerPillar(corner.transform, new Vector3(hW, height * 0.5f, hW), height, actualWallMat);
                    SpawnPrefab(lightPrefab, center + new Vector3(-hW + 0.4f, 2.5f, 0f), Quaternion.Euler(0f, 90f, 0f));
                    if (lightPrefab == "CrystalSconce")
                        SpawnPrefab("CrystalCluster_Turquoise", center + new Vector3(-hW * 0.6f, 0.1f, -hW * 0.6f), Quaternion.identity);
                    break;
            }
        }

        private static void CreateWall(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat = null)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = localScale;
            wall.GetComponent<Renderer>().sharedMaterial = mat != null ? mat : matRock;
        }

        private static void CreateCornerPillar(Transform parent, Vector3 localPos, float height, Material mat = null)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "InnerCornerPillar";
            pillar.transform.SetParent(parent);
            pillar.transform.localPosition = localPos;
            pillar.transform.localScale = new Vector3(1.3f, height * 0.5f, 1.3f);
            pillar.GetComponent<Renderer>().sharedMaterial = mat != null ? mat : matRock;
        }

        private static void CreateEndWall(Vector3 worldPos, Vector3 scale, Material mat = null)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "CaveDeadEndWall";
            wall.transform.position = worldPos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = mat != null ? mat : matRock;
        }

        private static void CreateTreasureChamber(Vector3 center, float size = 28f, float height = 9f, Material wallMat = null, Material floorMat = null)
        {
            Material actualWallMat = wallMat != null ? wallMat : (matL3Rock != null ? matL3Rock : matRock);
            Material actualFloorMat = floorMat != null ? floorMat : (matL3Path != null ? matL3Path : matPath);

            GameObject room = new GameObject("TreasureChamberRoom");
            room.transform.position = center;

            // Grand Floor & Ceiling
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ChamberFloor";
            floor.transform.SetParent(room.transform);
            floor.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            floor.transform.localScale = new Vector3(size, 0.4f, size);
            floor.GetComponent<Renderer>().sharedMaterial = actualFloorMat;

            GameObject ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "ChamberCeiling";
            ceil.transform.SetParent(room.transform);
            ceil.transform.localPosition = new Vector3(0f, height, 0f);
            ceil.transform.localScale = new Vector3(size, 0.6f, size);
            ceil.GetComponent<Renderer>().sharedMaterial = actualWallMat;

            float hS = size * 0.5f;

            // North Wall (+Z)
            CreateWall(room.transform, "ChamberNorthWall", new Vector3(0f, height * 0.5f, hS), new Vector3(size + 1.2f, height, 1.2f), actualWallMat);

            // South Wall (-Z)
            CreateWall(room.transform, "ChamberSouthWall", new Vector3(0f, height * 0.5f, -hS), new Vector3(size + 1.2f, height, 1.2f), actualWallMat);

            // East Wall (+X - Solid back wall behind chest)
            CreateWall(room.transform, "ChamberEastBackWall", new Vector3(hS, height * 0.5f, 0f), new Vector3(1.2f, height, size + 1.2f), actualWallMat);

            // West Wall (-X - Entrance archway connecting 10m corridor)
            CreateWall(room.transform, "WestWall_SouthWing", new Vector3(-hS, height * 0.5f, -10f), new Vector3(1.2f, height, 10f), actualWallMat);
            CreateWall(room.transform, "WestWall_NorthWing", new Vector3(-hS, height * 0.5f, 10f), new Vector3(1.2f, height, 10f), actualWallMat);
            CreateWall(room.transform, "WestWall_ArchLintel", new Vector3(-hS, height - 1.0f, 0f), new Vector3(1.2f, 2.0f, 10.5f), actualWallMat);

            // 4 Giant Ancient Pillars
            Vector3[] pillarOffsets = new Vector3[] {
                new Vector3(-9f, height * 0.5f, -9f),
                new Vector3(9f, height * 0.5f, -9f),
                new Vector3(-9f, height * 0.5f, 9f),
                new Vector3(9f, height * 0.5f, 9f)
            };

            foreach (var offset in pillarOffsets)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "GrandAncientPillar";
                pillar.transform.SetParent(room.transform);
                pillar.transform.localPosition = offset;
                pillar.transform.localScale = new Vector3(1.8f, height * 0.5f, 1.8f);
                pillar.GetComponent<Renderer>().sharedMaterial = actualWallMat;

                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cap.name = "PillarGoldCapital";
                cap.transform.SetParent(pillar.transform);
                cap.transform.localPosition = new Vector3(0f, 0.94f, 0f);
                cap.transform.localScale = new Vector3(1.35f, 0.14f, 1.35f);
                if (cap.GetComponent<Collider>()) DestroyImmediate(cap.GetComponent<Collider>());
                cap.GetComponent<Renderer>().sharedMaterial = matGold;
            }

            // Central Chest Dais Pedestal
            GameObject dais = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dais.name = "TreasureDais";
            dais.transform.SetParent(room.transform);
            dais.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            dais.transform.localScale = new Vector3(5.5f, 0.3f, 5.5f);
            dais.GetComponent<Renderer>().sharedMaterial = actualFloorMat;

            GameObject daisRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            daisRim.name = "DaisGoldRim";
            daisRim.transform.SetParent(dais.transform);
            daisRim.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            daisRim.transform.localScale = new Vector3(1.05f, 0.12f, 1.05f);
            if (daisRim.GetComponent<Collider>()) DestroyImmediate(daisRim.GetComponent<Collider>());
            daisRim.GetComponent<Renderer>().sharedMaterial = matGold;

            // 16 Mounds of Gold Coins
            for (int c = 0; c < 16; c++)
            {
                GameObject coinPile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coinPile.name = "CoinPile_" + c;
                coinPile.transform.SetParent(room.transform);
                float ang = c * 22.5f * Mathf.Deg2Rad;
                float r = (c % 2 == 0) ? 6.8f : 5.4f;
                coinPile.transform.localPosition = new Vector3(Mathf.Cos(ang) * r, 0.15f, Mathf.Sin(ang) * (r * 0.85f));
                coinPile.transform.localScale = new Vector3(2.5f, 0.55f, 2.5f);
                if (coinPile.GetComponent<Collider>()) DestroyImmediate(coinPile.GetComponent<Collider>());
                coinPile.GetComponent<Renderer>().sharedMaterial = matGold;
            }

            // Ornate Golden Wall Braziers illuminating the chamber
            SpawnPrefab("WallBrazier", center + new Vector3(-9f, 3.0f, -6f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("WallBrazier", center + new Vector3(9f, 3.0f, -6f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("WallBrazier", center + new Vector3(-9f, 3.0f, 6f), Quaternion.Euler(0f, 90f, 0f));
            SpawnPrefab("WallBrazier", center + new Vector3(9f, 3.0f, 6f), Quaternion.Euler(0f, -90f, 0f));
            SpawnPrefab("WallBrazier", center + new Vector3(hS - 0.5f, 3.0f, 0f), Quaternion.Euler(0f, -90f, 0f));

            // Glowing Crystals
            SpawnPrefab("CrystalCluster_Turquoise", center + new Vector3(-4f, 0.1f, 3.5f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Emerald", center + new Vector3(4f, 0.1f, 3.5f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Turquoise", center + new Vector3(-4f, 0.1f, -3.5f), Quaternion.identity);
            SpawnPrefab("CrystalCluster_Emerald", center + new Vector3(4f, 0.1f, -3.5f), Quaternion.identity);

            // Grand Sunbeam Spotlight pouring from ceiling down to chest
            GameObject spotObj = new GameObject("ChamberSunSpot");
            spotObj.transform.SetParent(room.transform);
            spotObj.transform.localPosition = new Vector3(0f, height, 0f);
            spotObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light spot = spotObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1.0f, 0.9f, 0.55f);
            spot.range = 22f;
            spot.spotAngle = 60f;
            spot.intensity = 6.5f;
            spot.shadows = LightShadows.None;
        }

        private static void CreateAbyssGap(Vector3 center, Vector3 size)
        {
            GameObject abyss = new GameObject("AbyssTriggerZone");
            abyss.transform.position = center;
            BoxCollider bc = abyss.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = size;

            abyss.AddComponent<AbyssTrigger>();
        }

        private static void CreateWaterPool(Vector3 pos, Vector3 size)
        {
            GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pool.name = "UndergroundWaterPool";
            pool.transform.position = pos;
            pool.transform.localScale = size;
            if (pool.GetComponent<Collider>()) DestroyImmediate(pool.GetComponent<Collider>());
            pool.GetComponent<Renderer>().sharedMaterial = matWater;
        }

        private static void CreateCheckpoint(Vector3 position)
        {
            GameObject cp = new GameObject("Checkpoint");
            cp.transform.position = position;

            BoxCollider bc = cp.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(4f, 3f, 3f);

            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "RuneCircle";
            indicator.transform.SetParent(cp.transform);
            indicator.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            indicator.transform.localScale = new Vector3(2.0f, 0.04f, 2.0f);
            if (indicator.GetComponent<Collider>()) DestroyImmediate(indicator.GetComponent<Collider>());
            indicator.GetComponent<Renderer>().sharedMaterial = matTurquoise;

            GameObject lightObj = new GameObject("CheckpointLight");
            lightObj.transform.SetParent(cp.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 1.0f, 0.7f);
            l.range = 5.5f;
            l.intensity = 2.5f;
            l.shadows = LightShadows.None;

            Checkpoint cpComp = cp.AddComponent<Checkpoint>();
            cpComp.activeIndicatorLight = l;
            cpComp.checkpointClip = LoadAudio("UI/Checkpoint.wav");
        }

        private static void CreateLevelExit(Vector3 position, string nextScene, string message, Material mat = null)
        {
            GameObject exit = new GameObject("LevelExitPortal");
            exit.transform.position = position;

            BoxCollider bc = exit.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(6f, 5f, 4f);
            bc.center = new Vector3(0f, 2.5f, 0f);

            Material portalMat = mat != null ? mat : matRock;

            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "ExitPillar_Left";
            left.transform.SetParent(exit.transform);
            left.transform.localPosition = new Vector3(-2.2f, 1.8f, 0f);
            left.transform.localScale = new Vector3(0.6f, 3.6f, 0.6f);
            if (left.GetComponent<Collider>()) DestroyImmediate(left.GetComponent<Collider>());
            left.GetComponent<Renderer>().sharedMaterial = portalMat;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "ExitPillar_Right";
            right.transform.SetParent(exit.transform);
            right.transform.localPosition = new Vector3(2.2f, 1.8f, 0f);
            right.transform.localScale = new Vector3(0.6f, 3.6f, 0.6f);
            if (right.GetComponent<Collider>()) DestroyImmediate(right.GetComponent<Collider>());
            right.GetComponent<Renderer>().sharedMaterial = portalMat;

            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "ExitHeader_Top";
            top.transform.SetParent(exit.transform);
            top.transform.localPosition = new Vector3(0f, 3.6f, 0f);
            top.transform.localScale = new Vector3(5.0f, 0.6f, 0.6f);
            if (top.GetComponent<Collider>()) DestroyImmediate(top.GetComponent<Collider>());
            top.GetComponent<Renderer>().sharedMaterial = portalMat;

            LevelExit le = exit.AddComponent<LevelExit>();
            le.nextSceneName = nextScene;
            le.exitClip = LoadAudio("UI/LevelStart.wav");
        }

        private static void CreateAerialLevelExit(Vector3 position, string nextScene, string message, float width = 12f, float height = 10f)
        {
            GameObject exit = new GameObject("AerialLevelExitPortal");
            exit.transform.position = position;

            // Trigger spans full 12m tunnel width, 10m height, and 8m depth so flying player cannot miss it!
            BoxCollider bc = exit.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(width, height, 8f);
            bc.center = new Vector3(0f, height * 0.5f, 0f);

            Material frameMat = matL2Rock != null ? matL2Rock : matRock;
            Material glowMat = matTurquoise != null ? matTurquoise : matL2Rock;

            // Left Stargate Pillar
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "ArchPillar_Left";
            left.transform.SetParent(exit.transform);
            left.transform.localPosition = new Vector3(-width * 0.5f + 0.6f, height * 0.5f, 0f);
            left.transform.localScale = new Vector3(1.2f, height, 1.4f);
            if (left.GetComponent<Collider>()) DestroyImmediate(left.GetComponent<Collider>());
            left.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Right Stargate Pillar
            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "ArchPillar_Right";
            right.transform.SetParent(exit.transform);
            right.transform.localPosition = new Vector3(width * 0.5f - 0.6f, height * 0.5f, 0f);
            right.transform.localScale = new Vector3(1.2f, height, 1.4f);
            if (right.GetComponent<Collider>()) DestroyImmediate(right.GetComponent<Collider>());
            right.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Top Arch Header Beam
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "ArchHeader_Top";
            top.transform.SetParent(exit.transform);
            top.transform.localPosition = new Vector3(0f, height - 0.5f, 0f);
            top.transform.localScale = new Vector3(width, 1.0f, 1.4f);
            if (top.GetComponent<Collider>()) DestroyImmediate(top.GetComponent<Collider>());
            top.GetComponent<Renderer>().sharedMaterial = frameMat;

            // Massive Glowing Energy Vortex Field (The celestial gateway)
            GameObject vortex = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vortex.name = "AerialVortexField";
            vortex.transform.SetParent(exit.transform);
            vortex.transform.localPosition = new Vector3(0f, height * 0.5f, 0.2f);
            vortex.transform.localScale = new Vector3(width - 1.2f, height - 1.2f, 0.2f);
            if (vortex.GetComponent<Collider>()) DestroyImmediate(vortex.GetComponent<Collider>());
            vortex.GetComponent<Renderer>().sharedMaterial = glowMat;

            // Brilliant Turquoise Energy Illumination
            GameObject lightObj = new GameObject("PortalVortexLight");
            lightObj.transform.SetParent(exit.transform);
            lightObj.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 0.95f, 1.0f);
            l.range = 22f;
            l.intensity = 6.0f;
            l.shadows = LightShadows.None;

            LevelExit le = exit.AddComponent<LevelExit>();
            le.nextSceneName = nextScene;
            le.delayBeforeLoad = 1.0f;
            le.exitClip = LoadAudio("UI/LevelStart.wav");
        }

        private static GameObject CreateThirdPersonCamera(Transform target)
        {
            GameObject camObj = new GameObject("ThirdPersonCamera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.07f, 0.10f);
            cam.fieldOfView = 65f;
            camObj.AddComponent<AudioListener>();

            ThirdPersonCamera tpc = camObj.AddComponent<ThirdPersonCamera>();
            tpc.target = target;
            tpc.defaultDistance = 4.5f;

            return camObj;
        }

        private static GameObject CreateGameplayHUDCanvas()
        {
            GameObject canvasObj = CreateCanvas("GameplayHUDCanvas");
            GameplayHUD hud = canvasObj.AddComponent<GameplayHUD>();

            GameObject topBar = CreatePanel(canvasObj.transform, "TopBar", new Vector2(960f, 70f), new Color(0.08f, 0.1f, 0.14f, 0.88f));
            RectTransform topBarRT = topBar.GetComponent<RectTransform>();
            topBarRT.anchorMin = new Vector2(0.5f, 1.0f);
            topBarRT.anchorMax = new Vector2(0.5f, 1.0f);
            topBarRT.pivot = new Vector2(0.5f, 1.0f);
            topBarRT.anchoredPosition = new Vector2(0f, -15f);

            hud.levelTitleText = CreateText(topBar.transform, "LevelTitle", "Stage 1: Cave Entrance", 24, new Vector2(-190f, 14f), new Vector2(500f, 34f), new Color(1.0f, 0.82f, 0.3f), FontStyle.Bold);
            hud.objectiveText = CreateText(topBar.transform, "Objective", "Explore the cave paths and avoid hazards.", 18, new Vector2(-190f, -14f), new Vector2(500f, 28f), Color.white, FontStyle.Bold);

            hud.healthText = CreateText(topBar.transform, "HealthText", "HEALTH: 3 / 3", 22, new Vector2(300f, 0f), new Vector2(200f, 34f), new Color(0.3f, 0.95f, 0.5f), FontStyle.Bold);

            GameObject promptPanel = CreatePanel(canvasObj.transform, "InteractionPromptPanel", new Vector2(460f, 56f), new Color(0.12f, 0.15f, 0.2f, 0.92f));
            hud.promptPanel = promptPanel;
            hud.promptText = CreateText(promptPanel.transform, "PromptText", "Press [E] to Interact", 22, Vector2.zero, new Vector2(440f, 44f), new Color(1.0f, 0.9f, 0.4f), FontStyle.Bold);
            promptPanel.SetActive(false);

            GameObject notifPanel = CreatePanel(canvasObj.transform, "NotificationPanel", new Vector2(540f, 56f), new Color(0.15f, 0.35f, 0.45f, 0.92f));
            RectTransform notifRT = notifPanel.GetComponent<RectTransform>();
            notifRT.anchoredPosition = new Vector2(0f, 160f);
            hud.notificationPanel = notifPanel;
            hud.notificationText = CreateText(notifPanel.transform, "NotifText", "Checkpoint Activated!", 22, Vector2.zero, new Vector2(520f, 44f), Color.white, FontStyle.Bold);
            notifPanel.SetActive(false);

            GameObject pauseMenu = CreatePanel(canvasObj.transform, "PauseMenuPanel", new Vector2(420f, 380f), new Color(0.08f, 0.1f, 0.14f, 0.96f));
            hud.pauseMenuPanel = pauseMenu;
            CreateText(pauseMenu.transform, "PauseTitle", "GAME PAUSED", 32, new Vector2(0f, 140f), new Vector2(340f, 44f), Color.white, FontStyle.Bold);
            hud.resumeButton = CreateButton(pauseMenu.transform, "ResumeBtn", "RESUME", new Vector2(0f, 70f), new Vector2(240f, 48f), new Color(0.2f, 0.5f, 0.5f));
            hud.restartButton = CreateButton(pauseMenu.transform, "RestartBtn", "RESTART LEVEL", new Vector2(0f, 12f), new Vector2(240f, 48f), new Color(0.45f, 0.45f, 0.2f));
            hud.levelSelectButton = CreateButton(pauseMenu.transform, "LvlSelectBtn", "LEVEL SELECT", new Vector2(0f, -46f), new Vector2(240f, 48f), new Color(0.2f, 0.4f, 0.55f));
            hud.mainMenuButton = CreateButton(pauseMenu.transform, "MainMenuBtn", "MAIN MENU", new Vector2(0f, -104f), new Vector2(240f, 48f), new Color(0.55f, 0.2f, 0.2f));
            pauseMenu.SetActive(false);

            GameObject gameOver = CreatePanel(canvasObj.transform, "GameOverPanel", new Vector2(500f, 380f), new Color(0.14f, 0.04f, 0.04f, 0.97f));
            hud.gameOverPanel = gameOver;
            CreateText(gameOver.transform, "GameOverTitle", "GAME OVER", 44, new Vector2(0f, 130f), new Vector2(420f, 54f), new Color(1.0f, 0.25f, 0.25f), FontStyle.Bold);
            CreateText(gameOver.transform, "GameOverSubtitle", "Your journey ended in the depths of the Lost Cave.", 18, new Vector2(0f, 85f), new Vector2(440f, 30f), new Color(0.85f, 0.85f, 0.85f), FontStyle.Normal);
            
            hud.gameOverRestartButton = CreateButton(gameOver.transform, "GameOverRestartBtn", "RESTART LEVEL [R]", new Vector2(0f, 25f), new Vector2(280f, 50f), new Color(0.85f, 0.35f, 0.15f));
            hud.retryCheckpointButton = CreateButton(gameOver.transform, "RetryBtn", "RETRY CHECKPOINT [C]", new Vector2(0f, -35f), new Vector2(280f, 50f), new Color(0.25f, 0.5f, 0.55f));
            hud.gameOverMainMenuButton = CreateButton(gameOver.transform, "GameOverMenuBtn", "MAIN MENU [ESC]", new Vector2(0f, -95f), new Vector2(280f, 50f), new Color(0.55f, 0.2f, 0.2f));
            gameOver.SetActive(false);

            return canvasObj;
        }

        private static GameObject SpawnPrefab(string prefabName, Vector3 position, Quaternion rotation)
        {
            string path = PREFAB_DIR + "/" + prefabName + ".prefab";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning("Prefab not found: " + path);
                return null;
            }
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.transform.position = position;
            go.transform.rotation = rotation;
            return go;
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject cObj = new GameObject(name);
            Canvas canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler cs = cObj.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1280, 720);
            cs.matchWidthOrHeight = 0.5f;
            cs.dynamicPixelsPerUnit = 3.5f;

            cObj.AddComponent<GraphicRaycaster>();
            cObj.AddComponent<EnsureInputModule>();

            EnsureInputModule.FixEventSystem();

            return cObj;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
        {
            GameObject pObj = new GameObject(name);
            pObj.transform.SetParent(parent, false);
            Image img = pObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = pObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return pObj;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2 size, Color color, FontStyle style)
        {
            GameObject tObj = new GameObject(name);
            tObj.transform.SetParent(parent, false);
            Text t = tObj.AddComponent<Text>();
            t.text = content;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            Outline outline = tObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.02f, 0.02f, 0.03f, 0.95f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            RectTransform rt = tObj.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color buttonColor)
        {
            GameObject bObj = new GameObject(name);
            bObj.transform.SetParent(parent, false);
            Image img = bObj.AddComponent<Image>();
            img.color = buttonColor;
            img.raycastTarget = true;

            Button btn = bObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = true;

            ColorBlock cb = btn.colors;
            cb.normalColor = buttonColor;
            cb.highlightedColor = Color.Lerp(buttonColor, Color.white, 0.3f);
            cb.pressedColor = Color.Lerp(buttonColor, Color.black, 0.3f);
            cb.selectedColor = cb.highlightedColor;
            btn.colors = cb;

            RectTransform rt = bObj.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Text txt = CreateText(bObj.transform, "BtnText", label, 20, Vector2.zero, size, Color.white, FontStyle.Bold);
            txt.raycastTarget = false;

            return btn;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            GameObject sObj = new GameObject(name);
            sObj.transform.SetParent(parent, false);
            RectTransform rt = sObj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Slider slider = sObj.AddComponent<Slider>();

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f);
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sObj.transform, false);
            RectTransform faRT = fillArea.AddComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0f, 0.25f);
            faRT.anchorMax = new Vector2(1f, 0.75f);
            faRT.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.8f, 0.6f);
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.sizeDelta = Vector2.zero;
            slider.fillRect = fillRT;

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;

            return slider;
        }

        private static void ConfigureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(SCENE_DIR + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SCENE_DIR + "/LevelSelection.unity", true),
                new EditorBuildSettingsScene(SCENE_DIR + "/Instructions.unity", true),
                new EditorBuildSettingsScene(SCENE_DIR + "/CaveLevel1.unity", true),
                new EditorBuildSettingsScene(SCENE_DIR + "/CaveLevel2.unity", true),
                new EditorBuildSettingsScene(SCENE_DIR + "/CaveLevel3.unity", true),
                new EditorBuildSettingsScene(SCENE_DIR + "/GameComplete.unity", true)
            };

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Build Settings updated with all 7 scenes.");
        }
    }
}
