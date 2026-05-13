using Mirror;
using Quoridor.Menu;
using Quoridor.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quoridor.Editor.MenuGeneration
{
    /// <summary>
    /// Editor-only setup for Mirror LAN components in the main menu scene.
    /// </summary>
    public static class LanNetworkSetupGenerator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string RoomPlayerPrefabPath = "Assets/Prefabs/UI/QuoridorRoomPlayer.prefab";

        /// <summary>
        /// Creates or refreshes the Mirror NetworkManager scene object and room-player prefab.
        /// </summary>
        [MenuItem("Tools/Quoridor/Setup LAN Networking")]
        public static void SetupLanNetworking()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            QuoridorNetworkManager networkManager = EnsureNetworkManager();
            GameObject playerPrefab = EnsureRoomPlayerPrefab();
            WireNetworkManager(networkManager, playerPrefab);
            WireMainMenuController(networkManager);

            EditorUtility.SetDirty(networkManager);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Quoridor LAN networking setup complete.");
        }

        private static QuoridorNetworkManager EnsureNetworkManager()
        {
            QuoridorNetworkManager networkManager = Object.FindFirstObjectByType<QuoridorNetworkManager>(FindObjectsInactive.Include);
            if (networkManager != null)
            {
                return networkManager;
            }

            GameObject networkObject = new("NetworkManager", typeof(TelepathyTransport), typeof(QuoridorNetworkManager), typeof(QuoridorLanDiscovery));
            return networkObject.GetComponent<QuoridorNetworkManager>();
        }

        private static GameObject EnsureRoomPlayerPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(RoomPlayerPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject prefabSource = new("QuoridorRoomPlayer", typeof(NetworkIdentity), typeof(QuoridorRoomPlayer));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabSource, RoomPlayerPrefabPath);
            Object.DestroyImmediate(prefabSource);
            return prefab;
        }

        private static void WireNetworkManager(QuoridorNetworkManager networkManager, GameObject playerPrefab)
        {
            TelepathyTransport transport = networkManager.GetComponent<TelepathyTransport>();
            if (transport == null)
            {
                transport = networkManager.gameObject.AddComponent<TelepathyTransport>();
            }

            QuoridorLanDiscovery discovery = networkManager.GetComponent<QuoridorLanDiscovery>();
            if (discovery == null)
            {
                discovery = networkManager.gameObject.AddComponent<QuoridorLanDiscovery>();
            }

            SerializedObject serializedManager = new(networkManager);
            serializedManager.FindProperty("transport").objectReferenceValue = transport;
            serializedManager.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            serializedManager.FindProperty("autoCreatePlayer").boolValue = true;
            serializedManager.FindProperty("maxConnections").intValue = 2;
            serializedManager.FindProperty("offlineScene").stringValue = "MainMenu";
            serializedManager.FindProperty("onlineScene").stringValue = string.Empty;
            serializedManager.FindProperty("roomName").stringValue = "Princess Room";
            serializedManager.FindProperty("matchSceneName").stringValue = "QuoridorDemo";
            serializedManager.FindProperty("discovery").objectReferenceValue = discovery;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedDiscovery = new(discovery);
            serializedDiscovery.FindProperty("transport").objectReferenceValue = transport;
            serializedDiscovery.FindProperty("networkManager").objectReferenceValue = networkManager;
            serializedDiscovery.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireMainMenuController(QuoridorNetworkManager networkManager)
        {
            MainMenuController controller = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                Debug.LogWarning("MainMenuController not found while wiring LAN networking.");
                return;
            }

            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("networkManager").objectReferenceValue = networkManager;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }
    }
}
