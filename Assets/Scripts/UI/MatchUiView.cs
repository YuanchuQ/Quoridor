// Scene-facing UI view for turn, wall counts, input mode, and victory state
using System;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Quoridor.UI
{
    /// <summary>
    /// Scene-facing UI view for turn, wall counts, input mode, and victory state.
    /// </summary>
    public sealed class MatchUiView : MonoBehaviour
    {
        [Tooltip("Text that shows the current turn")]
        [SerializeField] private Text turnText;
        [Tooltip("Text that shows the active input mode")]
        [SerializeField] private Text modeText;
        [Tooltip("Text that shows the current control hint")]
        [SerializeField] private Text hintText;
        [Tooltip("Text that shows remaining wall counts")]
        [SerializeField] private Text wallText;
        [Tooltip("Text that shows the current wall orientation")]
        [SerializeField] private Text orientationText;
        [Tooltip("Player one display name text")]
        [SerializeField] private Text playerOneNameText;
        [Tooltip("Player one secondary name text")]
        [SerializeField] private Text playerOneSubNameText;
        [Tooltip("Player one portrait image")]
        [SerializeField] private Image playerOnePortraitImage;
        [Tooltip("Player one pawn count text")]
        [SerializeField] private Text playerOnePawnCountText;
        [Tooltip("Player one wall count text")]
        [SerializeField] private Text playerOneWallCountText;
        [Tooltip("Player one turn status text")]
        [SerializeField] private Text playerOneStatusText;
        [Tooltip("Player two display name text")]
        [SerializeField] private Text playerTwoNameText;
        [Tooltip("Player two secondary name text")]
        [SerializeField] private Text playerTwoSubNameText;
        [Tooltip("Player two portrait image")]
        [SerializeField] private Image playerTwoPortraitImage;
        [Tooltip("Player two pawn count text")]
        [SerializeField] private Text playerTwoPawnCountText;
        [Tooltip("Player two wall count text")]
        [SerializeField] private Text playerTwoWallCountText;
        [Tooltip("Player two turn status text")]
        [SerializeField] private Text playerTwoStatusText;
        [Tooltip("Panel displayed when the match ends")]
        [SerializeField] private GameObject gameOverPanel;
        [Tooltip("Text that shows the winner")]
        [SerializeField] private Text winnerText;
        [Tooltip("Button that returns to the lobby after game over")]
        [SerializeField] private Button restartButton;

        private const string PlayerOneName = "优衣";
        private const string PlayerTwoName = "凯露";
        private const string PlayerOneLatinName = "YUI";
        private const string PlayerTwoLatinName = "KARU";
        private const int PawnCount = 1;

        private CharacterVisualDefinition playerOneCharacter;
        private CharacterVisualDefinition playerTwoCharacter;

        /// <summary>
        /// Raised when the player requests to leave the match and return to the lobby.
        /// </summary>
        public event Action ReturnToLobbyRequested;

        /// <summary>
        /// Refreshes the match HUD with current match data.
        /// </summary>
        public void Refresh(
            GameState state,
            PlayerId activePlayer,
            InputMode inputMode,
            WallOrientation wallOrientation,
            int playerOneWalls,
            int playerTwoWalls,
            PlayerId winner)
        {
            if (turnText != null)
            {
                turnText.text = state == GameState.GameOver ? "Game Over" : $"{FormatPlayer(activePlayer)} Turn";
            }

            if (modeText != null)
            {
                modeText.text = inputMode == InputMode.PawnMove ? "Mode: Move" : "Mode: Wall";
            }

            if (hintText != null)
            {
                hintText.text = inputMode == InputMode.PawnMove
                    ? "Click a highlighted cell to move   |   Tab: Place wall"
                    : "Hover a cell to preview   |   Click: Place wall   |   R: Rotate   |   Tab: Move";
            }

            if (wallText != null)
            {
                wallText.text = $"Walls  P1: {playerOneWalls}   P2: {playerTwoWalls}";
            }

            if (orientationText != null)
            {
                orientationText.text = $"Wall: {wallOrientation}";
            }

            RefreshPlayerPanel(
                playerOneNameText,
                playerOneSubNameText,
                playerOnePortraitImage,
                playerOnePawnCountText,
                playerOneWallCountText,
                playerOneStatusText,
                playerOneCharacter,
                PlayerOneName,
                PlayerOneLatinName,
                PawnCount,
                playerOneWalls,
                state,
                activePlayer,
                PlayerId.PlayerOne,
                winner);

            RefreshPlayerPanel(
                playerTwoNameText,
                playerTwoSubNameText,
                playerTwoPortraitImage,
                playerTwoPawnCountText,
                playerTwoWallCountText,
                playerTwoStatusText,
                playerTwoCharacter,
                PlayerTwoName,
                PlayerTwoLatinName,
                PawnCount,
                playerTwoWalls,
                state,
                activePlayer,
                PlayerId.PlayerTwo,
                winner);

            bool isGameOver = state == GameState.GameOver;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(isGameOver);
            }

            if (winnerText != null)
            {
                winnerText.text = isGameOver ? $"{FormatPlayer(winner)} Wins" : string.Empty;
            }
        }

        /// <summary>
        /// Applies character portraits and display names to both player HUD panels.
        /// </summary>
        public void SetPlayerCharacters(CharacterVisualDefinition playerOneCharacter, CharacterVisualDefinition playerTwoCharacter)
        {
            this.playerOneCharacter = playerOneCharacter;
            this.playerTwoCharacter = playerTwoCharacter;

            RefreshPlayerIdentity(
                playerOneNameText,
                playerOneSubNameText,
                playerOnePortraitImage,
                playerOneCharacter,
                PlayerOneName,
                PlayerOneLatinName);

            RefreshPlayerIdentity(
                playerTwoNameText,
                playerTwoSubNameText,
                playerTwoPortraitImage,
                playerTwoCharacter,
                PlayerTwoName,
                PlayerTwoLatinName);
        }

        private static string FormatPlayer(PlayerId playerId)
        {
            return playerId == PlayerId.PlayerOne ? "Player 1" : "Player 2";
        }

        private static void RefreshPlayerPanel(
            Text nameText,
            Text subNameText,
            Image portraitImage,
            Text pawnCountText,
            Text wallCountText,
            Text statusText,
            CharacterVisualDefinition character,
            string playerName,
            string latinName,
            int pawnCount,
            int wallCount,
            GameState state,
            PlayerId activePlayer,
            PlayerId panelPlayer,
            PlayerId winner)
        {
            if (nameText != null)
            {
                nameText.text = character != null ? character.DisplayName : playerName;
            }

            if (subNameText != null)
            {
                subNameText.text = character != null ? character.LatinName : latinName;
            }

            if (portraitImage != null && character != null && character.PortraitSprite != null)
            {
                portraitImage.sprite = character.PortraitSprite;
                portraitImage.enabled = true;
                portraitImage.preserveAspect = true;
            }

            if (pawnCountText != null)
            {
                pawnCountText.text = $"× {pawnCount}";
            }

            if (wallCountText != null)
            {
                wallCountText.text = $"× {wallCount}";
            }

            if (statusText != null)
            {
                statusText.text = FormatPanelStatus(state, activePlayer, panelPlayer, winner);
            }
        }

        private static void RefreshPlayerIdentity(
            Text nameText,
            Text subNameText,
            Image portraitImage,
            CharacterVisualDefinition character,
            string fallbackName,
            string fallbackLatinName)
        {
            if (nameText != null)
            {
                nameText.text = character != null ? character.DisplayName : fallbackName;
            }

            if (subNameText != null)
            {
                subNameText.text = character != null ? character.LatinName : fallbackLatinName;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = character != null ? character.PortraitSprite : null;
                portraitImage.enabled = portraitImage.sprite != null;
                portraitImage.preserveAspect = true;
            }
        }

        private static string FormatPanelStatus(GameState state, PlayerId activePlayer, PlayerId panelPlayer, PlayerId winner)
        {
            if (state == GameState.GameOver)
            {
                return winner == panelPlayer ? "胜利" : "败北";
            }

            return activePlayer == panelPlayer ? "你的回合" : "等待中";
        }

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleReturnToLobbyClicked);
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleReturnToLobbyClicked);
            }
        }

        private void HandleReturnToLobbyClicked()
        {
            ReturnToLobbyRequested?.Invoke();
        }
    }
}
