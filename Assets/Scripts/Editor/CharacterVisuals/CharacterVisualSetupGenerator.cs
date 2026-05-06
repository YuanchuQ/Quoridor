using Quoridor.Config;
using Quoridor.Core;
using Quoridor.GameFlow;
using Quoridor.Menu;
using Quoridor.Pawn;
using Quoridor.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Quoridor.Editor.CharacterVisuals
{
    /// <summary>
    /// Edit-time setup for character catalog, match portraits, and pawn character sprites.
    /// </summary>
    public static class CharacterVisualSetupGenerator
    {
        private const string CatalogPath = "Assets/Config/CharacterVisualCatalog.asset";
        private const string MatchHudPrefabPath = "Assets/Prefabs/UI/MatchHUD.prefab";
        private const string PawnPrefabPath = "Assets/Prefabs/Pawn/Pawn.prefab";
        private const string CharacterPawnMaterialPath = "Assets/Art/Materials/CharacterPawn.mat";
        private const string MatchScenePath = "Assets/Scenes/QuoridorDemo.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string CharacterFolder = "Assets/Art/Character";
        private const float PawnScale = 0.13f;

        private static readonly CharacterSeed[] CharacterSeeds =
        {
            new("yui", "优衣", "YUI", "1200px-优衣Q.png", "YuiPawnCharacter", new Vector2(0f, 0.12f)),
            new("pecorine", "佩可莉姆", "PECORINE", "1200px-佩可莉姆Q.png", "PecorinePawnCharacter", new Vector2(0f, 0.1f)),
            new("karyl", "凯露", "KARYL", "1200px-凯露Q.png", "KarylPawnCharacter", new Vector2(0f, 0.1f)),
            new("kokkoro", "可可萝", "KOKKORO", "1200px-可可萝Q.png", "KokkoroPawnCharacter", new Vector2(0f, 0.1f))
        };

        /// <summary>
        /// Creates or updates character visuals, HUD portrait masks, and scene references.
        /// </summary>
        [MenuItem("Tools/Quoridor/Generate Character Visual Setup")]
        public static void GenerateCharacterVisualSetup()
        {
            EnsureFolder("Assets/Scripts/Editor", "CharacterVisuals");
            CharacterVisualCatalog catalog = EnsureCatalog();
            Material pawnMaterial = EnsureCharacterPawnMaterial();
            EnsurePawnPrefab(catalog, pawnMaterial);
            EnsureMatchHudPrefab();
            WireMatchScene(catalog, pawnMaterial);
            WireMainMenuScene(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static CharacterVisualCatalog EnsureCatalog()
        {
            EnsureFolder("Assets", "Config");

            CharacterVisualCatalog catalog = AssetDatabase.LoadAssetAtPath<CharacterVisualCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CharacterVisualCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty characters = serializedCatalog.FindProperty("characters");
            characters.arraySize = CharacterSeeds.Length;

            for (int i = 0; i < CharacterSeeds.Length; i++)
            {
                CharacterSeed seed = CharacterSeeds[i];
                SerializedProperty character = characters.GetArrayElementAtIndex(i);
                character.FindPropertyRelative("characterId").stringValue = seed.Id;
                character.FindPropertyRelative("displayName").stringValue = seed.DisplayName;
                character.FindPropertyRelative("latinName").stringValue = seed.LatinName;
                Sprite sprite = LoadSprite(seed);
                character.FindPropertyRelative("portraitSprite").objectReferenceValue = sprite;
                character.FindPropertyRelative("pawnSprite").objectReferenceValue = sprite;
                character.FindPropertyRelative("pawnScale").floatValue = PawnScale;
                character.FindPropertyRelative("pawnOffset").vector2Value = seed.PawnOffset;
            }

            serializedCatalog.FindProperty("defaultPlayerOneCharacterId").stringValue = "yui";
            serializedCatalog.FindProperty("defaultPlayerTwoCharacterId").stringValue = "karyl";
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static Sprite LoadSprite(CharacterSeed seed)
        {
            string path = $"{CharacterFolder}/{seed.FileName}";
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite && sprite.name == seed.SpriteName)
                {
                    return sprite;
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Material EnsureCharacterPawnMaterial()
        {
            EnsureFolder("Assets/Art", "Materials");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(CharacterPawnMaterialPath);
            if (material == null)
            {
                material = new Material(FindSpriteShader());
                AssetDatabase.CreateAsset(material, CharacterPawnMaterialPath);
            }

            material.shader = FindSpriteShader();
            material.color = Color.white;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader FindSpriteShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader != null)
            {
                return shader;
            }

            return Shader.Find("Sprites/Default");
        }

        private static void EnsurePawnPrefab(CharacterVisualCatalog catalog, Material pawnMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PawnPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Pawn prefab not found at {PawnPrefabPath}.");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PawnView pawnView = instance.GetComponent<PawnView>();
            SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
            CharacterVisualDefinition defaultCharacter = catalog.GetDefault(PlayerId.PlayerOne);

            if (pawnView != null && defaultCharacter != null)
            {
                pawnView.SetCharacterVisual(defaultCharacter.PawnSprite, defaultCharacter.PawnScale, defaultCharacter.PawnOffset);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.sharedMaterial = pawnMaterial;
                spriteRenderer.sortingOrder = 20;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, PawnPrefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
        }

        private static void EnsureMatchHudPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MatchHudPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Match HUD prefab not found at {MatchHudPrefabPath}.");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            MatchUiView view = instance.GetComponent<MatchUiView>();
            Image playerOnePortrait = EnsurePortrait(instance.transform.Find("PlayerOneInfoPanel"), true);
            Image playerTwoPortrait = EnsurePortrait(instance.transform.Find("PlayerTwoInfoPanel"), false);
            Text playerOneSubName = FindChildText(instance.transform.Find("PlayerOneInfoPanel"), "NameSubText");
            Text playerTwoSubName = FindChildText(instance.transform.Find("PlayerTwoInfoPanel"), "NameSubText");

            if (view != null)
            {
                var serializedView = new SerializedObject(view);
                serializedView.FindProperty("playerOneSubNameText").objectReferenceValue = playerOneSubName;
                serializedView.FindProperty("playerOnePortraitImage").objectReferenceValue = playerOnePortrait;
                serializedView.FindProperty("playerTwoSubNameText").objectReferenceValue = playerTwoSubName;
                serializedView.FindProperty("playerTwoPortraitImage").objectReferenceValue = playerTwoPortrait;
                serializedView.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(instance, MatchHudPrefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
        }

        private static Image EnsurePortrait(Transform panel, bool isLeftSide)
        {
            if (panel == null)
            {
                return null;
            }

            Transform maskTransform = panel.Find("PortraitMask");
            GameObject maskObject;
            if (maskTransform == null)
            {
                maskObject = new GameObject("PortraitMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
                maskObject.transform.SetParent(panel, false);
            }
            else
            {
                maskObject = maskTransform.gameObject;
                if (maskObject.GetComponent<Image>() == null)
                {
                    maskObject.AddComponent<Image>();
                }

                if (maskObject.GetComponent<RectMask2D>() == null)
                {
                    maskObject.AddComponent<RectMask2D>();
                }
            }

            RectTransform maskRect = maskObject.GetComponent<RectTransform>();
            maskRect.anchorMin = new Vector2(0f, 1f);
            maskRect.anchorMax = new Vector2(0f, 1f);
            maskRect.pivot = new Vector2(0f, 1f);
            maskRect.anchoredPosition = isLeftSide ? new Vector2(39f, -52f) : new Vector2(51f, -52f);
            maskRect.sizeDelta = new Vector2(146f, 124f);

            Image maskImage = maskObject.GetComponent<Image>();
            maskImage.color = new Color(1f, 1f, 1f, 0f);
            maskImage.raycastTarget = false;

            Transform imageTransform = maskObject.transform.Find("PortraitImage");
            GameObject imageObject;
            if (imageTransform == null)
            {
                imageObject = new GameObject("PortraitImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                imageObject.transform.SetParent(maskObject.transform, false);
            }
            else
            {
                imageObject = imageTransform.gameObject;
                if (imageObject.GetComponent<Image>() == null)
                {
                    imageObject.AddComponent<Image>();
                }
            }

            RectTransform imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.5f, 0f);
            imageRect.anchorMax = new Vector2(0.5f, 0f);
            imageRect.pivot = new Vector2(0.5f, 0f);
            imageRect.anchoredPosition = new Vector2(0f, -16f);
            imageRect.sizeDelta = new Vector2(132f, 250f);

            Image portrait = imageObject.GetComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            return portrait;
        }

        private static Text FindChildText(Transform parent, string childName)
        {
            Transform child = parent != null ? parent.Find(childName) : null;
            return child != null ? child.GetComponent<Text>() : null;
        }

        private static void WireMatchScene(CharacterVisualCatalog catalog, Material pawnMaterial)
        {
            Scene scene = EditorSceneManager.OpenScene(MatchScenePath, OpenSceneMode.Single);
            PawnController pawnController = UnityEngine.Object.FindFirstObjectByType<PawnController>(FindObjectsInactive.Include);
            GameFlowController gameFlowController = UnityEngine.Object.FindFirstObjectByType<GameFlowController>(FindObjectsInactive.Include);
            MatchUiView matchUiView = UnityEngine.Object.FindFirstObjectByType<MatchUiView>(FindObjectsInactive.Include);

            if (pawnController != null)
            {
                var serializedPawnController = new SerializedObject(pawnController);
                serializedPawnController.FindProperty("characterCatalog").objectReferenceValue = catalog;
                serializedPawnController.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pawnController);
            }

            if (gameFlowController != null)
            {
                var serializedGameFlow = new SerializedObject(gameFlowController);
                serializedGameFlow.FindProperty("characterCatalog").objectReferenceValue = catalog;
                serializedGameFlow.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gameFlowController);
            }

            if (matchUiView != null)
            {
                GameObject hudInstance = matchUiView.gameObject;
                Image playerOnePortrait = EnsurePortrait(hudInstance.transform.Find("PlayerOneInfoPanel"), true);
                Image playerTwoPortrait = EnsurePortrait(hudInstance.transform.Find("PlayerTwoInfoPanel"), false);

                var serializedView = new SerializedObject(matchUiView);
                serializedView.FindProperty("playerOneSubNameText").objectReferenceValue =
                    FindChildText(hudInstance.transform.Find("PlayerOneInfoPanel"), "NameSubText");
                serializedView.FindProperty("playerOnePortraitImage").objectReferenceValue = playerOnePortrait;
                serializedView.FindProperty("playerTwoSubNameText").objectReferenceValue =
                    FindChildText(hudInstance.transform.Find("PlayerTwoInfoPanel"), "NameSubText");
                serializedView.FindProperty("playerTwoPortraitImage").objectReferenceValue = playerTwoPortrait;
                serializedView.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(matchUiView);
            }

            foreach (PawnView pawn in UnityEngine.Object.FindObjectsByType<PawnView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                CharacterVisualDefinition character = catalog.GetDefault(pawn.PlayerId);
                if (character != null)
                {
                    pawn.SetCharacterVisual(character.PawnSprite, character.PawnScale, character.PawnOffset);
                }

                SpriteRenderer spriteRenderer = pawn.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                    spriteRenderer.sharedMaterial = pawnMaterial;
                }

                EditorUtility.SetDirty(pawn);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void WireMainMenuScene(CharacterVisualCatalog catalog)
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            MainMenuController menu = UnityEngine.Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);

            if (menu != null)
            {
                var serializedMenu = new SerializedObject(menu);
                serializedMenu.FindProperty("characterCatalog").objectReferenceValue = catalog;
                serializedMenu.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(menu);
            }

            foreach (CharacterSelectButton button in UnityEngine.Object.FindObjectsByType<CharacterSelectButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serializedButton = new SerializedObject(button);
                string displayName = serializedButton.FindProperty("characterName").stringValue;
                CharacterVisualDefinition character = catalog.GetByDisplayName(displayName);
                if (character != null)
                {
                    serializedButton.FindProperty("characterId").stringValue = character.CharacterId;
                    serializedButton.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(button);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private readonly struct CharacterSeed
        {
            public CharacterSeed(string id, string displayName, string latinName, string fileName, string spriteName, Vector2 pawnOffset)
            {
                Id = id;
                DisplayName = displayName;
                LatinName = latinName;
                FileName = fileName;
                SpriteName = spriteName;
                PawnOffset = pawnOffset;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public string LatinName { get; }
            public string FileName { get; }
            public string SpriteName { get; }
            public Vector2 PawnOffset { get; }
        }
    }

}
