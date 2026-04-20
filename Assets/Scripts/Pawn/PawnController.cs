using System;
using System.Collections.Generic;
using Quoridor.Board;
using Quoridor.Config;
using Quoridor.Core;
using Quoridor.Input;
using UnityEngine;

namespace Quoridor.Pawn
{
    /// <summary>
    /// Coordinates local pawn movement input and legal move highlighting.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PawnController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private BoardView boardView;
        [SerializeField] private InputRouter inputRouter;
        [SerializeField] private PawnView playerOnePawn;
        [SerializeField] private PawnView playerTwoPawn;

        private readonly List<BoardPosition> legalMoves = new();
        private BoardGraph boardGraph;
        private PlayerId activePlayer = PlayerId.PlayerOne;
        private bool inputEnabled = true;

        /// <summary>
        /// Raised after a pawn successfully moves.
        /// </summary>
        public event Action<PawnMoveEvent> PawnMoved;

        /// <summary>
        /// Player whose pawn currently accepts movement input.
        /// </summary>
        public PlayerId ActivePlayer => activePlayer;

        /// <summary>
        /// Current position of player one's pawn.
        /// </summary>
        public BoardPosition PlayerOnePosition => playerOnePawn != null ? playerOnePawn.Position : GetStartPosition(PlayerId.PlayerOne);

        /// <summary>
        /// Current position of player two's pawn.
        /// </summary>
        public BoardPosition PlayerTwoPosition => playerTwoPawn != null ? playerTwoPawn.Position : GetStartPosition(PlayerId.PlayerTwo);

        /// <summary>
        /// True when pawn movement input is accepted.
        /// </summary>
        public bool InputEnabled => inputEnabled;

        /// <summary>
        /// Reinitializes the pawn controller with current inspector references.
        /// </summary>
        public void ResetMatch()
        {
            int boardSize = config != null ? config.BoardSize : QuoridorRules.BoardSize;
            float moveDuration = config != null ? config.PawnMoveDuration : 0.18f;

            boardGraph = BoardGraph.CreateOpenBoard(boardSize);
            activePlayer = PlayerId.PlayerOne;

            if (playerOnePawn != null)
            {
                playerOnePawn.Configure(PlayerId.PlayerOne, boardView, GetStartPosition(PlayerId.PlayerOne), moveDuration);
            }

            if (playerTwoPawn != null)
            {
                playerTwoPawn.Configure(PlayerId.PlayerTwo, boardView, GetStartPosition(PlayerId.PlayerTwo), moveDuration);
            }

            RefreshMoveHints();
        }

        /// <summary>
        /// Attempts to move the active player's pawn to the requested board position.
        /// </summary>
        public bool TryMoveActivePawn(BoardPosition destination)
        {
            if (!inputEnabled)
            {
                return false;
            }

            PawnView activePawn = GetPawn(activePlayer);
            PawnView opponentPawn = GetOpponentPawn(activePlayer);

            if (activePawn == null || opponentPawn == null || AnyPawnMoving())
            {
                return false;
            }

            RefreshLegalMoves(activePawn.Position, opponentPawn.Position);
            if (!legalMoves.Contains(destination))
            {
                return false;
            }

            BoardPosition from = activePawn.Position;
            activePawn.MoveTo(destination);
            PawnMoved?.Invoke(new PawnMoveEvent(activePlayer, from, destination));
            RefreshMoveHints();
            return true;
        }

        /// <summary>
        /// Replaces the board graph used for pawn movement validation.
        /// </summary>
        public void SetBoardGraph(BoardGraph nextBoardGraph)
        {
            boardGraph = nextBoardGraph ?? throw new ArgumentNullException(nameof(nextBoardGraph));
            RefreshMoveHints();
        }

        /// <summary>
        /// Changes which player's pawn accepts movement input.
        /// </summary>
        public void SetActivePlayer(PlayerId playerId)
        {
            activePlayer = playerId;
            RefreshMoveHints();
        }

        /// <summary>
        /// Enables or disables pawn movement input.
        /// </summary>
        public void SetInputEnabled(bool isEnabled)
        {
            inputEnabled = isEnabled;

            if (!inputEnabled && boardView != null)
            {
                boardView.ClearHighlights();
            }
            else
            {
                RefreshMoveHints();
            }
        }

        private void Awake()
        {
            ResetMatch();
        }

        private void OnEnable()
        {
            if (inputRouter != null)
            {
                inputRouter.BoardCellInput += HandleBoardCellInput;
            }
        }

        private void OnDisable()
        {
            if (inputRouter != null)
            {
                inputRouter.BoardCellInput -= HandleBoardCellInput;
            }
        }

        private void HandleBoardCellInput(BoardCellInputEvent inputEvent)
        {
            if (!inputEnabled)
            {
                return;
            }

            if (inputEvent.Mode != InputMode.PawnMove || inputEvent.Phase != BoardCellInputPhase.Selected)
            {
                return;
            }

            TryMoveActivePawn(inputEvent.Position);
        }

        private void RefreshMoveHints()
        {
            if (boardView == null)
            {
                return;
            }

            boardView.ClearHighlights();
            PawnView activePawn = GetPawn(activePlayer);
            PawnView opponentPawn = GetOpponentPawn(activePlayer);
            if (activePawn == null || opponentPawn == null)
            {
                return;
            }

            RefreshLegalMoves(activePawn.Position, opponentPawn.Position);
            foreach (BoardPosition move in legalMoves)
            {
                if (boardView.TryGetCell(move, out CellView cell))
                {
                    cell.SetMoveHint();
                }
            }
        }

        private void RefreshLegalMoves(BoardPosition currentPosition, BoardPosition opponentPosition)
        {
            if (boardGraph == null)
            {
                int boardSize = config != null ? config.BoardSize : QuoridorRules.BoardSize;
                boardGraph = BoardGraph.CreateOpenBoard(boardSize);
            }

            legalMoves.Clear();
            legalMoves.AddRange(PawnMovementRules.GetLegalMoves(boardGraph, currentPosition, opponentPosition));
        }

        private PawnView GetPawn(PlayerId playerId)
        {
            return playerId == PlayerId.PlayerOne ? playerOnePawn : playerTwoPawn;
        }

        private PawnView GetOpponentPawn(PlayerId playerId)
        {
            return playerId == PlayerId.PlayerOne ? playerTwoPawn : playerOnePawn;
        }

        private bool AnyPawnMoving()
        {
            return (playerOnePawn != null && playerOnePawn.IsMoving)
                || (playerTwoPawn != null && playerTwoPawn.IsMoving);
        }

        private BoardPosition GetStartPosition(PlayerId playerId)
        {
            if (config == null)
            {
                return playerId == PlayerId.PlayerOne ? QuoridorRules.PlayerOneStart : QuoridorRules.PlayerTwoStart;
            }

            Vector2Int start = playerId == PlayerId.PlayerOne ? config.PlayerOneStart : config.PlayerTwoStart;
            return new BoardPosition(start.x, start.y);
        }
    }
}
