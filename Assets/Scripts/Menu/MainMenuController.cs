using System;
using Quoridor.Config;
using Quoridor.Core;
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
        [SerializeField] private GameObject roomPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Text statusText;
        [SerializeField] private Text roomListText;
        [SerializeField] private Text roomTitleText;
        [SerializeField] private Text selectedCharacterText;
        [SerializeField] private CharacterVisualCatalog characterCatalog;
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
        [SerializeField] private Button roomBackButton;
        [SerializeField] private Button startLocalFromRoomButton;
        [SerializeField] private Button settingsBackButton;

        private const string DefaultStatus = "Ready";
        private const string OfflineRoomName = "Local Preview Room";
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
            BindButtons();
            ShowMain();
        }

        private void OnDestroy()
        {
            UnbindButtons();
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
            Add(lanBackButton, ShowTwoPlayer);
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
            Remove(lanBackButton, ShowTwoPlayer);
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
            ShowPanel(twoPlayerPanel);
            SetStatus("Choose a multiplayer mode");
        }

        private void ShowLan()
        {
            ShowPanel(lanPanel);
            if (roomListText != null)
            {
                roomListText.text = $"{OfflineRoomName}    1/2    LAN placeholder";
            }

            SetStatus("LAN room browser placeholder");
        }

        private void ShowSettings()
        {
            ShowPanel(settingsPanel);
            SetStatus("Settings placeholder");
        }

        private void ShowRoom(string roomName)
        {
            ShowPanel(roomPanel);
            if (roomTitleText != null)
            {
                roomTitleText.text = roomName;
            }

            if (string.IsNullOrEmpty(selectedCharacterName))
            {
                selectedCharacterName = "Not selected";
            }

            RefreshSelectedCharacter();
            SetStatus("Choose a character");
        }

        private void JoinRoom()
        {
            ShowRoom(OfflineRoomName);
        }

        private void CreateRoom()
        {
            ShowRoom("Created Room");
        }

        private void HandleSinglePlayer()
        {
            SetStatus("Single Player is reserved for a future AI opponent");
        }

        private void LoadLocalGame()
        {
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
                selectedCharacterText.text = $"Selected: {selectedCharacterName}";
            }
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
