using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;
    public TurnManager turnManager;

    [Header("UI Promoción")]
    public GameObject promotionUI;

    private BoardModel logicalBoard;
    private LogicalPiece selectedPiece = null;
    private int selectedX = -1;
    private int selectedY = -1;

    private bool isWhitePlayer;
    private bool isVsComputer;

    // --- NUEVO: Estado de Promoción ---
    private bool isWaitingForPromotion = false;
    private LogicalPiece pieceToPromote = null;
    private int promoX = -1;
    private int promoY = -1;

    private void Start()
    {
        isWhitePlayer = boardView.matchConfig.isPlayingWhite;
        isVsComputer = boardView.matchConfig.isVsComputer;

        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();
        boardView.InitializeView(logicalBoard);

        if (promotionUI != null) promotionUI.SetActive(false);

        turnManager.StartMatch();
    }

    private void Update()
    {
        // Bloqueamos el clic en el tablero si el juego terminó o si estamos esperando a que elijas pieza
        if (Input.GetMouseButtonDown(0) && IsHumanTurn() && !isWaitingForPromotion)
        {
            HandleClick();
        }
    }

    private bool IsHumanTurn()
    {
        if (turnManager.isGameOver) return false;
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
            selectedPiece.hasMoved = true;

            ProcessCastlingRook(selectedX, selectedY, targetX);

            // ==========================================
            // --- NUEVO: EJECUCIÓN PEÓN AL PASO ---
            // ==========================================
            bool isEnPassant = selectedPiece.type == PieceType.Pawn && selectedX != targetX && logicalBoard.grid[targetX, targetY] == null;
            if (isEnPassant)
            {
                // Destruimos lógica y visualmente al peón que estaba a nuestro lado
                logicalBoard.grid[targetX, selectedY] = null;
                boardView.DestroyVisualPiece(targetX, selectedY);
                Debug.Log("¡Captura al paso!");
            }

            // --- NUEVO: ANOTAR SALTO DOBLE PARA EL SIGUIENTE TURNO ---
            if (selectedPiece.type == PieceType.Pawn && Mathf.Abs(targetY - selectedY) == 2)
            {
                logicalBoard.lastDoublePawnPush = new Vector2Int(targetX, targetY);
            }
            else
            {
                // Si movemos cualquier otra cosa, el efecto "En Passant" caduca (se borra la memoria)
                logicalBoard.lastDoublePawnPush = new Vector2Int(-1, -1);
            }
            // ==========================================

            logicalBoard.grid[targetX, targetY] = selectedPiece;
            logicalBoard.grid[selectedX, selectedY] = null;

            boardView.UpdateVisualPiece(selectedX, selectedY, targetX, targetY, selectedPiece);

            if (CheckPromotion(selectedPiece, targetY))
            {
                isWaitingForPromotion = true;
                pieceToPromote = selectedPiece;
                promoX = targetX;
                promoY = targetY;
                promotionUI.SetActive(true);
            }
            else
            {
                CheckGameEndAndPassTurn();
            }
        }

        selectedPiece = null;
        selectedX = -1;
        selectedY = -1;
        boardView.ResetAllSquareColors();
    }
    // --- LA FUNCIÓN LIMPIA DE COMPROBACIÓN ---
    private bool CheckPromotion(LogicalPiece piece, int targetY)
    {
        if (piece.type == PieceType.Pawn)
        {
            int promotionRow = (piece.team == TeamColor.White) ? 7 : 0;
            return targetY == promotionRow;
        }
        return false;
    }

    // --- LA FUNCIÓN QUE LLAMAN LOS BOTONES DE LA UI ---
    // Recibimos un string ("Queen", "Rook"...) para que sea súper fácil configurarlo en los botones de Unity
    public void CompletePromotion(string pieceTypeString)
    {
        PieceType chosenType = PieceType.Queen; // Por defecto

        switch (pieceTypeString)
        {
            case "Queen": chosenType = PieceType.Queen; break;
            case "Rook": chosenType = PieceType.Rook; break;
            case "Bishop": chosenType = PieceType.Bishop; break;
            case "Knight": chosenType = PieceType.Knight; break;
        }

        // 1. Mutamos la pieza
        pieceToPromote.type = chosenType;

        // 2. Le pedimos a la Vista que le ponga el disfraz de la nueva pieza (mismo origen y destino)
        boardView.UpdateVisualPiece(promoX, promoY, promoX, promoY, pieceToPromote);

        // 3. Apagamos la UI y quitamos la pausa
        promotionUI.SetActive(false);
        isWaitingForPromotion = false;
        pieceToPromote = null;

        // 4. Ahora sí, el movimiento ha terminado: verificamos jaques y pasamos turno
        CheckGameEndAndPassTurn();
    }

    // --- EL VEREDICTO FINAL EXTRAÍDO ---
    private void CheckGameEndAndPassTurn()
    {
        TeamColor nextColor = (turnManager.currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;
        bool enemyHasMoves = MovementLogic.HasAnyValidMove(logicalBoard, nextColor);

        if (!enemyHasMoves)
        {
            if (MovementLogic.IsKingInCheck(logicalBoard, nextColor))
                turnManager.DeclareCheckmate(turnManager.currentTurn);
            else
                turnManager.DeclareStalemate();
        }
        else
        {
            turnManager.PassTurn();
        }
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

    // --- LÓGICA DE ENROQUE VISUAL Y LÓGICO ---
    private void ProcessCastlingRook(int startX, int startY, int targetX)
    {
        // Sabemos que es un enroque si movemos un Rey y el salto es de 2 casillas (Mathf.Abs calcula la distancia absoluta)
        bool isCastling = selectedPiece.type == PieceType.King && Mathf.Abs(startX - targetX) == 2;
        if (!isCastling) return;

        // Si salta a la derecha, la torre está en 7. Si salta a la izquierda, la torre está en 0.
        int rookStartX = (targetX > startX) ? 7 : 0;

        // La torre aterriza al lado opuesto del Rey (El Rey va al 6, la Torre al 5)
        int rookTargetX = (targetX > startX) ? targetX - 1 : targetX + 1;

        LogicalPiece rook = logicalBoard.grid[rookStartX, startY];
        rook.hasMoved = true;

        // Movemos la Torre lógicamente
        logicalBoard.grid[rookTargetX, startY] = rook;
        logicalBoard.grid[rookStartX, startY] = null;

        // Movemos la Torre visualmente (Sin promoción, así que el disfraz es el mismo)
        boardView.UpdateVisualPiece(rookStartX, startY, rookTargetX, startY, rook);

        Debug.Log("¡Enroque ejecutado!");
    }
}