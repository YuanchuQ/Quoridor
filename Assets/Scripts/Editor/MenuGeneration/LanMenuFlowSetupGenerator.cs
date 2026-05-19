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
    /// Editor-only setup for the Canvas LAN lobby layout.
    /// </summary>
    public static class LanMenuFlowSetupGenerator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string CatalogPath = "Assets/Config/CharacterVisualCatalog.asset";
        private const string SelectedGlowMaterialPath = "Assets/Art/Materials/CharacterSelectedGlow.mat";

        private static readonly Color LobbyPanelColor = new(0.12f, 0.14f, 0.22f, 0.88f);
        private static readonly Color RoomCardColor = new(1f, 1f, 1f, 0.10f);
        private static readonly Color ButtonColor = new(0.30f, 0.55f, 1f, 1f);
        private static readonly Color ButtonHighlightColor = new(0.42f, 0.63f, 1f, 1f);
        private static readonly Color TextColor = new(0.96f, 0.96f, 1f, 1f);
        private static readonly Color MutedTextColor = new(0.78f, 0.80f, 0.90f, 1f);
        private static readonly Color CharacterCardColor = new(1f, 1f, 1f, 0.12f);

        /// <summary>
        /// Rebuilds the LAN lobby into a single Canvas page with room list, room info, and character select.
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
            Material selectedGlowMaterial = EnsureSelectedGlowMaterial();
            Canvas canvas = controller.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.transform.localScale = Vector3.one;
            }

            RebuildLobbyPanel(lanPanel, controller, catalog, selectedGlowMaterial, out LobbyReferences lobby);
            RebuildWaitingRoomPanel(roomPanel, out WaitingRoomReferences waitingRoom);
            Transform roomListPanel = EnsureLegacyRoomListPanel(root);

            WireController(controller, lanPanel.gameObject, roomListPanel.gameObject, roomPanel.gameObject, lobby, waitingRoom, catalog);

            roomListPanel.gameObject.SetActive(false);
            lanPanel.gameObject.SetActive(false);
            roomPanel.gameObject.SetActive(false);

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void RebuildLobbyPanel(
            Transform panel,
            MainMenuController controller,
            CharacterVisualCatalog catalog,
            Material selectedGlowMaterial,
            out LobbyReferences references)
        {
            ClearChildren(panel);
            SetStretch(panel.GetComponent<RectTransform>(), new Vector2(44f, 42f), new Vector2(-44f, -44f));
            Image panelImage = EnsureImage(panel, "Background", null);
            SetStretch(panelImage.rectTransform, Vector2.zero, Vector2.zero);
            panelImage.color = new Color(0.05f, 0.06f, 0.10f, 0.58f);

            Text title = EnsureText(panel, "PanelTitle", "局域网大厅", 42, FontStyle.Bold, TextAnchor.MiddleLeft, TextColor);
            SetAnchored(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -28f), new Vector2(420f, 56f));

            RectTransform topArea = CreateRect(panel, "TopArea");
            topArea.anchorMin = new Vector2(0f, 0.30f);
            topArea.anchorMax = new Vector2(1f, 1f);
            topArea.offsetMin = new Vector2(0f, 8f);
            topArea.offsetMax = new Vector2(0f, -84f);

            Transform roomList = CreateBox(topArea, "RoomList", LobbyPanelColor).transform;
            RectTransform roomListRect = roomList.GetComponent<RectTransform>();
            roomListRect.anchorMin = new Vector2(0f, 0f);
            roomListRect.anchorMax = new Vector2(0.75f, 1f);
            roomListRect.offsetMin = new Vector2(10f, 10f);
            roomListRect.offsetMax = new Vector2(-10f, -10f);

            Text roomListTitle = EnsureText(roomList, "Title", "当前房间", 30, FontStyle.Bold, TextAnchor.MiddleLeft, TextColor);
            SetAnchored(roomListTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -18f), new Vector2(-40f, 42f));

            Text roomListText = EnsureText(roomList, "RoomListText", "正在搜索局域网房间...", 18, FontStyle.Normal, TextAnchor.UpperLeft, MutedTextColor);
            SetAnchored(roomListText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -64f), new Vector2(-40f, 34f));

            Button firstRoomButton = EnsureButton(roomList, "FirstRoomButton", "搜索中...");
            SetAnchored(firstRoomButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -118f), new Vector2(-40f, 58f));
            SetButtonImage(firstRoomButton, RoomCardColor);
            Button secondRoomButton = EnsureButton(roomList, "SecondRoomButton", "继续搜索...");
            SetAnchored(secondRoomButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -188f), new Vector2(-40f, 58f));
            SetButtonImage(secondRoomButton, RoomCardColor);

            Transform roomInfo = CreateBox(topArea, "RoomInfo", LobbyPanelColor).transform;
            RectTransform roomInfoRect = roomInfo.GetComponent<RectTransform>();
            roomInfoRect.anchorMin = new Vector2(0.75f, 0f);
            roomInfoRect.anchorMax = new Vector2(1f, 1f);
            roomInfoRect.offsetMin = new Vector2(10f, 10f);
            roomInfoRect.offsetMax = new Vector2(-10f, -10f);

            Text roomInfoTitle = EnsureText(roomInfo, "Title", "房间信息", 30, FontStyle.Bold, TextAnchor.MiddleLeft, TextColor);
            SetAnchored(roomInfoTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -18f), new Vector2(-40f, 42f));

            Text roomInfoText = EnsureText(roomInfo, "RoomInfoText", "房主：-\n人数：- / -\n状态：搜索中", 22, FontStyle.Normal, TextAnchor.UpperLeft, MutedTextColor);
            SetAnchored(roomInfoText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20f, -86f), new Vector2(-40f, 170f));

            RectTransform bottomArea = CreateRect(panel, "BottomArea");
            bottomArea.anchorMin = new Vector2(0f, 0f);
            bottomArea.anchorMax = new Vector2(1f, 0.30f);
            bottomArea.offsetMin = new Vector2(10f, 12f);
            bottomArea.offsetMax = new Vector2(-10f, -8f);

            Transform buttonColumn = CreateRect(bottomArea, "BottomButtons").transform;
            RectTransform buttonColumnRect = buttonColumn.GetComponent<RectTransform>();
            buttonColumnRect.anchorMin = new Vector2(0f, 0f);
            buttonColumnRect.anchorMax = new Vector2(0f, 1f);
            buttonColumnRect.pivot = new Vector2(0f, 0.5f);
            buttonColumnRect.anchoredPosition = Vector2.zero;
            buttonColumnRect.sizeDelta = new Vector2(320f, 0f);

            Button createRoomButton = EnsureButton(buttonColumn, "CreateRoomButton", "创建房间");
            SetAnchored(createRoomButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(0f, 48f));
            Button joinRoomButton = EnsureButton(buttonColumn, "JoinRoomButton", "加入房间");
            SetAnchored(joinRoomButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 48f));
            Button backButton = EnsureButton(buttonColumn, "BackButton", "返回");
            SetAnchored(backButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 2f), new Vector2(0f, 48f));

            Text selectedCharacterText = EnsureText(bottomArea, "LanSelectedCharacterText", "角色：优衣", 20, FontStyle.Bold, TextAnchor.MiddleLeft, TextColor);
            SetAnchored(selectedCharacterText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(350f, -4f), new Vector2(260f, 30f));

            EnsureCharacterGrid(bottomArea, controller, catalog, selectedGlowMaterial);

            references = new LobbyReferences(
                selectedCharacterText,
                roomListText,
                roomInfoText,
                joinRoomButton,
                createRoomButton,
                backButton,
                firstRoomButton,
                secondRoomButton);
        }

        private static void RebuildWaitingRoomPanel(Transform roomPanel, out WaitingRoomReferences references)
        {
            ClearChildren(roomPanel);
            SetAnchored(roomPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(760f, 420f));
            Image background = roomPanel.GetComponent<Image>();
            if (background != null)
            {
                background.color = LobbyPanelColor;
            }

            Text roomWaitingText = EnsureText(roomPanel, "RoomWaitingText", "等待第二位玩家进入...", 24, FontStyle.Bold, TextAnchor.MiddleCenter, TextColor);
            SetAnchored(roomWaitingText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(540f, 84f));

            Button startButton = EnsureButton(roomPanel, "StartLocalButton", "开始");
            SetAnchored(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 40f), new Vector2(240f, 50f));

            Button roomBackButton = EnsureButton(roomPanel, "BackButton", "返回");
            SetAnchored(roomBackButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 40f), new Vector2(220f, 50f));

            references = new WaitingRoomReferences(roomWaitingText, roomBackButton, startButton);
        }

        private static void EnsureCharacterGrid(Transform parent, MainMenuController controller, CharacterVisualCatalog catalog, Material selectedGlowMaterial)
        {
            Transform grid = CreateRect(parent, "LanCharacterGrid").transform;
            RectTransform gridRect = grid.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0f, 0f);
            gridRect.anchorMax = new Vector2(1f, 1f);
            gridRect.offsetMin = new Vector2(350f, 8f);
            gridRect.offsetMax = new Vector2(-10f, -34f);

            CharacterVisualDefinition[] characters = catalog != null ? catalog.Characters : Array.Empty<CharacterVisualDefinition>();
            int visibleCount = Mathf.Min(characters.Length, 4);
            for (int index = 0; index < visibleCount; index++)
            {
                CharacterVisualDefinition character = characters[index];
                if (character == null)
                {
                    continue;
                }

                Transform card = CreateBox(grid, $"{character.CharacterId}Card", CharacterCardColor).transform;
                card.gameObject.AddComponent<RectMask2D>();
                Button button = card.gameObject.AddComponent<Button>();
                SetButtonImage(button, CharacterCardColor);
                RectTransform rect = card.GetComponent<RectTransform>();
                float normalizedMin = index / 4f;
                float normalizedMax = (index + 1) / 4f;
                rect.anchorMin = new Vector2(normalizedMin, 0f);
                rect.anchorMax = new Vector2(normalizedMax, 1f);
                rect.offsetMin = new Vector2(10f, 0f);
                rect.offsetMax = new Vector2(-10f, 0f);

                Image portrait = EnsureImage(card, "Portrait", character.PortraitSprite);
                SetAnchored(portrait.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(74f, 88f));

                Text label = EnsureText(card, "Label", character.DisplayName, 15, FontStyle.Bold, TextAnchor.MiddleCenter, TextColor);
                SetAnchored(label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(-12f, 26f));

                CharacterSelectButton selector = card.gameObject.AddComponent<CharacterSelectButton>();
                SerializedObject serializedSelector = new(selector);
                serializedSelector.FindProperty("menuController").objectReferenceValue = controller;
                serializedSelector.FindProperty("button").objectReferenceValue = button;
                serializedSelector.FindProperty("characterId").stringValue = character.CharacterId;
                serializedSelector.FindProperty("characterName").stringValue = character.DisplayName;
                serializedSelector.FindProperty("portraitImage").objectReferenceValue = portrait;
                serializedSelector.FindProperty("selectedMaterial").objectReferenceValue = selectedGlowMaterial;
                serializedSelector.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Transform EnsureLegacyRoomListPanel(Transform root)
        {
            Transform existing = root.Find("RoomListPanel");
            if (existing != null)
            {
                return existing;
            }

            GameObject panel = CreateBox(root, "RoomListPanel", LobbyPanelColor);
            SetAnchored(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 420f));
            return panel.transform;
        }

        private static Material EnsureSelectedGlowMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(SelectedGlowMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Quoridor/UI/Character Selected Glow");
            if (shader == null)
            {
                Debug.LogWarning("Character selected glow shader is unavailable.");
                return null;
            }

            material = new Material(shader)
            {
                name = "CharacterSelectedGlow"
            };
            material.SetColor("_GlowColor", new Color(1f, 0.82f, 0.22f, 1f));
            material.SetFloat("_GlowSize", 4f);
            material.SetFloat("_GlowStrength", 1.5f);
            AssetDatabase.CreateAsset(material, SelectedGlowMaterialPath);
            return material;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject rectObject = new(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            return rectObject.GetComponent<RectTransform>();
        }

        private static GameObject CreateBox(Transform parent, string name, Color color)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                UnityEngine.Object.DestroyImmediate(parent.GetChild(index).gameObject);
            }
        }

        private static Button EnsureButton(Transform parent, string name, string label)
        {
            GameObject buttonObject = CreateBox(parent, name, ButtonColor);
            Button button = buttonObject.AddComponent<Button>();
            SetButtonImage(button, ButtonColor);
            SetButtonLabel(button, label);
            return button;
        }

        private static void SetButtonImage(Button button, Color color)
        {
            Image image = button.GetComponent<Image>();
            button.targetGraphic = image;
            image.color = color;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = ButtonHighlightColor;
            colors.pressedColor = new Color(0.22f, 0.42f, 0.86f, 1f);
            colors.selectedColor = ButtonHighlightColor;
            button.colors = colors;
        }

        private static void SetButtonLabel(Button button, string value)
        {
            Text label = EnsureText(button.transform, "Label", value, 20, FontStyle.Bold, TextAnchor.MiddleCenter, TextColor);
            SetStretch(label.rectTransform, Vector2.zero, Vector2.zero);
        }

        private static Image EnsureImage(Transform parent, string name, Sprite sprite)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.08f);
            image.raycastTarget = false;
            return image;
        }

        private static Text EnsureText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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

        private static void SetStretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
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
            LobbyReferences lobby,
            WaitingRoomReferences waitingRoom,
            CharacterVisualCatalog catalog)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("lanPanel").objectReferenceValue = lanPanel;
            serializedController.FindProperty("roomListPanel").objectReferenceValue = roomListPanel;
            serializedController.FindProperty("roomPanel").objectReferenceValue = roomPanel;
            serializedController.FindProperty("lanSelectedCharacterText").objectReferenceValue = lobby.SelectedCharacterText;
            serializedController.FindProperty("roomListText").objectReferenceValue = lobby.RoomListText;
            serializedController.FindProperty("roomInfoText").objectReferenceValue = lobby.RoomInfoText;
            serializedController.FindProperty("roomWaitingText").objectReferenceValue = waitingRoom.WaitingText;
            serializedController.FindProperty("joinRoomButton").objectReferenceValue = lobby.JoinRoomButton;
            serializedController.FindProperty("createRoomButton").objectReferenceValue = lobby.CreateRoomButton;
            serializedController.FindProperty("lanBackButton").objectReferenceValue = lobby.BackButton;
            serializedController.FindProperty("roomListFirstRoomButton").objectReferenceValue = lobby.FirstRoomButton;
            serializedController.FindProperty("roomListSecondRoomButton").objectReferenceValue = lobby.SecondRoomButton;
            serializedController.FindProperty("roomListBackButton").objectReferenceValue = lobby.BackButton;
            serializedController.FindProperty("roomBackButton").objectReferenceValue = waitingRoom.BackButton;
            serializedController.FindProperty("startLocalFromRoomButton").objectReferenceValue = waitingRoom.StartButton;
            if (catalog != null)
            {
                serializedController.FindProperty("characterCatalog").objectReferenceValue = catalog;
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct LobbyReferences
        {
            public LobbyReferences(
                Text selectedCharacterText,
                Text roomListText,
                Text roomInfoText,
                Button joinRoomButton,
                Button createRoomButton,
                Button backButton,
                Button firstRoomButton,
                Button secondRoomButton)
            {
                SelectedCharacterText = selectedCharacterText;
                RoomListText = roomListText;
                RoomInfoText = roomInfoText;
                JoinRoomButton = joinRoomButton;
                CreateRoomButton = createRoomButton;
                BackButton = backButton;
                FirstRoomButton = firstRoomButton;
                SecondRoomButton = secondRoomButton;
            }

            public Text SelectedCharacterText { get; }
            public Text RoomListText { get; }
            public Text RoomInfoText { get; }
            public Button JoinRoomButton { get; }
            public Button CreateRoomButton { get; }
            public Button BackButton { get; }
            public Button FirstRoomButton { get; }
            public Button SecondRoomButton { get; }
        }

        private readonly struct WaitingRoomReferences
        {
            public WaitingRoomReferences(Text waitingText, Button backButton, Button startButton)
            {
                WaitingText = waitingText;
                BackButton = backButton;
                StartButton = startButton;
            }

            public Text WaitingText { get; }
            public Button BackButton { get; }
            public Button StartButton { get; }
        }
    }
}
