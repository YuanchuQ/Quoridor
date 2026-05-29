// Generates and clears Quoridor board cell instances in edit mode
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Quoridor.EditorTools
{
    /// <summary>
    /// Generates and clears Quoridor board cell instances in edit mode.
    /// </summary>
    public static class QuoridorBoardGenerator
    {
        /// <summary>
        /// Number of cells on one side of a Quoridor board.
        /// </summary>
        public const int BoardSize = 9;

        /// <summary>
        /// Default child transform name used to hold generated cell instances.
        /// </summary>
        public const string DefaultContainerName = "GeneratedCells";

        private const string UndoGenerateGroup = "Generate Quoridor Board";
        private const string UndoClearGroup = "Clear Quoridor Board";

        /// <summary>
        /// Clears existing generated cells, instantiates a 9x9 board from the cell prefab, and optionally wires a BoardView reference.
        /// </summary>
        public static IReadOnlyList<GameObject> GenerateBoard(
            Transform boardRoot,
            GameObject cellPrefab,
            UnityObject boardView,
            float cellSpacing = 1f,
            string containerName = DefaultContainerName)
        {
            ValidateGenerationInput(boardRoot, cellPrefab, cellSpacing);

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoGenerateGroup);

            Transform container = GetOrCreateContainer(boardRoot, containerName);
            ClearGeneratedCells(boardRoot, containerName);

            var generatedCells = new List<GameObject>(BoardSize * BoardSize);
            float centerOffset = (BoardSize - 1) * 0.5f;

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    GameObject instance = CreateCellInstance(cellPrefab, container, x, y, centerOffset, cellSpacing);
                    generatedCells.Add(instance);
                }
            }

            UnityObject resolvedBoardView = boardView != null ? ResolveBoardViewObject(boardView) : FindBoardViewNearRoot(boardRoot);
            BoardViewReferenceWriter.TryWireBoardView(resolvedBoardView, generatedCells);

            EditorUtility.SetDirty(boardRoot);
            EditorSceneManager.MarkSceneDirty(boardRoot.gameObject.scene);
            Undo.CollapseUndoOperations(undoGroup);

            return generatedCells;
        }

        /// <summary>
        /// Destroys previously generated cells under the board root's generated-cell container.
        /// </summary>
        public static int ClearGeneratedCells(Transform boardRoot, string containerName = DefaultContainerName)
        {
            if (boardRoot == null)
            {
                throw new ArgumentNullException(nameof(boardRoot));
            }

            Transform container = boardRoot.Find(SanitizeContainerName(containerName));
            if (container == null)
            {
                return 0;
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoClearGroup);

            int destroyedCount = 0;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Transform child = container.GetChild(i);
                if (!child.name.StartsWith("Cell_", StringComparison.Ordinal))
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(child.gameObject);
                destroyedCount++;
            }

            EditorUtility.SetDirty(boardRoot);
            EditorSceneManager.MarkSceneDirty(boardRoot.gameObject.scene);
            Undo.CollapseUndoOperations(undoGroup);

            return destroyedCount;
        }

        /// <summary>
        /// Returns generated cell GameObjects in hierarchy order from the generated-cell container.
        /// </summary>
        public static IReadOnlyList<GameObject> GetGeneratedCells(Transform boardRoot, string containerName = DefaultContainerName)
        {
            if (boardRoot == null)
            {
                throw new ArgumentNullException(nameof(boardRoot));
            }

            Transform container = boardRoot.Find(SanitizeContainerName(containerName));
            var cells = new List<GameObject>(BoardSize * BoardSize);

            if (container == null)
            {
                return cells;
            }

            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                if (child.name.StartsWith("Cell_", StringComparison.Ordinal))
                {
                    cells.Add(child.gameObject);
                }
            }

            return cells;
        }

        /// <summary>
        /// Attempts to write generated cell references into a serialized BoardView cell collection.
        /// </summary>
        public static bool TryWireBoardView(UnityObject boardView, IReadOnlyList<GameObject> generatedCells)
        {
            return BoardViewReferenceWriter.TryWireBoardView(boardView, generatedCells);
        }

        private static void ValidateGenerationInput(Transform boardRoot, GameObject cellPrefab, float cellSpacing)
        {
            if (boardRoot == null)
            {
                throw new ArgumentNullException(nameof(boardRoot));
            }

            if (cellPrefab == null)
            {
                throw new ArgumentNullException(nameof(cellPrefab));
            }

            if (PrefabUtility.GetPrefabAssetType(cellPrefab) == PrefabAssetType.NotAPrefab)
            {
                throw new ArgumentException("Cell source must be a prefab asset.", nameof(cellPrefab));
            }

            if (cellSpacing <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSpacing), "Cell spacing must be greater than zero.");
            }
        }

        private static GameObject CreateCellInstance(GameObject cellPrefab, Transform container, int x, int y, float centerOffset, float cellSpacing)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab, container);
            Undo.RegisterCreatedObjectUndo(instance, UndoGenerateGroup);

            instance.name = $"Cell_{x}_{y}";
            instance.transform.localPosition = new Vector3((x - centerOffset) * cellSpacing, (y - centerOffset) * cellSpacing, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = cellPrefab.transform.localScale;

            CellInstanceConfigurator.TryConfigureCellInstance(instance, x, y);
            return instance;
        }

        private static Transform GetOrCreateContainer(Transform boardRoot, string containerName)
        {
            string safeName = SanitizeContainerName(containerName);
            Transform existing = boardRoot.Find(safeName);
            if (existing != null)
            {
                return existing;
            }

            var container = new GameObject(safeName);
            Undo.RegisterCreatedObjectUndo(container, UndoGenerateGroup);
            container.transform.SetParent(boardRoot, false);
            return container.transform;
        }

        private static string SanitizeContainerName(string containerName)
        {
            return string.IsNullOrWhiteSpace(containerName) ? DefaultContainerName : containerName.Trim();
        }

        private static UnityObject ResolveBoardViewObject(UnityObject boardView)
        {
            if (boardView is GameObject gameObject)
            {
                Component boardViewComponent = FindBoardViewComponent(gameObject);
                return boardViewComponent != null ? boardViewComponent : gameObject;
            }

            return boardView;
        }

        private static UnityObject FindBoardViewNearRoot(Transform boardRoot)
        {
            if (boardRoot == null)
            {
                return null;
            }

            UnityObject localBoardView = FindBoardViewComponent(boardRoot.gameObject);
            if (localBoardView != null)
            {
                return localBoardView;
            }

            Transform parent = boardRoot.parent;
            while (parent != null)
            {
                localBoardView = FindBoardViewComponent(parent.gameObject);
                if (localBoardView != null)
                {
                    return localBoardView;
                }

                parent = parent.parent;
            }

            return null;
        }

        private static Component FindBoardViewComponent(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            Component[] components = gameObject.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component != null && component.GetType().Name.Equals("BoardView", StringComparison.Ordinal))
                {
                    return component;
                }
            }

            return null;
        }

    }
}
