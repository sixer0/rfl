#nullable enable
using System;
using System.IO;
using System.Linq;
using RFL.Game.GreyBox;
using RFL.Game.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RFL.Editor
{
    /// <summary>
    /// Builds the generated parts of the project from code, so they can be rebuilt at any time:
    /// locales and the UI string table, PanelSettings, the Menu and GreyBox scenes, build scenes and
    /// player settings. Run from RFL/Setup or with -executeMethod RFL.Editor.ProjectSetup.Run.
    /// </summary>
    public static class ProjectSetup
    {
        public const string Root = "Assets/RFL";
        public const string LocDir = Root + "/Localization";
        public const string UiDir = Root + "/UI";
        public const string SceneDir = Root + "/Scenes";
        public const string MatDir = Root + "/Materials";
        public const string MenuScenePath = SceneDir + "/Menu.unity";
        public const string GreyBoxScenePath = SceneDir + "/GreyBox.unity";

        static readonly Color Background = new Color(0.055f, 0.071f, 0.086f);

        [MenuItem("RFL/Setup/Rebuild project setup")]
        public static void Run()
        {
            try
            {
                CleanTemplate();
                Folder(LocDir);
                Folder(LocDir + "/Locales");
                Folder(LocDir + "/Tables");
                Folder(SceneDir);
                Folder(MatDir);
                EnsureLocalization();
                StringTableImporter.Import();
                EnsurePanelSettings();
                AssetDatabase.SaveAssets();
                // NewScene unloads unused assets, so each scene loads its own fresh references.
                BuildMenuScene();
                BuildGreyBoxScene();
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MenuScenePath, true),
                    new EditorBuildSettingsScene(GreyBoxScenePath, true),
                };
                SetPlayerSettings();
                AssetDatabase.SaveAssets();
                Debug.Log("RFL project setup complete.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        static void CleanTemplate()
        {
            foreach (
                var path in new[] { "Assets/Readme.asset", "Assets/TutorialInfo", "Assets/Scenes" }
            )
            {
                if (AssetDatabase.IsValidFolder(path) || File.Exists(path))
                    AssetDatabase.DeleteAsset(path);
            }
        }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            Folder(parent!); // Assets always exists, so the recursion stops before a null parent
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void EnsureLocalization()
        {
            var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, LocDir + "/LocalizationSettings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }
            foreach (var code in StringTableImporter.Codes)
            {
                var id = new LocaleIdentifier(code);
                if (LocalizationEditorSettings.GetLocale(id) != null)
                    continue;
                var locale = Locale.CreateLocale(id);
                AssetDatabase.CreateAsset(locale, $"{LocDir}/Locales/{code}.asset");
                LocalizationEditorSettings.AddLocale(locale);
            }
            var selectors = settings.GetStartupLocaleSelectors();
            if (!selectors.OfType<SpecificLocaleSelector>().Any())
                selectors.Add(new SpecificLocaleSelector { LocaleId = new LocaleIdentifier("en") });
            EditorUtility.SetDirty(settings);
        }

        static PanelSettings EnsurePanelSettings()
        {
            const string path = PanelSettingsPath;
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, path);
            }
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                UiDir + "/RFLTheme.tss"
            );
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 1f;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        const string PanelSettingsPath = UiDir + "/RFLPanelSettings.asset";

        static PanelSettings LoadPanelSettings() =>
            AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath)
            ?? throw new InvalidOperationException("Missing " + PanelSettingsPath);

        static void SaveSceneChecked(Scene scene, string path, UIDocument doc)
        {
            if (doc.panelSettings == null || doc.visualTreeAsset == null)
                throw new InvalidOperationException(
                    $"{path}: UIDocument lost its PanelSettings or UXML."
                );
            EditorSceneManager.SaveScene(scene, path);
        }

        static VisualTreeAsset Uxml(string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UiDir}/{name}.uxml");
            if (asset == null)
                throw new InvalidOperationException($"Missing {UiDir}/{name}.uxml");
            return asset;
        }

        static Camera AddCamera(Vector3 position, Vector3 lookAt)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener))
            {
                tag = "MainCamera",
            };
            go.transform.position = position;
            go.transform.LookAt(lookAt);
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Background;
            cam.fieldOfView = 40f;
            return cam;
        }

        static void AddEventSystem() =>
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var panel = LoadPanelSettings();
            AddCamera(new Vector3(0, 1, -10), Vector3.zero);
            AddEventSystem();
            var ui = new GameObject("Menu UI");
            var doc = ui.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.visualTreeAsset = Uxml("Menu");
            var menu = ui.AddComponent<MenuController>();
            menu.TitleScreen = Uxml("Title");
            menu.MainMenu = Uxml("MainMenu");
            menu.Versus = Uxml("Versus");
            menu.Difficulty = Uxml("Difficulty");
            menu.RobotSelect = Uxml("RobotSelect");
            menu.Settings = Uxml("Settings");
            menu.Confirm = Uxml("Confirm");
            SaveSceneChecked(scene, MenuScenePath, doc);
        }

        static Material Mat(string name, Color color, Color? emission = null)
        {
            var path = $"{MatDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.35f);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static GameObject Prim(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 pos,
            Vector3 scale,
            Material mat
        )
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); // the fight never uses Unity physics
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static void BuildGreyBoxScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var panel = LoadPanelSettings();
            AddCamera(new Vector3(0f, 1.9f, -8.5f), new Vector3(0f, 1.2f, 0f));
            var light = new GameObject("Key Light", typeof(Light));
            light.GetComponent<Light>().type = LightType.Directional;
            light.GetComponent<Light>().intensity = 1.4f;
            light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.ambientLight = new Color(0.32f, 0.35f, 0.4f);
            AddEventSystem();

            var floor = Mat("GreyBox_Floor", new Color(0.22f, 0.24f, 0.27f));
            var post = Mat("GreyBox_Post", new Color(0.12f, 0.13f, 0.15f));
            var rope = Mat("GreyBox_Rope", new Color(0.78f, 0.2f, 0.18f));
            var p1 = Mat("GreyBox_P1", new Color(0.95f, 0.55f, 0.16f));
            var p2 = Mat("GreyBox_P2", new Color(0.18f, 0.77f, 0.71f));
            var visor = Mat("GreyBox_Visor", Color.black, new Color(2.2f, 2.2f, 2.4f));

            var ringPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RingPrefabPath);
            if (ringPrefab != null)
                PrefabUtility.InstantiatePrefab(ringPrefab, scene);
            else
                BuildPlaceholderRing(floor, post, rope);

            var robotP1 = Robot("P1 robot", -1.25f, 1f, p1, visor);
            var robotP2 = Robot("P2 robot", 1.25f, -1f, p2, visor);

            var ui = new GameObject("GreyBox UI");
            var doc = ui.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.visualTreeAsset = Uxml("GreyBox");
            var ctrl = ui.AddComponent<GreyBoxController>();
            ctrl.PauseTemplate = Uxml("Pause");
            ctrl.IntroTemplate = Uxml("Intro");
            ctrl.CornerTemplate = Uxml("CornerBreak");
            ctrl.ResultsTemplate = Uxml("Results");
            ctrl.P1Robot = robotP1;
            ctrl.P2Robot = robotP2;
            ctrl.RobotPrefabs = new[]
            {
                RobotPrefab("Standard"),
                RobotPrefab("Power"),
                RobotPrefab("Speed"),
            }; // StarterType order
            SaveSceneChecked(scene, GreyBoxScenePath, doc);
        }

        const string RingPrefabPath = Root + "/Art/Arena/LeagueRing/RFL_ARN_LeagueRing.prefab";

        static GameObject RobotPrefab(string type) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{Root}/Art/Robots/RFL_ROB_{type}/RFL_ROB_{type}.prefab"
            );

        /// <summary>The grey-box ring, used only when the league ring art is missing.</summary>
        static void BuildPlaceholderRing(Material floor, Material post, Material rope)
        {
            var ring = new GameObject("Ring (7 x 7 m inside the ropes)").transform;
            Prim(
                PrimitiveType.Cube,
                "Canvas",
                ring,
                new Vector3(0, -0.1f, 0),
                new Vector3(8f, 0.2f, 8f),
                floor
            );
            const float half = 3.5f;
            foreach (var x in new[] { -half, half })
            foreach (var z in new[] { -half, half })
                Prim(
                    PrimitiveType.Cylinder,
                    "Post",
                    ring,
                    new Vector3(x, 0.8f, z),
                    new Vector3(0.18f, 0.8f, 0.18f),
                    post
                );
            foreach (var y in new[] { 0.55f, 0.95f, 1.35f })
            {
                Prim(
                    PrimitiveType.Cube,
                    "Rope",
                    ring,
                    new Vector3(0, y, half),
                    new Vector3(2 * half, 0.04f, 0.04f),
                    rope
                );
                Prim(
                    PrimitiveType.Cube,
                    "Rope",
                    ring,
                    new Vector3(0, y, -half),
                    new Vector3(2 * half, 0.04f, 0.04f),
                    rope
                );
                Prim(
                    PrimitiveType.Cube,
                    "Rope",
                    ring,
                    new Vector3(half, y, 0),
                    new Vector3(0.04f, 0.04f, 2 * half),
                    rope
                );
                Prim(
                    PrimitiveType.Cube,
                    "Rope",
                    ring,
                    new Vector3(-half, y, 0),
                    new Vector3(0.04f, 0.04f, 2 * half),
                    rope
                );
            }
        }

        /// <summary>A capsule body with a visor bar on the side facing the opponent.</summary>
        static Transform Robot(string name, float x, float facing, Material body, Material visor)
        {
            var root = new GameObject(name).transform;
            root.position = new Vector3(x, 1.25f, 0f);
            var capsule = Prim(
                PrimitiveType.Capsule,
                "Body",
                root,
                Vector3.zero,
                Vector3.one,
                body
            );
            capsule.transform.localScale = Vector3.one;
            Prim(
                PrimitiveType.Cube,
                "Visor",
                root,
                new Vector3(0.42f * facing, 0.62f, 0f),
                new Vector3(0.18f, 0.09f, 0.55f),
                visor
            );
            return root;
        }

        static void SetPlayerSettings()
        {
            PlayerSettings.companyName = "Robot Fighting League";
            PlayerSettings.productName = "Robot Fighting League";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Standalone,
                "com.robotfightingleague.game"
            );
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
        }
    }
}
