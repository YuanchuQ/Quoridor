using Quoridor.Config;
using Quoridor.GameFlow;
using Quoridor.Input;
using Quoridor.Pawn;
using Quoridor.UI;
using Quoridor.Wall;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Quoridor.Editor.UIGeneration
{
    /// <summary>
    /// Edit-time generator for match HUD prefab and scene wiring.
    /// </summary>
    public static class MatchUiSetupGenerator
    {
        private const string MatchHudPrefabPath = "Assets/Prefabs/UI/MatchHUD.prefab";
        private const string PlayerInfoTexturePath = "Assets/Art/UI/Information.png";
        private const int HudSortingOrder = 50;
        private static readonly Color PlayerOneTextColor = new Color(0.68f, 0.24f, 0.32f, 1f);
        private static readonly Color PlayerTwoTextColor = new Color(0.2f, 0.34f, 0.68f, 1f);

        /// <summary>
        /// Creates or updates the match HUD prefab, scene instance, and game flow wiring.
        /// </summary>
        [MenuItem("Tools/Quoridor/Generate Match UI Setup")]
        public static void GenerateMatchUiSetup()
        {
            EnsureFolders();
            GameObject prefab = EnsureMatchHudPrefab();
            WireScene(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Prefabs", "UI");
            EnsureFolder("Assets/Scripts/Editor", "UIGeneration");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static GameObject EnsureMatchHudPrefab()
        {
            GameObject hud = new GameObject("MatchHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MatchUiView));
            Canvas canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = HudSortingOrder;

            CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            CreatePlayerInfoPanel(
                hud.transform,
                "PlayerOneInfoPanel",
                true,
                PlayerOneTextColor,
                out Text playerOneNameText,
                out Text playerOnePawnCountText,
                out Text playerOneWallCountText,
                out Text playerOneStatusText);

            CreatePlayerInfoPanel(
                hud.transform,
                "PlayerTwoInfoPanel",
                false,
                PlayerTwoTextColor,
                out Text playerTwoNameText,
                out Text playerTwoPawnCountText,
                out Text playerTwoWallCountText,
                out Text playerTwoStatusText);

            Text turnText = CreateText(hud.transform, "TurnText", new Vector2(0f, -18f), new Vector2(420f, 34f), 22, TextAnchor.MiddleCenter);
            SetAnchored(turnText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), turnText.rectTransform.anchoredPosition, turnText.rectTransform.sizeDelta);

            Text modeText = CreateText(hud.transform, "ModeText", new Vector2(-224f, 34f), new Vector2(190f, 26f), 16, TextAnchor.MiddleLeft);
            Text orientationText = CreateText(hud.transform, "OrientationText", new Vector2(224f, 34f), new Vector2(190f, 26f), 16, TextAnchor.MiddleRight);
            Text hintText = CreateText(hud.transform, "HintText", new Vector2(0f, 22f), new Vector2(620f, 42f), 16, TextAnchor.MiddleCenter);
            Text wallText = CreateText(hud.transform, "WallText", new Vector2(0f, 58f), new Vector2(390f, 24f), 16, TextAnchor.MiddleCenter);

            SetBottomAnchored(modeText.rectTransform);
            SetBottomAnchored(orientationText.rectTransform);
            SetBottomAnchored(hintText.rectTransform);
            SetBottomAnchored(wallText.rectTransform);

            GameObject panel = CreateGameOverPanel(hud.transform, out Text winnerText, out Button restartButton);

            var serializedView = new SerializedObject(hud.GetComponent<MatchUiView>());
            serializedView.FindProperty("turnText").objectReferenceValue = turnText;
            serializedView.FindProperty("modeText").objectReferenceValue = modeText;
            serializedView.FindProperty("hintText").objectReferenceValue = hintText;
            serializedView.FindProperty("wallText").objectReferenceValue = wallText;
            serializedView.FindProperty("orientationText").objectReferenceValue = orientationText;
            serializedView.FindProperty("playerOneNameText").objectReferenceValue = playerOneNameText;
            serializedView.FindProperty("playerOnePawnCountText").objectReferenceValue = playerOnePawnCountText;
            serializedView.FindProperty("playerOneWallCountText").objectReferenceValue = playerOneWallCountText;
            serializedView.FindProperty("playerOneStatusText").objectReferenceValue = playerOneStatusText;
            serializedView.FindProperty("playerTwoNameText").objectReferenceValue = playerTwoNameText;
            serializedView.FindProperty("playerTwoPawnCountText").objectReferenceValue = playerTwoPawnCountText;
            serializedView.FindProperty("playerTwoWallCountText").objectReferenceValue = playerTwoWallCountText;
            serializedView.FindProperty("playerTwoStatusText").objectReferenceValue = playerTwoStatusText;
            serializedView.FindProperty("gameOverPanel").objectReferenceValue = panel;
            serializedView.FindProperty("winnerText").objectReferenceValue = winnerText;
            serializedView.FindProperty("restartButton").objectReferenceValue = restartButton;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(hud, MatchHudPrefabPath);
            Object.DestroyImmediate(hud);
            return AssetDatabase.LoadAssetAtPath<GameObject>(MatchHudPrefabPath);
        }

        private static void CreatePlayerInfoPanel(
            Transform parent,
            string name,
            bool isLeftSide,
            Color textColor,
            out Text nameText,
            out Text pawnCountText,
            out Text wallCountText,
            out Text statusText)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Vector2 horizontalAnchor = isLeftSide ? Vector2.zero : Vector2.one;
            panelRect.anchorMin = new Vector2(horizontalAnchor.x, 0.5f);
            panelRect.anchorMax = new Vector2(horizontalAnchor.x, 0.5f);
            panelRect.pivot = new Vector2(horizontalAnchor.x, 0.5f);
            panelRect.anchoredPosition = new Vector2(isLeftSide ? 12f : -12f, -18f);
            panelRect.sizeDelta = new Vector2(238f, 424f);

            RawImage frameImage = CreatePanelFrame(panel.transform, isLeftSide);
            RectTransform frameRect = frameImage.rectTransform;
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.sizeDelta = Vector2.zero;

            nameText = CreatePanelText(panel.transform, "NameText", new Vector2(46.7f, -170.4f), new Vector2(150f, 34f), 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Text nameSubText = CreatePanelText(panel.transform, "NameSubText", new Vector2(51.7f, -176.4f), new Vector2(140f, 22f), 12, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            nameSubText.text = isLeftSide ? "YUI" : "KARU";

            Text pawnLabel = CreatePanelText(panel.transform, "PawnLabel", new Vector2(30f, -235.9f), new Vector2(120f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleCenter, textColor);
            pawnLabel.text = "棋子";
            pawnCountText = CreatePanelText(panel.transform, "PawnCountText", new Vector2(106.7f, -230.9f), new Vector2(90f, 34f), 22, FontStyle.Bold, TextAnchor.MiddleLeft, textColor);

            Text wallLabel = CreatePanelText(panel.transform, "WallLabel", new Vector2(30f, -259.90002f), new Vector2(120f, 24f), 18, FontStyle.Bold, TextAnchor.MiddleCenter, textColor);
            wallLabel.text = "墙壁";
            wallCountText = CreatePanelText(panel.transform, "WallCountText", new Vector2(106.7f, -254.90002f), new Vector2(90f, 34f), 22, FontStyle.Bold, TextAnchor.MiddleLeft, textColor);

            statusText = CreatePanelText(panel.transform, "StatusText", new Vector2(46.7f, -320.8f), new Vector2(150f, 34f), 20, FontStyle.Bold, TextAnchor.MiddleCenter, textColor);
        }

        private static RawImage CreatePanelFrame(Transform parent, bool isLeftSide)
        {
            GameObject frameObject = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            frameObject.transform.SetParent(parent, false);
            RawImage image = frameObject.GetComponent<RawImage>();
            image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PlayerInfoTexturePath);
            image.uvRect = isLeftSide ? new Rect(0f, 0f, 0.5f, 1f) : new Rect(0.5f, 0f, 0.5f, 1f);
            image.raycastTarget = false;
            return image;
        }

        private static Text CreatePanelText(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color)
        {
            Text text = CreateText(parent, name, anchoredPosition, size, fontSize, alignment);
            text.fontStyle = fontStyle;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();

            bool alignRight = alignment == TextAnchor.MiddleRight;
            rectTransform.anchorMin = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rectTransform.anchorMax = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rectTransform.pivot = alignRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.92f, 0.96f, 1f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void SetBottomAnchored(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
        }

        private static void SetAnchored(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }

        private static GameObject CreateGameOverPanel(Transform parent, out Text winnerText, out Button restartButton)
        {
            GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(360f, 156f);

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.05f, 0.06f, 0.08f, 0.86f);

            winnerText = CreateText(panel.transform, "WinnerText", Vector2.zero, new Vector2(320f, 72f), 30, TextAnchor.MiddleCenter);
            RectTransform winnerRect = winnerText.GetComponent<RectTransform>();
            winnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            winnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            winnerRect.pivot = new Vector2(0.5f, 0.5f);
            winnerRect.anchoredPosition = new Vector2(0f, 24f);
            winnerText.color = Color.white;

            restartButton = CreateRestartButton(panel.transform);
            panel.SetActive(false);
            return panel;
        }

        private static Button CreateRestartButton(Transform parent)
        {
            GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, -44f);
            rectTransform.sizeDelta = new Vector2(140f, 34f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.42f, 0.78f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.24f, 0.52f, 0.92f, 1f);
            colors.pressedColor = new Color(0.12f, 0.28f, 0.56f, 1f);
            button.colors = colors;

            Text label = CreateText(buttonObject.transform, "Label", Vector2.zero, new Vector2(128f, 28f), 18, TextAnchor.MiddleCenter);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            label.text = "Restart";
            label.color = Color.white;
            return button;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        private static void WireScene(GameObject prefab)
        {
            GameObject sceneRoot = GameObject.Find("SceneRoot");
            GameObject systems = GameObject.Find("SceneRoot/Systems");
            if (sceneRoot == null || systems == null)
            {
                Debug.LogError("Match UI setup requires SceneRoot and SceneRoot/Systems.");
                return;
            }

            GameObject hudObject = EnsureHudInstance(prefab, sceneRoot.transform);
            MatchUiView matchUiView = hudObject.GetComponent<MatchUiView>();
            GameFlowController gameFlowController = systems.GetComponent<GameFlowController>();
            if (gameFlowController == null)
            {
                gameFlowController = systems.AddComponent<GameFlowController>();
            }

            var serializedController = new SerializedObject(gameFlowController);
            serializedController.FindProperty("config").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
            serializedController.FindProperty("inputRouter").objectReferenceValue = systems.GetComponent<InputRouter>();
            serializedController.FindProperty("pawnController").objectReferenceValue = systems.GetComponent<PawnController>();
            serializedController.FindProperty("wallController").objectReferenceValue = systems.GetComponent<WallController>();
            serializedController.FindProperty("matchUiView").objectReferenceValue = matchUiView;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(gameFlowController);
            EditorUtility.SetDirty(matchUiView);
            EditorSceneManager.MarkSceneDirty(sceneRoot.scene);
            EditorSceneManager.SaveScene(sceneRoot.scene);
        }

        private static GameObject EnsureHudInstance(GameObject prefab, Transform sceneRoot)
        {
            Transform existing = sceneRoot.Find("MatchHUD");
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject hudObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, sceneRoot);
            hudObject.name = "MatchHUD";
            return hudObject;
        }
    }
}
