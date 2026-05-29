using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;
    public TurnManager turnManager;

    private BoardModel logicalBoard;

    private LogicalPiece selectedPiece = null;
    private int selectedX = -1;
    private int selectedY = -1;

    private bool isWhitePlayer;
    private bool isVsComputer;

    private void Start()
    {
        isWhitePlayer = boardView.matchConfig.isPlayingWhite;
        isVsComputer = boardView.matchConfig.isVsComputer;

        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();
        boardView.InitializeView(logicalBoard);

        turnManager.StartMatch();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsHumanTurn())
        {
            HandleClick();
        }
    }

    private bool IsHumanTurn()
    {
        // 1. Si el juego ya ha terminado, bloqueamos los clics
        if (turnManager.isGameOver) return false;

        // 2. Comprobaciones de IA (Lo que ya teníamos)
        if (!isVsComputer) return true;

        TeamColor myColor = isWhitePlayer ? TeamColor.White : TeamColor.Black;
        return turnManager.currentTurn == myColor;
    }

    private void HandleClick()
    {
        if (TryGetClickedSquare(out int logicalX, out int logicalY))
        {
            LogicalPiece clickedPiece = logicalBoard.grid[logicalX, logicalY];
            ProcessSelectionAndMove(clickedPiece, logicalX, logicalY, turnManager.currentTurn);
        }
    }

    private void ProcessSelectionAndMove(LogicalPiece clickedPiece, int logicalX, int logicalY, TeamColor activeColor)
    {
        if (clickedPiece != null && clickedPiece.team == activeColor)
        {
            SelectPiece(clickedPiece, logicalX, logicalY);
        }
        else if (selectedPiece != null)
        {
            TryExecuteMove(logicalX, logicalY);
        }
    }

    private void SelectPiece(LogicalPiece piece, int x, int y)
    {
        boardView.ResetAllSquareColors();
        selectedPiece = piece;
        selectedX = x;
        selectedY = y;

        boardView.HighlightSquare(x, y);

        List<Vector2Int> validMoves = MovementLogic.GetValidMoves(logicalBoard, x, y);
        boardView.HighlightValidMoves(validMoves);
    }

    private void TryExecuteMove(int targetX, int targetY)
    {
        List<Vector2Int> validMoves = MovementLogic.GetValidMoves(logicalBoard, selectedX, selectedY);
        Vector2Int targetMove = new Vector2Int(targetX, targetY);

        if (validMoves.Contains(targetMove))
        {
            // ==========================================
            // --- NUEVO: MEMORIA Y PROMOCIÓN ---
            // ==========================================
            selectedPiece.hasMoved = true; // El Árbitro anota que esta pieza ya no es virgen

            if (selectedPiece.type == PieceType.Pawn)
            {
                int promotionRow = (selectedPiece.team == TeamColor.White) ? 7 : 0;
                if (targetY == promotionRow)
                {
                    selectedPiece.type = PieceType.Queen; // ¡Mutación a Reina!
                    Debug.Log("¡Peón coronado a Reina!");
                }
            }
            // ==========================================

            logicalBoard.grid[targetX, targetY] = selectedPiece;
            logicalBoard.grid[selectedX, selectedY] = null;

            // FÍJATE AQUÍ: Le pasamos 'selectedPiece' al final de la función
            boardView.UpdateVisualPiece(selectedX, selectedY, targetX, targetY, selectedPiece);

            // ... (Aquí sigue tu código de EL VEREDICTO FINAL)

            // ==========================================
            // --- EL VEREDICTO FINAL ---
            // ==========================================

            TeamColor nextColor = (turnManager.currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;

            // Preguntamos al Cerebro: ¿El SIGUIENTE jugador tiene algún movimiento legal?
            bool enemyHasMoves = MovementLogic.HasAnyValidMove(logicalBoard, nextColor);

            if (!enemyHasMoves)
            {
                // El enemigo no puede moverse. ¿Es porque está en Jaque?
                if (MovementLogic.IsKingInCheck(logicalBoard, nextColor))
                {
                    // Jaque Mate: Gana el turno actual
                    turnManager.DeclareCheckmate(turnManager.currentTurn);
                }
                else
                {
                    // Rey Ahogado: Nadie gana
                    turnManager.DeclareStalemate();
                }
            }
            else
            {
                // El enemigo puede moverse. La partida continúa normalmente.
                turnManager.PassTurn();
            }
            // ==========================================
        }

        selectedPiece = null;
        selectedX = -1;
        selectedY = -1;
        boardView.ResetAllSquareColors();
    }

    private bool TryGetClickedSquare(out int logicalX, out int logicalY)
    {
        logicalX = -1;
        logicalY = -1;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            float clickedX = hit.collider.transform.position.x;
            float clickedY = hit.collider.transform.position.y;

            int visualX = Mathf.RoundToInt(clickedX + 3.5f);
            int visualY = Mathf.RoundToInt(clickedY + 3.5f);

            logicalX = isWhitePlayer ? visualX : 7 - visualX;
            logicalY = isWhitePlayer ? visualY : 7 - visualY;
            return true;
        }

        return false;
    }
}