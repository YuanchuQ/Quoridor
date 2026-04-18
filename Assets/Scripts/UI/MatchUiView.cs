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
        [SerializeField] private Text wallText;
        [SerializeField] private Text orientationText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text winnerText;

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

            if (wallText != null)
            {
                wallText.text = $"Walls  P1: {playerOneWalls}   P2: {playerTwoWalls}";
            }

            if (orientationText != null)
            {
                orientationText.text = $"Wall: {wallOrientation}";
            }

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
    }
}
