using System;
using Quoridor.Config;
using Quoridor.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Quoridor.Editor.MenuGeneration
{
    /// <summary>
    /// Incremental editor setup for LAN menu prototype panels in the existing main menu scene.
    /// </summary>
    public static class LanMenuFlowSetupGenerator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string CatalogPath = "Assets/Config/CharacterVisualCatalog.asset";
        private static readonly Color PanelColor = new(0.93f, 0.84f, 0.74f, 0.94f);
        private static readonly Color FieldColor = new(1f, 0.96f, 0.9f, 0.92f);
        private static readonly Color ButtonColor = new(0.72f, 0.46f, 0.52f, 1f);
        private static readonly Color ButtonHighlightColor = new(0.84f, 0.58f, 0.64f, 1f);
        private static readonly Color TextColor = new(0.18f, 0.12f, 0.13f, 1f);

        /// <summary>
        /// Adds or refreshes LAN character selection, room list, and waiting-room UI.
        /// </summary>
        [MenuItem("Tools/Quoridor/Generate LAN Menu Flow")]
        public static void GenerateLanMenuFlow()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            MainMenuController controller = UnityEngine.Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                Debug.LogError("MainMenuController not found in MainMenu scene.");
                return;
            }

            Transform root = controller.transform;
            Transform lanPanel = root.Find("LanPanel");
            Transform roomPanel = root.Find("RoomPanel");
            if (lanPanel == null || roomPanel == null)
            {
                Debug.LogError("LAN setup requires LanPanel and RoomPanel under MainMenuRoot.");
                return;
            }

            CharacterVisualCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterVisualCatalog>(CatalogPath);
            ResizePanel(lanPanel, new Vector2(760f, 500f));
            ResizePanel(roomPanel, new Vector2(820f, 500f));

            Text lanTitle = EnsureText(lanPanel, "PanelTitle", "局域网", 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetAnchored(lanTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(360f, 54f));

            RemoveChild(lanPanel, "NicknameLabel");
            RemoveChild(lanPanel, "NicknameInputField");

            Text selectedCharacterText = EnsureText(lanPanel, "LanSelectedCharacterText", "角色：优衣", 22, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetAnchored(selectedCharacterText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(260f, 34f));

            EnsureLanCharacterGrid(lanPanel, controller, catalog);

            Button joinRoomButton = EnsureButton(lanPanel, "JoinRoomButton", "加入房间");
            SetAnchored(joinRoomButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-160f, 86f), new Vector2(220f, 54f));
            Button createRoomButton = EnsureButton(lanPanel, "CreateRoomButton", "创建房间");
            SetAnchored(createRoomButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160f, 86f), new Vector2(220f, 54f));
            Button lanBackButton = EnsureButton(lanPanel, "BackButton", "返回");
            SetAnchored(lanBackButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(220f, 46f));

            Transform roomListPanel = EnsureRoomListPanel(root);
            Text roomListText = EnsureText(roomListPanel, "RoomListText", "Princess Room    1/2\nPractice Room    1/2", 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetAnchored(roomListText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(520f, 80f));
            Button firstRoomButton = EnsureButton(roomListPanel, "FirstRoomButton", "Princess Room    1/2");
            SetAnchored(firstRoomButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 52f), new Vector2(430f, 58f));
            Button secondRoomButton = EnsureButton(roomListPanel, "SecondRoomButton", "Practice Room    1/2");
            SetAnchored(secondRoomButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(430f, 58f));
            Button roomListBackButton = EnsureButton(roomListPanel, "BackButton", "返回");
            SetAnchored(roomListBackButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(220f, 48f));

            Text roomWaitingText = EnsureText(roomPanel, "RoomWaitingText", "等待第二位玩家进入...", 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetAnchored(roomWaitingText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -102f), new Vector2(520f, 80f));
            Button simulateButton = EnsureButton(roomPanel, "SimulateSecondPlayerButton", "模拟第二玩家进入");
            SetAnchored(simulateButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 108f), new Vector2(260f, 48f));

            Button startButton = FindButton(roomPanel, "StartLocalButton");
            if (startButton != null)
            {
                SetAnchored(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 40f), new Vector2(240f, 50f));
                SetButtonLabel(startButton, "开始");
            }

            Button roomBackButton = FindButton(roomPanel, "BackButton");
            if (roomBackButton != null)
            {
                SetAnchored(roomBackButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 40f), new Vector2(220f, 50f));
            }

            WireController(
                controller,
                lanPanel.gameObject,
                roomListPanel.gameObject,
                roomPanel.gameObject,
                selectedCharacterText,
                roomListText,
                roomWaitingText,
                joinRoomButton,
                createRoomButton,
                lanBackButton,
                firstRoomButton,
                secondRoomButton,
                roomListBackButton,
                roomBackButton,
                simulateButton,
                startButton,
                catalog);

            roomListPanel.gameObject.SetActive(false);
            lanPanel.gameObject.SetActive(false);
            roomPanel.gameObject.SetActive(false);

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void ResizePanel(Transform panel, Vector2 size)
        {
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = size;
            }
        }

        private static void RemoveChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void EnsureLanCharacterGrid(Transform parent, MainMenuController controller, CharacterVisualCatalog catalog)
        {
            Transform grid = parent.Find("LanCharacterGrid");
            if (grid == null)
            {
                grid = new GameObject("LanCharacterGrid", typeof(RectTransform)).transform;
                grid.SetParent(parent, false);
            }

            RectTransform gridRect = grid.GetComponent<RectTransform>();
            SetAnchored(gridRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(640f, 170f));

            CharacterVisualDefinition[] characters = catalog != null ? catalog.Characters : Array.Empty<CharacterVisualDefinition>();
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterVisualDefinition character = characters[i];
                if (character == null)
                {
                    continue;
                }

                Transform card = grid.Find($"{character.CharacterId}Card");
                if (card == null)
                {
                    card = CreatePanel(grid, $"{character.CharacterId}Card", FieldColor).transform;
                    Button button = card.gameObject.AddComponent<Button>();
                    CharacterSelectButton selector = card.gameObject.AddComponent<CharacterSelectButton>();
                    var serializedSelector = new SerializedObject(selector);
                    serializedSelector.FindProperty("menuController").objectReferenceValue = controller;
                    serializedSelector.FindProperty("button").objectReferenceValue = button;
                    serializedSelector.FindProperty("characterId").stringValue = character.CharacterId;
                    serializedSelector.FindProperty("characterName").stringValue = character.DisplayName;
                    serializedSelector.ApplyModifiedPropertiesWithoutUndo();
                }

                SetAnchored(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240f + i * 160f, 0f), new Vector2(132f, 156f));
                Image portrait = EnsureImage(card, "Portrait", character.PortraitSprite);
                SetAnchored(portrait.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(104f, 100f));
                Text label = EnsureText(card, "Label", character.DisplayName, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
                SetAnchored(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(120f, 26f));
            }
        }

        private static Transform EnsureRoomListPanel(Transform root)
        {
            Transform existing = root.Find("RoomListPanel");
            if (existing != null)
            {
                return existing;
            }

            GameObject panel = CreatePanel(root, "RoomListPanel", PanelColor);
            SetAnchored(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -82f), new Vector2(620f, 420f));
            Text title = EnsureText(panel.transform, "PanelTitle", "房间列表", 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(360f, 54f));
            return panel.transform;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Button EnsureButton(Transform parent, string name, string label)
        {
            Transform existing = parent.Find(name);
            GameObject buttonObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = ButtonColor;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = ButtonHighlightColor;
            colors.pressedColor = new Color(0.58f, 0.34f, 0.4f, 1f);
            colors.selectedColor = ButtonHighlightColor;
            button.colors = colors;
            SetButtonLabel(button, label);
            return button;
        }

        private static Button FindButton(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static void SetButtonLabel(Button button, string value)
        {
            Text label = EnsureText(button.transform, "Label", value, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetAnchored(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            label.color = Color.white;
        }

        private static Image EnsureImage(Transform parent, string name, Sprite sprite)
        {
            Transform existing = parent.Find(name);
            GameObject imageObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private static Text EnsureText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, TextAnchor alignment)
        {
            Transform existing = parent.Find(name);
            GameObject textObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = TextColor;
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
            GameObject lanPanel,
            GameObject roomListPanel,
            GameObject roomPanel,
            Text lanSelectedCharacterText,
            Text roomListText,
            Text roomWaitingText,
            Button joinRoomButton,
            Button createRoomButton,
            Button lanBackButton,
            Button firstRoomButton,
            Button secondRoomButton,
            Button roomListBackButton,
            Button roomBackButton,
            Button simulateButton,
            Button startButton,
            CharacterVisualCatalog catalog)
        {
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("lanPanel").objectReferenceValue = lanPanel;
            serializedController.FindProperty("roomListPanel").objectReferenceValue = roomListPanel;
            serializedController.FindProperty("roomPanel").objectReferenceValue = roomPanel;
            serializedController.FindProperty("lanSelectedCharacterText").objectReferenceValue = lanSelectedCharacterText;
            serializedController.FindProperty("roomListText").objectReferenceValue = roomListText;
            serializedController.FindProperty("roomWaitingText").objectReferenceValue = roomWaitingText;
            serializedController.FindProperty("joinRoomButton").objectReferenceValue = joinRoomButton;
            serializedController.FindProperty("createRoomButton").objectReferenceValue = createRoomButton;
            serializedController.FindProperty("lanBackButton").objectReferenceValue = lanBackButton;
            serializedController.FindProperty("roomListFirstRoomButton").objectReferenceValue = firstRoomButton;
            serializedController.FindProperty("roomListSecondRoomButton").objectReferenceValue = secondRoomButton;
            serializedController.FindProperty("roomListBackButton").objectReferenceValue = roomListBackButton;
            serializedController.FindProperty("roomBackButton").objectReferenceValue = roomBackButton;
            serializedController.FindProperty("simulateSecondPlayerButton").objectReferenceValue = simulateButton;
            serializedController.FindProperty("startLocalFromRoomButton").objectReferenceValue = startButton;
            if (catalog != null)
            {
                serializedController.FindProperty("characterCatalog").objectReferenceValue = catalog;
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
