using System;
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
        [SerializeField] private Text turnText;
        [SerializeField] private Text modeText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text wallText;
        [SerializeField] private Text orientationText;
        [SerializeField] private Text playerOneNameText;
        [SerializeField] private Text playerOnePawnCountText;
        [SerializeField] private Text playerOneWallCountText;
        [SerializeField] private Text playerOneStatusText;
        [SerializeField] private Text playerTwoNameText;
        [SerializeField] private Text playerTwoPawnCountText;
        [SerializeField] private Text playerTwoWallCountText;
        [SerializeField] private Text playerTwoStatusText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text winnerText;
        [SerializeField] private Button restartButton;

        private const string PlayerOneName = "优衣";
        private const string PlayerTwoName = "凯露";
        private const int PawnCount = 1;

        /// <summary>
        /// Raised when the player requests a fresh local match.
        /// </summary>
        public event Action RestartRequested;

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
                playerOnePawnCountText,
                playerOneWallCountText,
                playerOneStatusText,
                PlayerOneName,
                PawnCount,
                playerOneWalls,
                state,
                activePlayer,
                PlayerId.PlayerOne,
                winner);

            RefreshPlayerPanel(
                playerTwoNameText,
                playerTwoPawnCountText,
                playerTwoWallCountText,
                playerTwoStatusText,
                PlayerTwoName,
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

        private static string FormatPlayer(PlayerId playerId)
        {
            return playerId == PlayerId.PlayerOne ? "Player 1" : "Player 2";
        }

        private static void RefreshPlayerPanel(
            Text nameText,
            Text pawnCountText,
            Text wallCountText,
            Text statusText,
            string playerName,
            int pawnCount,
            int wallCount,
            GameState state,
            PlayerId activePlayer,
            PlayerId panelPlayer,
            PlayerId winner)
        {
            if (nameText != null)
            {
                nameText.text = playerName;
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
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }

        private void HandleRestartClicked()
        {
            RestartRequested?.Invoke();
        }
    }
}
