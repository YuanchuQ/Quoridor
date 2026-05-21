using System;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Quoridor.Menu
{
    /// <summary>
    /// Controls the main menu Canvas panels, placeholder room flow, and character selection.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string localGameSceneName = "QuoridorDemo";
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject twoPlayerPanel;
        [SerializeField] private GameObject lanPanel;
        [SerializeField] private GameObject roomListPanel;
        [SerializeField] private GameObject roomPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Text statusText;
        [SerializeField] private Text lanSelectedCharacterText;
        [SerializeField] private Text roomListText;
        [SerializeField] private Text roomInfoText;
        [SerializeField] private Text roomTitleText;
        [SerializeField] private Text roomWaitingText;
        [SerializeField] private Text selectedCharacterText;
        [SerializeField] private CharacterVisualCatalog characterCatalog;
        [SerializeField] private QuoridorNetworkManager networkManager;
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button twoPlayerButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button lanButton;
        [SerializeField] private Button localButton;
        [SerializeField] private Button twoPlayerBackButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button lanBackButton;
        [SerializeField] private Button roomListFirstRoomButton;
        [SerializeField] private Button roomListSecondRoomButton;
        [SerializeField] private Button roomListBackButton;
        [SerializeField] private Button roomBackButton;
        [SerializeField] private Button startLocalFromRoomButton;
        [SerializeField] private Button settingsBackButton;

        private const string DefaultStatus = "Ready";
        private const string CreatedRoomName = "我的房间";
        private const string FirstRoomName = "Princess Room";
        private const string SecondRoomName = "Practice Room";
        private static readonly string[] PreferredFontNames =
        {
            "PingFang SC",
            "Hiragino Sans GB",
            "Microsoft YaHei",
            "Noto Sans CJK SC",
            "Arial Unicode MS",
            "Arial"
        };

        private string selectedCharacterName = string.Empty;
        private string selectedCharacterId = string.Empty;
        private string activeRoomName = string.Empty;
        private bool activeRoomIsNetworked;
        private QuoridorLanRoomInfo firstDiscoveredRoom;
        private QuoridorLanRoomInfo secondDiscoveredRoom;
        private QuoridorLanRoomInfo selectedDiscoveredRoom;
        private bool hasFirstDiscoveredRoom;
        private bool hasSecondDiscoveredRoom;
        private bool hasSelectedDiscoveredRoom;

        /// <summary>
        /// Selects a character by display name from a character card button.
        /// </summary>
        public void SelectCharacter(string characterId, string characterName)
        {
            selectedCharacterId = string.IsNullOrWhiteSpace(characterId)
                ? ResolveCharacterId(characterName)
                : characterId;
            selectedCharacterName = string.IsNullOrWhiteSpace(characterName) ? "Unknown" : characterName;
            RefreshSelectedCharacter();
            if (networkManager != null && networkManager.IsClientConnected)
            {
                QuoridorRoomPlayer localRoomPlayer = GetLocalRoomPlayer();
                if (localRoomPlayer != null)
                {
                    localRoomPlayer.SetLocalCharacter(selectedCharacterId);
                }
            }
        }

        /// <summary>
        /// Selects a character by display name from legacy character card buttons.
        /// </summary>
        public void SelectCharacter(string characterName)
        {
            SelectCharacter(ResolveCharacterId(characterName), characterName);
        }

        private void Awake()
        {
            ApplyReadableFont();
            ResolveNetworkManager();
            BindButtons();
            if (networkManager != null)
            {
                networkManager.RoomStateChanged += HandleNetworkRoomStateChanged;
            }

            ShowMain();
        }

        private void OnDestroy()
        {
            UnbindButtons();
            if (networkManager != null)
            {
                networkManager.RoomStateChanged -= HandleNetworkRoomStateChanged;
                networkManager.StopLanSearch(HandleRoomFound);
            }
        }

        private void BindButtons()
        {
            Add(singlePlayerButton, HandleSinglePlayer);
            Add(twoPlayerButton, ShowTwoPlayer);
            Add(settingsButton, ShowSettings);
            Add(quitButton, HandleQuit);
            Add(lanButton, ShowLan);
            Add(localButton, LoadLocalGame);
            Add(twoPlayerBackButton, ShowMain);
            Add(joinRoomButton, JoinRoom);
            Add(createRoomButton, CreateRoom);
            Add(lanBackButton, ReturnFromLan);
            Add(roomListFirstRoomButton, JoinFirstRoom);
            Add(roomListSecondRoomButton, JoinSecondRoom);
            if (roomListBackButton != lanBackButton)
            {
                Add(roomListBackButton, ShowLan);
            }

            Add(roomBackButton, ShowLan);
            Add(startLocalFromRoomButton, LoadLocalGame);
            Add(settingsBackButton, ShowMain);
        }

        private void UnbindButtons()
        {
            Remove(singlePlayerButton, HandleSinglePlayer);
            Remove(twoPlayerButton, ShowTwoPlayer);
            Remove(settingsButton, ShowSettings);
            Remove(quitButton, HandleQuit);
            Remove(lanButton, ShowLan);
            Remove(localButton, LoadLocalGame);
            Remove(twoPlayerBackButton, ShowMain);
            Remove(joinRoomButton, JoinRoom);
            Remove(createRoomButton, CreateRoom);
            Remove(lanBackButton, ReturnFromLan);
            Remove(roomListFirstRoomButton, JoinFirstRoom);
            Remove(roomListSecondRoomButton, JoinSecondRoom);
            if (roomListBackButton != lanBackButton)
            {
                Remove(roomListBackButton, ShowLan);
            }

            Remove(roomBackButton, ShowLan);
            Remove(startLocalFromRoomButton, LoadLocalGame);
            Remove(settingsBackButton, ShowMain);
        }

        private static void Add(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void Remove(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private void ShowMain()
        {
            ShowPanel(mainPanel);
            SetStatus(DefaultStatus);
        }

        private void ShowTwoPlayer()
        {
            StopRoomSearch();
            ShowPanel(twoPlayerPanel);
            SetStatus("Choose a multiplayer mode");
        }

        private void ReturnFromLan()
        {
            StopRoomSearch();
            activeRoomIsNetworked = false;
            activeRoomName = string.Empty;
            ClearDiscoveredRooms();

            if (networkManager != null && (networkManager.IsHostActive || networkManager.IsClientConnected))
            {
                networkManager.StopLanSession();
            }

            ShowMain();
        }

        private void ShowLan()
        {
            ShowPanel(lanPanel);
            EnsureDefaultCharacterSelection();
            ClearDiscoveredRooms();
            RefreshLanPanel();
            SetRoomListButton(roomListFirstRoomButton, "搜索中...", false);
            SetRoomListButton(roomListSecondRoomButton, "继续搜索...", false);
            StartRoomSearch();
            SetStatus("选择角色并加入房间");
        }

        private void ShowRoomList()
        {
            ShowPanel(roomListPanel);
            ClearDiscoveredRooms();
            SetRoomListButton(roomListFirstRoomButton, "搜索中...", false);
            SetRoomListButton(roomListSecondRoomButton, "搜索中...", false);
            SetRoomListText("正在搜索局域网房间...");
            StartRoomSearch();
            SetStatus("搜索局域网房间");
        }

        private void ShowSettings()
        {
            ShowPanel(settingsPanel);
            SetStatus("Settings placeholder");
        }

        private void ShowRoom(string roomName)
        {
            activeRoomName = roomName;
            ShowPanel(roomPanel);
            EnsureDefaultCharacterSelection();
            if (roomTitleText != null)
            {
                roomTitleText.text = roomName;
            }

            RefreshSelectedCharacter();
            RefreshRoomState();
            SetStatus("等待第二位玩家");
        }

        private void JoinRoom()
        {
            JoinDiscoveredRoom(selectedDiscoveredRoom, hasSelectedDiscoveredRoom);
        }

        private void CreateRoom()
        {
            ResolveNetworkManager();
            if (networkManager == null)
            {
                SetStatus("NetworkManager 未配置");
                return;
            }

            EnsureDefaultCharacterSelection();
            activeRoomName = CreatedRoomName;
            ShowRoom(CreatedRoomName);
            activeRoomIsNetworked = true;
            networkManager.StartLanHost(selectedCharacterId);
            RefreshRoomState();
            SetStatus("房间已创建，等待第二位玩家");
        }

        private void JoinFirstRoom()
        {
            SelectDiscoveredRoom(firstDiscoveredRoom, hasFirstDiscoveredRoom);
        }

        private void JoinSecondRoom()
        {
            SelectDiscoveredRoom(secondDiscoveredRoom, hasSecondDiscoveredRoom);
        }

        private void HandleSinglePlayer()
        {
            SetStatus("Single Player is reserved for a future AI opponent");
        }

        private void LoadLocalGame()
        {
            if (activeRoomIsNetworked)
            {
                StartNetworkGame();
                return;
            }

            ApplyLocalCharacterSelection();
            SceneManager.LoadScene(localGameSceneName);
        }

        private void HandleQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowPanel(GameObject activePanel)
        {
            SetActive(mainPanel, activePanel == mainPanel);
            SetActive(twoPlayerPanel, activePanel == twoPlayerPanel);
            SetActive(lanPanel, activePanel == lanPanel);
            SetActive(roomListPanel, activePanel == roomListPanel);
            SetActive(roomPanel, activePanel == roomPanel);
            SetActive(settingsPanel, activePanel == settingsPanel);
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void RefreshSelectedCharacter()
        {
            if (selectedCharacterText != null)
            {
                selectedCharacterText.text = $"角色：{selectedCharacterName}";
            }

            if (lanSelectedCharacterText != null)
            {
                lanSelectedCharacterText.text = $"角色：{selectedCharacterName}";
            }
        }

        private void RefreshLanPanel()
        {
            RefreshSelectedCharacter();
            RefreshRoomList();
        }

        private void RefreshRoomState()
        {
            if (activeRoomIsNetworked)
            {
                RefreshNetworkRoomState();
                return;
            }

            if (startLocalFromRoomButton != null)
            {
                startLocalFromRoomButton.interactable = true;
            }
        }

        private void StartRoomSearch()
        {
            ResolveNetworkManager();
            if (networkManager == null)
            {
                SetRoomListText("NetworkManager 未配置");
                return;
            }

            networkManager.SearchLanRooms(HandleRoomFound);
        }

        private void StopRoomSearch()
        {
            if (networkManager != null)
            {
                networkManager.StopLanSearch(HandleRoomFound);
            }
        }

        private void ClearDiscoveredRooms()
        {
            firstDiscoveredRoom = default;
            secondDiscoveredRoom = default;
            selectedDiscoveredRoom = default;
            hasFirstDiscoveredRoom = false;
            hasSecondDiscoveredRoom = false;
            hasSelectedDiscoveredRoom = false;
        }

        private void HandleRoomFound(QuoridorLanRoomInfo roomInfo)
        {
            if (!hasFirstDiscoveredRoom || firstDiscoveredRoom.ServerId == roomInfo.ServerId)
            {
                firstDiscoveredRoom = roomInfo;
                hasFirstDiscoveredRoom = true;
            }
            else if (!hasSecondDiscoveredRoom || secondDiscoveredRoom.ServerId == roomInfo.ServerId)
            {
                secondDiscoveredRoom = roomInfo;
                hasSecondDiscoveredRoom = true;
            }

            if (!hasSelectedDiscoveredRoom)
            {
                SelectDiscoveredRoom(roomInfo, true);
                return;
            }

            RefreshRoomList();
        }

        private void RefreshRoomList()
        {
            SetRoomListText(BuildRoomListText());
            SetRoomListButton(roomListFirstRoomButton, BuildRoomButtonLabel(firstDiscoveredRoom, hasFirstDiscoveredRoom), hasFirstDiscoveredRoom);
            SetRoomListButton(roomListSecondRoomButton, BuildRoomButtonLabel(secondDiscoveredRoom, hasSecondDiscoveredRoom), hasSecondDiscoveredRoom);
            RefreshRoomInfo();
        }

        private string BuildRoomListText()
        {
            if (!hasFirstDiscoveredRoom && !hasSecondDiscoveredRoom)
            {
                return "正在搜索局域网房间...";
            }

            string first = hasFirstDiscoveredRoom ? firstDiscoveredRoom.DisplayLabel : string.Empty;
            string second = hasSecondDiscoveredRoom ? secondDiscoveredRoom.DisplayLabel : string.Empty;
            return string.IsNullOrWhiteSpace(second) ? first : $"{first}\n{second}";
        }

        private string BuildRoomButtonLabel(QuoridorLanRoomInfo roomInfo, bool hasRoom)
        {
            if (!hasRoom)
            {
                return hasFirstDiscoveredRoom ? "继续搜索..." : "未发现房间";
            }

            string prefix = hasSelectedDiscoveredRoom && selectedDiscoveredRoom.ServerId == roomInfo.ServerId ? "✓ " : string.Empty;
            return $"{prefix}{roomInfo.DisplayLabel}";
        }

        private void SelectDiscoveredRoom(QuoridorLanRoomInfo roomInfo, bool hasRoom)
        {
            if (!hasRoom)
            {
                SetStatus("还没有发现可加入的房间");
                return;
            }

            selectedDiscoveredRoom = roomInfo;
            hasSelectedDiscoveredRoom = true;
            RefreshRoomList();
            SetStatus($"已选择 {roomInfo.RoomName}");
        }

        private void SetRoomListText(string value)
        {
            if (roomListText != null)
            {
                roomListText.text = value;
            }
        }

        private void RefreshRoomInfo()
        {
            if (roomInfoText == null)
            {
                return;
            }

            if (!hasSelectedDiscoveredRoom)
            {
                roomInfoText.text = "房主：-\n人数：- / -\n状态：搜索中";
                return;
            }

            string status = selectedDiscoveredRoom.PlayerCount >= selectedDiscoveredRoom.MaxPlayers ? "已满" : "等待中";
            roomInfoText.text = $"房主：{selectedDiscoveredRoom.RoomName}\n人数：{selectedDiscoveredRoom.PlayerCount} / {selectedDiscoveredRoom.MaxPlayers}\n状态：{status}";
        }

        private static void SetRoomListButton(Button button, string label, bool interactable)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = interactable;
            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        private void JoinDiscoveredRoom(QuoridorLanRoomInfo roomInfo, bool hasRoom)
        {
            if (!hasRoom)
            {
                SetStatus("还没有发现可加入的房间");
                return;
            }

            ResolveNetworkManager();
            if (networkManager == null)
            {
                SetStatus("NetworkManager 未配置");
                return;
            }

            StopRoomSearch();
            activeRoomName = roomInfo.RoomName;
            ShowRoom(roomInfo.RoomName);
            activeRoomIsNetworked = true;
            networkManager.JoinLanRoom(roomInfo, selectedCharacterId);
            RefreshRoomState();
            SetStatus("正在加入房间");
        }

        private void RefreshNetworkRoomState()
        {
            bool canStart = networkManager != null && networkManager.CanStartMatch;
            bool isHost = networkManager != null && networkManager.IsHostActive;
            if (roomWaitingText != null)
            {
                if (networkManager == null)
                {
                    roomWaitingText.text = "NetworkManager 未配置";
                }
                else if (canStart)
                {
                    roomWaitingText.text = "第二位玩家已进入\n房主可以开始游戏";
                }
                else if (isHost)
                {
                    roomWaitingText.text = "你已创建房间\n等待第二位玩家进入...";
                }
                else
                {
                    roomWaitingText.text = networkManager.IsClientConnected
                        ? "已加入房间\n等待房主开始..."
                        : "正在连接房间...";
                }
            }

            if (startLocalFromRoomButton != null)
            {
                startLocalFromRoomButton.interactable = canStart;
            }
        }

        private void StartNetworkGame()
        {
            if (networkManager == null || !networkManager.CanStartMatch)
            {
                SetStatus("等待第二位玩家进入后才能开始");
                return;
            }

            networkManager.StartNetworkMatch();
            SetStatus("正在进入游戏");
        }

        private void HandleNetworkRoomStateChanged()
        {
            if (roomPanel != null && roomPanel.activeSelf)
            {
                RefreshRoomState();
            }
        }

        private QuoridorRoomPlayer GetLocalRoomPlayer()
        {
            QuoridorRoomPlayer[] players = FindObjectsByType<QuoridorRoomPlayer>(FindObjectsSortMode.None);
            foreach (QuoridorRoomPlayer player in players)
            {
                if (player != null && player.IsLocalRoomPlayer)
                {
                    return player;
                }
            }

            return null;
        }

        private void ResolveNetworkManager()
        {
            if (networkManager == null)
            {
                networkManager = FindFirstObjectByType<QuoridorNetworkManager>();
            }
        }

        private void EnsureDefaultCharacterSelection()
        {
            if (!string.IsNullOrWhiteSpace(selectedCharacterId))
            {
                return;
            }

            CharacterVisualDefinition defaultCharacter = characterCatalog != null
                ? characterCatalog.GetDefault(PlayerId.PlayerOne)
                : null;
            if (defaultCharacter == null)
            {
                selectedCharacterName = "Not selected";
                return;
            }

            selectedCharacterId = defaultCharacter.CharacterId;
            selectedCharacterName = defaultCharacter.DisplayName;
        }

        private void ApplyLocalCharacterSelection()
        {
            string playerOneId = string.IsNullOrWhiteSpace(selectedCharacterId)
                ? GetDefaultCharacterId(PlayerId.PlayerOne)
                : selectedCharacterId;
            string playerTwoId = GetDefaultCharacterId(PlayerId.PlayerTwo);

            if (!string.IsNullOrWhiteSpace(playerOneId) && playerOneId == playerTwoId)
            {
                playerTwoId = GetFallbackOpponentId(playerOneId);
            }

            LocalPlayerSelection.SetCharacter(PlayerId.PlayerOne, playerOneId);
            LocalPlayerSelection.SetCharacter(PlayerId.PlayerTwo, playerTwoId);
        }

        private string ResolveCharacterId(string characterName)
        {
            CharacterVisualDefinition character = characterCatalog != null
                ? characterCatalog.GetByDisplayName(characterName)
                : null;
            return character != null ? character.CharacterId : string.Empty;
        }

        private string GetDefaultCharacterId(PlayerId playerId)
        {
            if (characterCatalog == null)
            {
                return string.Empty;
            }

            return playerId == PlayerId.PlayerOne
                ? characterCatalog.DefaultPlayerOneCharacterId
                : characterCatalog.DefaultPlayerTwoCharacterId;
        }

        private string GetFallbackOpponentId(string unavailableCharacterId)
        {
            if (characterCatalog == null)
            {
                return string.Empty;
            }

            foreach (CharacterVisualDefinition character in characterCatalog.Characters)
            {
                if (character != null && character.CharacterId != unavailableCharacterId)
                {
                    return character.CharacterId;
                }
            }

            return unavailableCharacterId;
        }

        private void ApplyReadableFont()
        {
            Font font = Font.CreateDynamicFontFromOSFont(PreferredFontNames, 24);
            if (font == null)
            {
                return;
            }

            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                text.font = font;
            }
        }
    }
}
