using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;
    public TurnManager turnManager; // <-- Conectamos al relojero

    private BoardModel logicalBoard;

    private LogicalPiece selectedPiece = null;
    private int selectedX = -1;
    private int selectedY = -1;

    // Variables cacheadas al inicio (Tu mejora aplicada aquí)
    private bool isWhitePlayer;
    private bool isVsComputer;

    private void Start()
    {
        // 1. Guardamos la configuración una sola vez
        isWhitePlayer = boardView.matchConfig.isPlayingWhite;
        isVsComputer = boardView.matchConfig.isVsComputer;

        // 2. Inicializamos el tablero
        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();
        boardView.InitializeView(logicalBoard);

        // 3. Arrancamos los turnos usando el manager externo
        turnManager.StartMatch();
    }

    private void Update()
    {
        // Consultamos la función protectora antes de procesar clics
        if (Input.GetMouseButtonDown(0) && IsHumanTurn())
        {
            HandleClick();
        }
    }

    // --- PROTECCIÓN DE INTERACCIÓN ---
    private bool IsHumanTurn()
    {
        // Si no jugamos contra la máquina (Pass & Play local), siempre devolvemos true
        if (!isVsComputer)
            return true;

        // Si jugamos contra la máquina, solo devolvemos true si nos toca a nosotros
        TeamColor myColor = isWhitePlayer ? TeamColor.White : TeamColor.Black;
        return turnManager.currentTurn == myColor;
    }

    private void HandleClick()
    {
        if (TryGetClickedSquare(out int logicalX, out int logicalY))
        {
            LogicalPiece clickedPiece = logicalBoard.grid[logicalX, logicalY];

            // Pasamos el turno que nos dicta el TurnManager
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
            logicalBoard.grid[targetX, targetY] = selectedPiece;
            logicalBoard.grid[selectedX, selectedY] = null;

            boardView.UpdateVisualPiece(selectedX, selectedY, targetX, targetY);

            // La jugada es válida, ordenamos al Manager que cambie el turno
            turnManager.PassTurn();
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