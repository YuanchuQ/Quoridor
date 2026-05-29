// Editor window used to generate the Quoridor board once during edit mode
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Quoridor.EditorTools
{
    /// <summary>
    /// Editor window used to generate the Quoridor board once during edit mode.
    /// </summary>
    public sealed class QuoridorBoardGeneratorWindow : EditorWindow
    {
        private const string WindowTitle = "Quoridor Board Generator";

        [SerializeField] private Transform boardRoot;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private UnityObject boardView;
        [SerializeField] private float cellSpacing = 1f;
        [SerializeField] private string generatedContainerName = QuoridorBoardGenerator.DefaultContainerName;

        /// <summary>
        /// Opens the board generator editor window.
        /// </summary>
        [MenuItem("Tools/Quoridor/Board Generator")]
        public static void Open()
        {
            GetWindow<QuoridorBoardGeneratorWindow>(WindowTitle);
        }

        /// <summary>
        /// Opens the board generator and assigns the selected transform as the board root.
        /// </summary>
        [MenuItem("GameObject/Quoridor/Generate 9x9 Board From Selected Root", false, 10)]
        public static void OpenForSelectedRoot()
        {
            QuoridorBoardGeneratorWindow window = GetWindow<QuoridorBoardGeneratorWindow>(WindowTitle);
            window.boardRoot = Selection.activeTransform;
            window.Focus();
        }

        [MenuItem("GameObject/Quoridor/Generate 9x9 Board From Selected Root", true)]
        private static bool CanOpenForSelectedRoot()
        {
            return Selection.activeTransform != null;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Edit-time Board Generation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Generates prefab instances under a GeneratedCells child. This is an editor-only tool and does not generate the board at runtime.",
                MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                boardRoot = (Transform)EditorGUILayout.ObjectField("Board Root", boardRoot, typeof(Transform), true);
                cellPrefab = (GameObject)EditorGUILayout.ObjectField("Cell Prefab", cellPrefab, typeof(GameObject), false);
                boardView = EditorGUILayout.ObjectField("Board View", boardView, typeof(UnityObject), true);
                cellSpacing = EditorGUILayout.FloatField("Cell Spacing", cellSpacing);
                generatedContainerName = EditorGUILayout.TextField("Container Name", generatedContainerName);
            }

            using (new EditorGUI.DisabledScope(boardRoot == null || cellPrefab == null))
            {
                if (GUILayout.Button("Generate Board"))
                {
                    QuoridorBoardGenerator.GenerateBoard(boardRoot, cellPrefab, boardView, cellSpacing, generatedContainerName);
                }
            }

            using (new EditorGUI.DisabledScope(boardRoot == null))
            {
                if (GUILayout.Button("Clear Generated Cells"))
                {
                    QuoridorBoardGenerator.ClearGeneratedCells(boardRoot, generatedContainerName);
                }
            }

            using (new EditorGUI.DisabledScope(boardRoot == null || boardView == null))
            {
                if (GUILayout.Button("Wire Existing Generated Cells"))
                {
                    IReadOnlyList<GameObject> cells = QuoridorBoardGenerator.GetGeneratedCells(boardRoot, generatedContainerName);
                    QuoridorBoardGenerator.TryWireBoardView(boardView, cells);
                }
            }
        }
    }
}
