using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quoridor.EditorTools
{
    /// <summary>
    /// Applies project art assets to Quoridor prefabs and import settings.
    /// </summary>
    public static class QuoridorArtAssetUtility
    {
        /// <summary>
        /// Source tile art used by the cell prefab.
        /// </summary>
        public const string TileSpritePath = "Assets/Art/Tile.png";

        /// <summary>
        /// Source wall sheet supplied by art.
        /// </summary>
        public const string WallSourcePath = "Assets/Art/Wall.png";

        /// <summary>
        /// Derived transparent wall strip used by the wall prefab.
        /// </summary>
        public const string WallSpritePath = "Assets/Art/WallQuoridor.png";

        private const string CellPrefabPath = "Assets/Prefabs/Board/Cell.prefab";
        private const string WallPrefabPath = "Assets/Prefabs/Wall/Wall.prefab";
        private const string CellDefaultMaterialPath = "Assets/Art/Materials/CellDefault.mat";
        private const string WallPlacedMaterialPath = "Assets/Art/Materials/WallPlaced.mat";
        private const float TilePixelsPerUnit = 1254f;
        private const float WallPixelsPerUnit = 250f;
        private const string WallSourceSpriteName = "Wall_0";
        private const int NeutralMinimum = 185;
        private const int NeutralTolerance = 16;

        /// <summary>
        /// Applies the current tile and wall art to game prefabs.
        /// </summary>
        [MenuItem("Tools/Quoridor/Apply Art Refresh")]
        public static void ApplyArtRefresh()
        {
            Sprite tileSprite = EnsureTileSprite();
            Sprite wallSprite = EnsureWallSprite();

            SetPrefabSprite(CellPrefabPath, tileSprite);
            SetPrefabSprite(WallPrefabPath, wallSprite);
            SetMaterialColor(CellDefaultMaterialPath, Color.white);
            SetMaterialColor(WallPlacedMaterialPath, Color.white);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Quoridor art refresh applied.");
        }

        /// <summary>
        /// Imports and returns the tile sprite used by board cells.
        /// </summary>
        public static Sprite EnsureTileSprite()
        {
            ConfigureSpriteImporter(TileSpritePath, TilePixelsPerUnit);
            return LoadSprite(TileSpritePath);
        }

        /// <summary>
        /// Creates, imports, and returns the transparent wall sprite used by wall pieces.
        /// </summary>
        public static Sprite EnsureWallSprite()
        {
            CreateTransparentWallSprite();
            ConfigureSpriteImporter(WallSpritePath, WallPixelsPerUnit);
            return LoadSprite(WallSpritePath);
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite not found at {path}.");
            }

            return sprite;
        }

        private static void ConfigureSpriteImporter(string path, float pixelsPerUnit)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Texture importer not found at {path}.");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        private static void CreateTransparentWallSprite()
        {
            Texture2D source = LoadReadablePng(WallSourcePath);
            RectInt rect = GetWallSourceRect(source);
            Color32[] sourcePixels = source.GetPixels32();
            bool[] transparentMask = BuildTransparentMask(sourcePixels, source.width, rect);
            Texture2D output = BuildWallTexture(sourcePixels, source.width, rect, transparentMask);

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), WallSpritePath);
            File.WriteAllBytes(fullPath, output.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(WallSpritePath, ImportAssetOptions.ForceUpdate);
        }

        private static Texture2D LoadReadablePng(string assetPath)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            byte[] bytes = File.ReadAllBytes(fullPath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                throw new InvalidOperationException($"Could not load image at {assetPath}.");
            }

            return texture;
        }

        private static RectInt GetWallSourceRect(Texture2D source)
        {
            Sprite[] sprites = LoadAllSprites(WallSourcePath);
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name != WallSourceSpriteName)
                {
                    continue;
                }

                Rect rect = sprite.textureRect;
                return ClampRect(new RectInt(
                    Mathf.RoundToInt(rect.x),
                    Mathf.RoundToInt(rect.y),
                    Mathf.RoundToInt(rect.width),
                    Mathf.RoundToInt(rect.height)), source);
            }

            return ClampRect(new RectInt(82, 124, 1362, 250), source);
        }

        private static Sprite[] LoadAllSprites(string path)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new List<Sprite>();
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
        }

        private static RectInt ClampRect(RectInt rect, Texture2D source)
        {
            int x = Mathf.Clamp(rect.x, 0, source.width - 1);
            int y = Mathf.Clamp(rect.y, 0, source.height - 1);
            int width = Mathf.Clamp(rect.width, 1, source.width - x);
            int height = Mathf.Clamp(rect.height, 1, source.height - y);
            return new RectInt(x, y, width, height);
        }

        private static bool[] BuildTransparentMask(Color32[] sourcePixels, int sourceWidth, RectInt rect)
        {
            bool[] transparentMask = new bool[rect.width * rect.height];
            var queue = new Queue<int>();

            for (int x = 0; x < rect.width; x++)
            {
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, x, 0);
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, x, rect.height - 1);
            }

            for (int y = 0; y < rect.height; y++)
            {
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, 0, y);
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, rect.width - 1, y);
            }

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % rect.width;
                int y = index / rect.width;
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, x + 1, y);
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, x - 1, y);
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, x, y + 1);
                TryMarkTransparent(sourcePixels, sourceWidth, rect, transparentMask, queue, x, y - 1);
            }

            return transparentMask;
        }

        private static void TryMarkTransparent(
            Color32[] sourcePixels,
            int sourceWidth,
            RectInt rect,
            bool[] transparentMask,
            Queue<int> queue,
            int x,
            int y)
        {
            if (x < 0 || y < 0 || x >= rect.width || y >= rect.height)
            {
                return;
            }

            int index = y * rect.width + x;
            if (transparentMask[index])
            {
                return;
            }

            Color32 color = sourcePixels[(rect.y + y) * sourceWidth + rect.x + x];
            if (!IsNeutralChecker(color))
            {
                return;
            }

            transparentMask[index] = true;
            queue.Enqueue(index);
        }

        private static bool IsNeutralChecker(Color32 color)
        {
            int max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            int min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            int average = (color.r + color.g + color.b) / 3;
            return average >= NeutralMinimum && max - min <= NeutralTolerance;
        }

        private static Texture2D BuildWallTexture(Color32[] sourcePixels, int sourceWidth, RectInt rect, bool[] transparentMask)
        {
            var output = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
            Color32[] outputPixels = new Color32[rect.width * rect.height];

            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    int index = y * rect.width + x;
                    Color32 color = sourcePixels[(rect.y + y) * sourceWidth + rect.x + x];
                    color.a = transparentMask[index] ? (byte)0 : (byte)255;
                    outputPixels[index] = color;
                }
            }

            output.SetPixels32(outputPixels);
            output.Apply();
            return output;
        }

        private static void SetPrefabSprite(string prefabPath, Sprite sprite)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                SpriteRenderer spriteRenderer = prefabRoot.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    throw new InvalidOperationException($"SpriteRenderer not found on {prefabPath}.");
                }

                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void SetMaterialColor(string materialPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                throw new InvalidOperationException($"Material not found at {materialPath}.");
            }

            material.color = color;
            EditorUtility.SetDirty(material);
        }
    }
}
