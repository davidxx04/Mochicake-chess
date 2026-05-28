using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;

    private BoardModel logicalBoard;

    private LogicalPiece selectedPiece = null;
    private int selectedX = -1;
    private int selectedY = -1;

    // El "dibujado" de la cámara (perspectiva fija)
    private bool isWhitePlayer;

    // NUEVO: La memoria del turno actual
    private TeamColor currentTurn = TeamColor.White;

    private void Start()
    {
        isWhitePlayer = boardView.matchConfig.isPlayingWhite;
        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();
        boardView.InitializeView(logicalBoard);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (TryGetClickedSquare(out int logicalX, out int logicalY))
        {
            LogicalPiece clickedPiece = logicalBoard.grid[logicalX, logicalY];

            // EL CAMBIO CLAVE: Ya no miramos nuestro color fijo, miramos de quién es el turno
            ProcessSelectionAndMove(clickedPiece, logicalX, logicalY, currentTurn);
        }
    }

    private void ProcessSelectionAndMove(LogicalPiece clickedPiece, int logicalX, int logicalY, TeamColor activeColor)
    {
        // 1. Tocar una pieza del equipo al que le toca jugar
        if (clickedPiece != null && clickedPiece.team == activeColor)
        {
            SelectPiece(clickedPiece, logicalX, logicalY);
        }
        // 2. Intentar mover la pieza seleccionada
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

            // NUEVO: ¡La jugada fue un éxito! Pasamos el turno al rival.
            currentTurn = (currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;
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