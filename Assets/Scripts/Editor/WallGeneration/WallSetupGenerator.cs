using Quoridor.Board;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Input;
using Quoridor.Pawn;
using Quoridor.Wall;
using Quoridor.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Quoridor.Editor.WallGeneration
{
    /// <summary>
    /// Edit-time generator for wall prefab and scene wiring.
    /// </summary>
    public static class WallSetupGenerator
    {
        private const string WallPrefabPath = "Assets/Prefabs/Wall/Wall.prefab";
        private const int WallSortingOrder = 15;

        /// <summary>
        /// Creates or updates the wall prefab, preview instance and wall controller wiring.
        /// </summary>
        [MenuItem("Tools/Quoridor/Generate Wall Setup")]
        public static void GenerateWallSetup()
        {
            EnsureFolders();
            Sprite wallSprite = EnsureWallSprite();
            GameObject prefab = EnsureWallPrefab(wallSprite);
            WireScene(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Prefabs", "Wall");
            EnsureFolder("Assets/Scripts/Editor", "WallGeneration");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Sprite EnsureWallSprite()
        {
            return QuoridorArtAssetUtility.EnsureWallSprite();
        }

        private static GameObject EnsureWallPrefab(Sprite wallSprite)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallPrefabPath);
            GameObject wallObject = existingPrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(existingPrefab)
                : new GameObject("Wall");

            SpriteRenderer spriteRenderer = wallObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = wallObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = wallSprite;
            spriteRenderer.sortingOrder = WallSortingOrder;
            spriteRenderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPlaced.mat");

            BoxCollider2D collider = wallObject.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = wallObject.AddComponent<BoxCollider2D>();
            }

            collider.size = Vector2.one;

            if (wallObject.GetComponent<WallView>() == null)
            {
                wallObject.AddComponent<WallView>();
            }

            var serializedWall = new SerializedObject(wallObject.GetComponent<WallView>());
            serializedWall.FindProperty("spriteRenderer").objectReferenceValue = spriteRenderer;
            serializedWall.FindProperty("validPreviewMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPreviewValid.mat");
            serializedWall.FindProperty("invalidPreviewMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPreviewInvalid.mat");
            serializedWall.FindProperty("placedMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPlaced.mat");
            serializedWall.FindProperty("sortingOrder").intValue = WallSortingOrder;
            serializedWall.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(wallObject, WallPrefabPath);
            Object.DestroyImmediate(wallObject);
            return AssetDatabase.LoadAssetAtPath<GameObject>(WallPrefabPath);
        }

        private static void WireScene(GameObject wallPrefab)
        {
            GameObject sceneRoot = GameObject.Find("SceneRoot");
            GameObject systems = GameObject.Find("SceneRoot/Systems");
            GameObject boardRoot = GameObject.Find("SceneRoot/BoardRoot");

            if (sceneRoot == null || systems == null || boardRoot == null)
            {
                Debug.LogError("Wall setup requires SceneRoot, SceneRoot/Systems and SceneRoot/BoardRoot.");
                return;
            }

            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
            BoardView boardView = boardRoot.GetComponent<BoardView>();
            InputRouter inputRouter = systems.GetComponent<InputRouter>();
            PawnController pawnController = systems.GetComponent<PawnController>();

            if (config == null || boardView == null || inputRouter == null || pawnController == null)
            {
                Debug.LogError("Wall setup requires GameConfig, BoardView, InputRouter and PawnController.");
                return;
            }

            Transform wallsRoot = EnsureChild(sceneRoot.transform, "Walls");
            Transform placedWallsRoot = EnsureChild(wallsRoot, "PlacedWalls");
            WallView previewWall = EnsurePreviewWall(wallPrefab, wallsRoot, boardView, config);
            WallController wallController = systems.GetComponent<WallController>();
            if (wallController == null)
            {
                wallController = systems.AddComponent<WallController>();
            }

            var serializedController = new SerializedObject(wallController);
            serializedController.FindProperty("config").objectReferenceValue = config;
            serializedController.FindProperty("boardView").objectReferenceValue = boardView;
            serializedController.FindProperty("inputRouter").objectReferenceValue = inputRouter;
            serializedController.FindProperty("pawnController").objectReferenceValue = pawnController;
            serializedController.FindProperty("previewWall").objectReferenceValue = previewWall;
            serializedController.FindProperty("placedWallPrefab").objectReferenceValue = wallPrefab.GetComponent<WallView>();
            serializedController.FindProperty("placedWallsRoot").objectReferenceValue = placedWallsRoot;
            serializedController.FindProperty("validPreviewMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPreviewValid.mat");
            serializedController.FindProperty("invalidPreviewMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPreviewInvalid.mat");
            serializedController.FindProperty("placedMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPlaced.mat");
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(wallController);
            EditorSceneManager.MarkSceneDirty(sceneRoot.scene);
            EditorSceneManager.SaveScene(sceneRoot.scene);
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            var childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            return childObject.transform;
        }

        private static WallView EnsurePreviewWall(GameObject wallPrefab, Transform wallsRoot, BoardView boardView, GameConfig config)
        {
            Transform existing = wallsRoot.Find("WallPreview");
            GameObject previewObject = existing != null
                ? existing.gameObject
                : (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab, wallsRoot);

            previewObject.name = "WallPreview";
            WallView wallView = previewObject.GetComponent<WallView>();
            wallView.Configure(new WallPlacement(QuoridorRules.PlayerOneStart, WallOrientation.Horizontal), boardView, config,
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WallPreviewValid.mat"));
            wallView.SetPreviewValidity(true);
            wallView.SetVisible(false);
            EditorUtility.SetDirty(previewObject);
            return wallView;
        }
    }
}
