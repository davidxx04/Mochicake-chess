using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;

    private BoardModel logicalBoard;

    // --- Memoria del Árbitro ---
    private LogicalPiece selectedPiece = null;
    private int selectedX = -1;
    private int selectedY = -1;

    private void Start()
    {
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
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            float clickedX = hit.collider.transform.position.x;
            float clickedY = hit.collider.transform.position.y;

            int visualX = Mathf.RoundToInt(clickedX + 3.5f);
            int visualY = Mathf.RoundToInt(clickedY + 3.5f);

            bool isWhite = boardView.matchConfig.isPlayingWhite;
            int logicalX = isWhite ? visualX : 7 - visualX;
            int logicalY = isWhite ? visualY : 7 - visualY;

            LogicalPiece clickedPiece = logicalBoard.grid[logicalX, logicalY];
            TeamColor myColor = isWhite ? TeamColor.White : TeamColor.Black;

            // Llamamos a nuestra nueva función ultra-limpia
            ProcessSelectionAndMove(clickedPiece, logicalX, logicalY, myColor);
        }
    }

    // --- LA MEJORA QUE HAS SUGERIDO ---
    private void ProcessSelectionAndMove(LogicalPiece clickedPiece, int logicalX, int logicalY, TeamColor myColor)
    {
        // 1. Toco una pieza de mi equipo -> Seleccionar (da igual si tenía otra antes)
        if (clickedPiece != null && clickedPiece.team == myColor)
        {
            boardView.ResetAllSquareColors(); // Limpiamos por si había algo seleccionado antes
            SelectPiece(clickedPiece, logicalX, logicalY);
        }
        // 2. Toco otra cosa Y ADEMÁS tengo una pieza en la mano -> Intentar mover
        else if (selectedPiece != null)
        {
            Debug.Log($"¡Moviendo {selectedPiece.type} de [{selectedX}, {selectedY}] a [{logicalX}, {logicalY}]!");

            // TODO: Validación real y ejecución del movimiento

            selectedPiece = null;
            selectedX = -1;
            selectedY = -1;
            boardView.ResetAllSquareColors();
        }
        // 3. (Implícito) Si toco una casilla vacía o enemiga pero NO tengo pieza seleccionada... no hace nada.
    }

    private void SelectPiece(LogicalPiece piece, int x, int y)
    {
        selectedPiece = piece;
        selectedX = x;
        selectedY = y;

        boardView.HighlightSquare(x, y);

        List<Vector2Int> validMoves = GetValidMoves(piece, x, y);
        boardView.HighlightValidMoves(validMoves);
    }

    private List<Vector2Int> GetValidMoves(LogicalPiece piece, int currentX, int currentY)
    {
        return MovementLogic.GetValidMoves(logicalBoard, currentX, currentY);
    }
}