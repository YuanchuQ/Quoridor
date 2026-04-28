using System;
using System.Collections.Generic;
using Quoridor.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Quoridor.Editor.MenuGeneration
{
    /// <summary>
    /// Edit-time generator for the Canvas-based main menu scene.
    /// </summary>
    public static class MainMenuSceneGenerator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string MatchScenePath = "Assets/Scenes/QuoridorDemo.unity";
        private const string CharacterFolder = "Assets/Art/Character";
        private const string RootName = "MainMenuRoot";
        private static readonly Color BackgroundColor = new Color(0.08f, 0.1f, 0.11f, 1f);
        private static readonly Color PanelColor = new Color(0.93f, 0.84f, 0.74f, 0.94f);
        private static readonly Color ButtonColor = new Color(0.72f, 0.46f, 0.52f, 1f);
        private static readonly Color ButtonHighlightColor = new Color(0.84f, 0.58f, 0.64f, 1f);
        private static readonly Color TextColor = new Color(0.18f, 0.12f, 0.13f, 1f);
        private static readonly Color LightTextColor = new Color(0.96f, 0.89f, 0.82f, 1f);

        /// <summary>
        /// Creates or replaces the main menu scene and updates build settings.
        /// </summary>
        [MenuItem("Tools/Quoridor/Generate Main Menu Scene")]
        public static void GenerateMainMenuScene()
        {
            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets/Scripts/Editor", "MenuGeneration");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();
            MainMenuController controller = CreateCanvas();

            if (controller == null)
            {
                throw new InvalidOperationException("Main menu controller was not created.");
            }

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated main menu scene at {MainMenuScenePath}.");
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static MainMenuController CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject root = CreatePanel(canvasObject.transform, RootName, Vector2.zero, Vector2.zero, Vector2.one, Color.clear);
            MainMenuController controller = root.AddComponent<MainMenuController>();

            CreateBackground(root.transform);
            Text titleText = CreateText(root.transform, "TitleText", "Quoridor", 84, FontStyle.Bold, TextAnchor.MiddleCenter, LightTextColor);
            SetAnchored(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(760f, 112f));

            Text statusText = CreateText(root.transform, "StatusText", string.Empty, 22, FontStyle.Normal, TextAnchor.MiddleCenter, LightTextColor);
            SetAnchored(statusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(760f, 38f));

            GameObject mainPanel = CreateMainPanel(root.transform, out Button singlePlayerButton, out Button twoPlayerButton, out Button settingsButton, out Button quitButton);
            GameObject twoPlayerPanel = CreateTwoPlayerPanel(root.transform, out Button lanButton, out Button localButton, out Button twoPlayerBackButton);
            GameObject lanPanel = CreateLanPanel(root.transform, out Text roomListText, out Button joinRoomButton, out Button createRoomButton, out Button lanBackButton);
            GameObject roomPanel = CreateRoomPanel(root.transform, controller, out Text roomTitleText, out Text selectedCharacterText, out Button startLocalFromRoomButton, out Button roomBackButton);
            GameObject settingsPanel = CreateSettingsPanel(root.transform, out Button settingsBackButton);

            WireController(
                controller,
                mainPanel,
                twoPlayerPanel,
                lanPanel,
                roomPanel,
                settingsPanel,
                statusText,
                roomListText,
                roomTitleText,
                selectedCharacterText,
                singlePlayerButton,
                twoPlayerButton,
                settingsButton,
                quitButton,
                lanButton,
                localButton,
                twoPlayerBackButton,
                joinRoomButton,
                createRoomButton,
                lanBackButton,
                roomBackButton,
                startLocalFromRoomButton,
                settingsBackButton);

            return controller;
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject background = CreatePanel(parent, "Background", Vector2.zero, Vector2.zero, Vector2.one, BackgroundColor);
            background.transform.SetAsFirstSibling();
        }

        private static GameObject CreateMainPanel(
            Transform parent,
            out Button singlePlayerButton,
            out Button twoPlayerButton,
            out Button settingsButton,
            out Button quitButton)
        {
            GameObject panel = CreateMenuPanel(parent, "MainPanel", new Vector2(0f, -82f), new Vector2(420f, 360f));
            singlePlayerButton = CreateButton(panel.transform, "SinglePlayerButton", "单人游戏", new Vector2(0f, 102f), new Vector2(320f, 58f));
            twoPlayerButton = CreateButton(panel.transform, "TwoPlayerButton", "双人游戏", new Vector2(0f, 32f), new Vector2(320f, 58f));
            settingsButton = CreateButton(panel.transform, "SettingsButton", "设置", new Vector2(0f, -38f), new Vector2(320f, 58f));
            quitButton = CreateButton(panel.transform, "QuitButton", "退出", new Vector2(0f, -108f), new Vector2(320f, 58f));
            return panel;
        }

        private static GameObject CreateTwoPlayerPanel(
            Transform parent,
            out Button lanButton,
            out Button localButton,
            out Button backButton)
        {
            GameObject panel = CreateMenuPanel(parent, "TwoPlayerPanel", new Vector2(0f, -82f), new Vector2(460f, 330f));
            CreatePanelTitle(panel.transform, "双人游戏");
            lanButton = CreateButton(panel.transform, "LanButton", "局域网", new Vector2(0f, 46f), new Vector2(320f, 58f));
            localButton = CreateButton(panel.transform, "LocalButton", "本地", new Vector2(0f, -24f), new Vector2(320f, 58f));
            backButton = CreateButton(panel.transform, "BackButton", "返回", new Vector2(0f, -106f), new Vector2(220f, 48f));
            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateLanPanel(
            Transform parent,
            out Text roomListText,
            out Button joinButton,
            out Button createButton,
            out Button backButton)
        {
            GameObject panel = CreateMenuPanel(parent, "LanPanel", new Vector2(0f, -82f), new Vector2(620f, 420f));
            CreatePanelTitle(panel.transform, "当前房间");
            roomListText = CreateText(panel.transform, "RoomListText", "Local Preview Room    1/2", 24, FontStyle.Normal, TextAnchor.MiddleCenter, TextColor);
            SetAnchored(roomListText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(520f, 86f));
            joinButton = CreateButton(panel.transform, "JoinRoomButton", "加入房间", new Vector2(-120f, -48f), new Vector2(220f, 54f));
            createButton = CreateButton(panel.transform, "CreateRoomButton", "创建房间", new Vector2(120f, -48f), new Vector2(220f, 54f));
            backButton = CreateButton(panel.transform, "BackButton", "返回", new Vector2(0f, -132f), new Vector2(220f, 48f));
            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateRoomPanel(
            Transform parent,
            MainMenuController controller,
            out Text roomTitleText,
            out Text selectedCharacterText,
            out Button startButton,
            out Button backButton)
        {
            GameObject panel = CreateMenuPanel(parent, "RoomPanel", new Vector2(0f, -82f), new Vector2(820f, 480f));
            roomTitleText = CreateText(panel.transform, "RoomTitleText", "Room", 34, FontStyle.Bold, TextAnchor.MiddleCenter, TextColor);
            SetAnchored(roomTitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(680f, 50f));

            selectedCharacterText = CreateText(panel.transform, "SelectedCharacterText", "Selected: Not selected", 22, FontStyle.Normal, TextAnchor.MiddleCenter, TextColor);
            SetAnchored(selectedCharacterText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(680f, 34f));

            CreateCharacterGrid(panel.transform, controller);
            startButton = CreateButton(panel.transform, "StartLocalButton", "开始本地对局", new Vector2(132f, -186f), new Vector2(240f, 50f));
            backButton = CreateButton(panel.transform, "BackButton", "返回", new Vector2(-132f, -186f), new Vector2(220f, 50f));
            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateSettingsPanel(Transform parent, out Button backButton)
        {
            GameObject panel = CreateMenuPanel(parent, "SettingsPanel", new Vector2(0f, -82f), new Vector2(520f, 300f));
            CreatePanelTitle(panel.transform, "设置");
            Text placeholder = CreateText(panel.transform, "SettingsPlaceholderText", "设置项预留", 26, FontStyle.Normal, TextAnchor.MiddleCenter, TextColor);
            SetAnchored(placeholder.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(360f, 48f));
            backButton = CreateButton(panel.transform, "BackButton", "返回", new Vector2(0f, -84f), new Vector2(220f, 48f));
            panel.SetActive(false);
            return panel;
        }

        private static void CreateCharacterGrid(Transform parent, MainMenuController controller)
        {
            IReadOnlyList<CharacterEntry> characters = LoadCharacters();
            float startX = -270f;
            for (int i = 0; i < characters.Count; i++)
            {
                CharacterEntry character = characters[i];
                GameObject card = CreatePanel(parent, $"{character.DisplayName}Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 0.96f, 0.9f, 0.85f));
                SetAnchored(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + i * 180f, 30f), new Vector2(148f, 210f));

                Button button = card.AddComponent<Button>();
                ColorBlock colors = button.colors;
                colors.highlightedColor = new Color(1f, 0.88f, 0.78f, 1f);
                colors.pressedColor = new Color(0.86f, 0.68f, 0.66f, 1f);
                button.colors = colors;

                Image portrait = CreateImage(card.transform, "Portrait", character.Sprite, true);
                SetAnchored(portrait.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(118f, 132f));

                Text label = CreateText(card.transform, "Label", character.DisplayName, 18, FontStyle.Bold, TextAnchor.MiddleCenter, TextColor);
                SetAnchored(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(132f, 32f));

                CharacterSelectButton selector = card.AddComponent<CharacterSelectButton>();
                var serializedSelector = new SerializedObject(selector);
                serializedSelector.FindProperty("menuController").objectReferenceValue = controller;
                serializedSelector.FindProperty("button").objectReferenceValue = button;
                serializedSelector.FindProperty("characterName").stringValue = character.DisplayName;
                serializedSelector.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static IReadOnlyList<CharacterEntry> LoadCharacters()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { CharacterFolder });
            Array.Sort(guids, StringComparer.Ordinal);
            var characters = new List<CharacterEntry>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = LoadNamedZeroSprite(path);
                if (sprite == null)
                {
                    continue;
                }

                characters.Add(new CharacterEntry(FormatCharacterName(sprite.name), sprite));
            }

            return characters;
        }

        private static Sprite LoadNamedZeroSprite(string path)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite fallback = null;
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is not Sprite sprite)
                {
                    continue;
                }

                if (fallback == null || sprite.rect.width * sprite.rect.height > fallback.rect.width * fallback.rect.height)
                {
                    fallback = sprite;
                }

                if (sprite.name.EndsWith("_0", StringComparison.Ordinal))
                {
                    return sprite;
                }
            }

            return fallback;
        }

        private static string FormatCharacterName(string spriteName)
        {
            string trimmed = spriteName;
            if (trimmed.EndsWith("_0", StringComparison.Ordinal))
            {
                trimmed = trimmed[..^2];
            }

            trimmed = trimmed.Replace("1200px-", string.Empty, StringComparison.Ordinal);
            trimmed = trimmed.Replace("Q", string.Empty, StringComparison.Ordinal);
            return trimmed;
        }

        private static GameObject CreateMenuPanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject panel = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), PanelColor);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            return panel;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static void CreatePanelTitle(Transform parent, string text)
        {
            Text title = CreateText(parent, "PanelTitle", text, 36, FontStyle.Bold, TextAnchor.MiddleCenter, TextColor);
            SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(360f, 54f));
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetAnchored(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

            Image image = buttonObject.GetComponent<Image>();
            image.color = ButtonColor;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = ButtonHighlightColor;
            colors.pressedColor = new Color(0.58f, 0.34f, 0.4f, 1f);
            colors.selectedColor = ButtonHighlightColor;
            button.colors = colors;

            Text text = CreateText(buttonObject.transform, "Label", label, 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            RectTransform labelRect = text.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            return button;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, bool preserveAspect)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.color = Color.white;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void SetAnchored(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        private static void WireController(
            MainMenuController controller,
            GameObject mainPanel,
            GameObject twoPlayerPanel,
            GameObject lanPanel,
            GameObject roomPanel,
            GameObject settingsPanel,
            Text statusText,
            Text roomListText,
            Text roomTitleText,
            Text selectedCharacterText,
            Button singlePlayerButton,
            Button twoPlayerButton,
            Button settingsButton,
            Button quitButton,
            Button lanButton,
            Button localButton,
            Button twoPlayerBackButton,
            Button joinRoomButton,
            Button createRoomButton,
            Button lanBackButton,
            Button roomBackButton,
            Button startLocalFromRoomButton,
            Button settingsBackButton)
        {
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("localGameSceneName").stringValue = "QuoridorDemo";
            serializedController.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            serializedController.FindProperty("twoPlayerPanel").objectReferenceValue = twoPlayerPanel;
            serializedController.FindProperty("lanPanel").objectReferenceValue = lanPanel;
            serializedController.FindProperty("roomPanel").objectReferenceValue = roomPanel;
            serializedController.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            serializedController.FindProperty("statusText").objectReferenceValue = statusText;
            serializedController.FindProperty("roomListText").objectReferenceValue = roomListText;
            serializedController.FindProperty("roomTitleText").objectReferenceValue = roomTitleText;
            serializedController.FindProperty("selectedCharacterText").objectReferenceValue = selectedCharacterText;
            serializedController.FindProperty("singlePlayerButton").objectReferenceValue = singlePlayerButton;
            serializedController.FindProperty("twoPlayerButton").objectReferenceValue = twoPlayerButton;
            serializedController.FindProperty("settingsButton").objectReferenceValue = settingsButton;
            serializedController.FindProperty("quitButton").objectReferenceValue = quitButton;
            serializedController.FindProperty("lanButton").objectReferenceValue = lanButton;
            serializedController.FindProperty("localButton").objectReferenceValue = localButton;
            serializedController.FindProperty("twoPlayerBackButton").objectReferenceValue = twoPlayerBackButton;
            serializedController.FindProperty("joinRoomButton").objectReferenceValue = joinRoomButton;
            serializedController.FindProperty("createRoomButton").objectReferenceValue = createRoomButton;
            serializedController.FindProperty("lanBackButton").objectReferenceValue = lanBackButton;
            serializedController.FindProperty("roomBackButton").objectReferenceValue = roomBackButton;
            serializedController.FindProperty("startLocalFromRoomButton").objectReferenceValue = startLocalFromRoomButton;
            serializedController.FindProperty("settingsBackButton").objectReferenceValue = settingsBackButton;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(MatchScenePath, true)
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == MainMenuScenePath || scene.path == MatchScenePath)
                {
                    continue;
                }

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private readonly struct CharacterEntry
        {
            public CharacterEntry(string displayName, Sprite sprite)
            {
                DisplayName = displayName;
                Sprite = sprite;
            }

            public string DisplayName { get; }
            public Sprite Sprite { get; }
        }
    }
}
