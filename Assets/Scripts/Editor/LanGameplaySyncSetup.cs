using Quoridor.GameFlow;
using Quoridor.Input;
using Quoridor.Networking;
using Quoridor.Pawn;
using Quoridor.Wall;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Quoridor.EditorTools
{
    /// <summary>
    /// Editor utility for wiring LAN gameplay synchronization into the match scene.
    /// </summary>
    public static class LanGameplaySyncSetup
    {
        private const string MatchScenePath = "Assets/Scenes/QuoridorDemo.unity";
        private const string SystemsObjectName = "Systems";

        /// <summary>
        /// Adds and wires NetworkMatchController on the match scene Systems object.
        /// </summary>
        [MenuItem("Tools/Quoridor/Setup LAN Gameplay Sync")]
        public static void SetupMatchScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MatchScenePath);
            GameObject systems = GameObject.Find(SystemsObjectName);
            if (systems == null)
            {
                Debug.LogError($"LAN setup failed: missing {SystemsObjectName} object in {MatchScenePath}.");
                return;
            }

            NetworkMatchController matchController = systems.GetComponent<NetworkMatchController>();
            if (matchController == null)
            {
                matchController = Undo.AddComponent<NetworkMatchController>(systems);
            }

            SerializedObject serializedController = new(matchController);
            SetReference(serializedController, "inputRouter", systems.GetComponent<InputRouter>());
            SetReference(serializedController, "pawnController", systems.GetComponent<PawnController>());
            SetReference(serializedController, "wallController", systems.GetComponent<WallController>());
            SetReference(serializedController, "gameFlowController", systems.GetComponent<GameFlowController>());
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("LAN gameplay sync setup complete.");
        }

        private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
