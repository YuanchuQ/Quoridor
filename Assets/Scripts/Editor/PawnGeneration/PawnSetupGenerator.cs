using System.IO;
using Quoridor.Board;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Input;
using Quoridor.Pawn;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Quoridor.Editor.PawnGeneration
{
    /// <summary>
    /// Edit-time generator for pawn prefab and scene wiring.
    /// </summary>
    public static class PawnSetupGenerator
    {
        private const string PawnPrefabPath = "Assets/Prefabs/Pawn/Pawn.prefab";
        private const string PawnSpritePath = "Assets/Art/PawnCircle.png";
        private const int PawnTextureSize = 96;
        private const int PawnSortingOrder = 20;

        /// <summary>
        /// Creates or updates the pawn prefab, scene pawn instances and pawn controller wiring.
        /// </summary>
        [MenuItem("Tools/Quoridor/Generate Pawn Setup")]
        public static void GeneratePawnSetup()
        {
            EnsureFolders();
            Sprite pawnSprite = EnsurePawnSprite();
            GameObject prefab = EnsurePawnPrefab(pawnSprite);
            WireScene(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Prefabs", "Pawn");
            EnsureFolder("Assets/Scripts/Editor", "PawnGeneration");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Sprite EnsurePawnSprite()
        {
            if (!File.Exists(PawnSpritePath))
            {
                Texture2D texture = CreatePawnTexture();
                File.WriteAllBytes(PawnSpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(PawnSpritePath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(PawnSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = PawnTextureSize;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(PawnSpritePath);
        }

        private static Texture2D CreatePawnTexture()
        {
            var texture = new Texture2D(PawnTextureSize, PawnTextureSize, TextureFormat.RGBA32, false);
            var clear = new Color(0f, 0f, 0f, 0f);
            var fill = Color.white;
            Vector2 center = new Vector2((PawnTextureSize - 1) * 0.5f, (PawnTextureSize - 1) * 0.5f);
            float outerRadius = PawnTextureSize * 0.42f;
            float innerRadius = PawnTextureSize * 0.34f;

            for (int y = 0; y < PawnTextureSize; y++)
            {
                for (int x = 0; x < PawnTextureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color color = distance <= outerRadius ? fill : clear;
                    if (distance <= innerRadius)
                    {
                        color = Color.Lerp(fill, new Color(0.82f, 0.82f, 0.82f, 1f), 0.25f);
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private static GameObject EnsurePawnPrefab(Sprite pawnSprite)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PawnPrefabPath);
            GameObject pawnObject = existingPrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(existingPrefab)
                : new GameObject("Pawn");

            SpriteRenderer spriteRenderer = pawnObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = pawnObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = pawnSprite;
            spriteRenderer.sortingOrder = PawnSortingOrder;
            spriteRenderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/PawnPlayerOne.mat");

            if (pawnObject.GetComponent<CircleCollider2D>() == null)
            {
                pawnObject.AddComponent<CircleCollider2D>().radius = 0.36f;
            }

            if (pawnObject.GetComponent<PawnView>() == null)
            {
                pawnObject.AddComponent<PawnView>();
            }

            pawnObject.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
            PrefabUtility.SaveAsPrefabAsset(pawnObject, PawnPrefabPath);
            Object.DestroyImmediate(pawnObject);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PawnPrefabPath);
        }

        private static void WireScene(GameObject pawnPrefab)
        {
            GameObject sceneRoot = GameObject.Find("SceneRoot");
            GameObject systems = GameObject.Find("SceneRoot/Systems");
            GameObject boardRoot = GameObject.Find("SceneRoot/BoardRoot");

            if (sceneRoot == null || systems == null || boardRoot == null)
            {
                Debug.LogError("Pawn setup requires SceneRoot, SceneRoot/Systems and SceneRoot/BoardRoot in the active scene.");
                return;
            }

            BoardView boardView = boardRoot.GetComponent<BoardView>();
            InputRouter inputRouter = systems.GetComponent<InputRouter>();
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
            if (boardView == null || inputRouter == null || config == null)
            {
                Debug.LogError("Pawn setup requires BoardView, InputRouter and GameConfig.");
                return;
            }

            Transform pawnsRoot = EnsureChild(sceneRoot.transform, "Pawns");
            PawnView playerOnePawn = EnsurePawnInstance(pawnPrefab, pawnsRoot, "PlayerOnePawn", PlayerId.PlayerOne, boardView, config);
            PawnView playerTwoPawn = EnsurePawnInstance(pawnPrefab, pawnsRoot, "PlayerTwoPawn", PlayerId.PlayerTwo, boardView, config);
            WirePawnController(systems, config, boardView, inputRouter, playerOnePawn, playerTwoPawn);

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

        private static PawnView EnsurePawnInstance(
            GameObject pawnPrefab,
            Transform pawnsRoot,
            string objectName,
            PlayerId playerId,
            BoardView boardView,
            GameConfig config)
        {
            Transform existing = pawnsRoot.Find(objectName);
            GameObject pawnObject = existing != null
                ? existing.gameObject
                : (GameObject)PrefabUtility.InstantiatePrefab(pawnPrefab, pawnsRoot);

            pawnObject.name = objectName;
            PawnView pawnView = pawnObject.GetComponent<PawnView>();
            SpriteRenderer spriteRenderer = pawnObject.GetComponent<SpriteRenderer>();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                playerId == PlayerId.PlayerOne
                    ? "Assets/Art/Materials/PawnPlayerOne.mat"
                    : "Assets/Art/Materials/PawnPlayerTwo.mat");

            if (spriteRenderer != null)
            {
                spriteRenderer.sharedMaterial = material;
                spriteRenderer.sortingOrder = PawnSortingOrder;
            }

            BoardPosition startPosition = GetStartPosition(playerId, config);
            pawnView.Configure(playerId, boardView, startPosition, config.PawnMoveDuration);
            pawnView.SetMaterial(material);
            EditorUtility.SetDirty(pawnObject);
            return pawnView;
        }

        private static void WirePawnController(
            GameObject systems,
            GameConfig config,
            BoardView boardView,
            InputRouter inputRouter,
            PawnView playerOnePawn,
            PawnView playerTwoPawn)
        {
            PawnController controller = systems.GetComponent<PawnController>();
            if (controller == null)
            {
                controller = systems.AddComponent<PawnController>();
            }

            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("config").objectReferenceValue = config;
            serializedController.FindProperty("boardView").objectReferenceValue = boardView;
            serializedController.FindProperty("inputRouter").objectReferenceValue = inputRouter;
            serializedController.FindProperty("playerOnePawn").objectReferenceValue = playerOnePawn;
            serializedController.FindProperty("playerTwoPawn").objectReferenceValue = playerTwoPawn;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static BoardPosition GetStartPosition(PlayerId playerId, GameConfig config)
        {
            Vector2Int start = playerId == PlayerId.PlayerOne ? config.PlayerOneStart : config.PlayerTwoStart;
            return new BoardPosition(start.x, start.y);
        }
    }
}
