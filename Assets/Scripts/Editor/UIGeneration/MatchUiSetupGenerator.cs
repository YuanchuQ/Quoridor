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
        private const int HudSortingOrder = 50;

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

            Text turnText = CreateText(hud.transform, "TurnText", new Vector2(24f, -22f), new Vector2(360f, 34f), 24, TextAnchor.MiddleLeft);
            Text modeText = CreateText(hud.transform, "ModeText", new Vector2(24f, -60f), new Vector2(260f, 28f), 18, TextAnchor.MiddleLeft);
            Text orientationText = CreateText(hud.transform, "OrientationText", new Vector2(24f, -92f), new Vector2(260f, 28f), 18, TextAnchor.MiddleLeft);
            Text wallText = CreateText(hud.transform, "WallText", new Vector2(-24f, -24f), new Vector2(390f, 30f), 18, TextAnchor.MiddleRight);
            GameObject panel = CreateGameOverPanel(hud.transform, out Text winnerText);

            var serializedView = new SerializedObject(hud.GetComponent<MatchUiView>());
            serializedView.FindProperty("turnText").objectReferenceValue = turnText;
            serializedView.FindProperty("modeText").objectReferenceValue = modeText;
            serializedView.FindProperty("wallText").objectReferenceValue = wallText;
            serializedView.FindProperty("orientationText").objectReferenceValue = orientationText;
            serializedView.FindProperty("gameOverPanel").objectReferenceValue = panel;
            serializedView.FindProperty("winnerText").objectReferenceValue = winnerText;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(hud, MatchHudPrefabPath);
            Object.DestroyImmediate(hud);
            return AssetDatabase.LoadAssetAtPath<GameObject>(MatchHudPrefabPath);
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

        private static GameObject CreateGameOverPanel(Transform parent, out Text winnerText)
        {
            GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(360f, 128f);

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.05f, 0.06f, 0.08f, 0.86f);

            winnerText = CreateText(panel.transform, "WinnerText", Vector2.zero, new Vector2(320f, 72f), 30, TextAnchor.MiddleCenter);
            RectTransform winnerRect = winnerText.GetComponent<RectTransform>();
            winnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            winnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            winnerRect.pivot = new Vector2(0.5f, 0.5f);
            winnerRect.anchoredPosition = Vector2.zero;
            winnerText.color = Color.white;
            panel.SetActive(false);
            return panel;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
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
