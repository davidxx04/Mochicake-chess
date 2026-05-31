using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;
    public TurnManager turnManager;
    public AIController aiController;

    [Header("UI Promocion")]
    public GameObject promotionUI;

    private BoardModel logicalBoard;
    private LogicalPiece selectedPiece = null;
    private int selectedX = -1;
    private int selectedY = -1;

    private bool isWhitePlayer;
    private bool isVsComputer;
    private bool isAiThinking;

    private bool isWaitingForPromotion = false;
    private LogicalPiece pieceToPromote = null;
    private int promoX = -1;
    private int promoY = -1;

    private readonly Dictionary<ulong, int> positionHistory = new Dictionary<ulong, int>();

    private void Awake()
    {
        if (aiController == null)
            aiController = GetComponent<AIController>();
    }

    private void Start()
    {
        isWhitePlayer = boardView.matchConfig.isPlayingWhite;
        isVsComputer = boardView.matchConfig.isVsComputer;

        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();
        boardView.InitializeView(logicalBoard);

        if (promotionUI != null) promotionUI.SetActive(false);

        turnManager.StartMatch();
        RecordCurrentPosition();
        TryStartAiTurn();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsHumanTurn() && !isWaitingForPromotion)
            HandleClick();
    }

    public bool IsVsComputerActive() => isVsComputer;

    public void SetAiThinking(bool thinking) => isAiThinking = thinking;

    public TeamColor GetAiTeam()
    {
        return isWhitePlayer ? TeamColor.Black : TeamColor.White;
    }

    public BoardModel CloneBoard() => logicalBoard.Clone();

    public IReadOnlyDictionary<ulong, int> GetPositionHistory() => positionHistory;

    public bool ShouldApplyAiMove(TeamColor aiColor)
    {
        return isVsComputer
            && !turnManager.isGameOver
            && turnManager.currentTurn == aiColor;
    }

    public void ApplyAiMove(SearchResult result)
    {
        if (!ShouldApplyAiMove(GetAiTeam()))
            return;

        List<Move> validMoves = MovementLogic.GetValidMoves(logicalBoard, result.fromX, result.fromY);
        Move chosen = validMoves.Find(m => m.MatchesDestination(result.toX, result.toY));

        if (chosen == null)
        {
            Debug.LogWarning($"[MatchController] IA eligi� ({result.fromX},{result.fromY})->({result.toX},{result.toY}) pero no coincide con un movimiento legal.");
            return;
        }

        bool requiresPromotion = chosen.Execute(logicalBoard, boardView);
        TryShowImmediatePopup(chosen);

        if (requiresPromotion)
        {
            chosen.pieceToMove.type = PieceType.Queen;
            boardView.UpdateVisualPiece(result.toX, result.toY, result.toX, result.toY, chosen.pieceToMove);
            TryShowPromotionPopup();
        }

        CheckGameEndAndPassTurn();
    }

    private bool IsHumanTurn()
    {
        if (turnManager.isGameOver) return false;
        if (isAiThinking) return false;
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
            SelectPiece(clickedPiece, logicalX, logicalY);
        else if (selectedPiece != null)
            TryExecuteMove(logicalX, logicalY);
    }

    private void SelectPiece(LogicalPiece piece, int x, int y)
    {
        boardView.ResetAllSquareColors();
        selectedPiece = piece;
        selectedX = x;
        selectedY = y;

        boardView.HighlightSquare(x, y);

        List<Move> validMoves = MovementLogic.GetValidMoves(logicalBoard, x, y);
        boardView.HighlightValidMoves(validMoves);
    }

    private void TryExecuteMove(int targetX, int targetY)
    {
        List<Move> validMoves = MovementLogic.GetValidMoves(logicalBoard, selectedX, selectedY);
        Move chosen = validMoves.Find(m => m.MatchesDestination(targetX, targetY));

        if (chosen != null)
        {
            bool requiresPromotion = chosen.Execute(logicalBoard, boardView);
            TryShowImmediatePopup(chosen);

            if (requiresPromotion)
            {
                isWaitingForPromotion = true;
                pieceToPromote = chosen.pieceToMove;
                promoX = chosen.targetX;
                promoY = chosen.targetY;
                promotionUI.SetActive(true);
            }
            else
            {
                CheckGameEndAndPassTurn();
            }
        }

        ClearSelection();
    }

    private void ClearSelection()
    {
        selectedPiece = null;
        selectedX = -1;
        selectedY = -1;
        boardView.ResetAllSquareColors();
    }

    private void TryShowImmediatePopup(Move chosen)
    {
        if (FXManager.Instance == null || chosen == null)
            return;

        if (chosen is CastlingMove)
            FXManager.Instance.ShowPopup(FXManager.PopupEvent.Castling);
        else if (chosen is EnPassantMove)
            FXManager.Instance.ShowPopup(FXManager.PopupEvent.EnPassant);
    }

    private void TryShowPromotionPopup()
    {
        if (FXManager.Instance != null)
            FXManager.Instance.ShowPopup(FXManager.PopupEvent.Promotion);
    }

    private void TryShowCheckPopup()
    {
        if (FXManager.Instance != null)
            FXManager.Instance.ShowPopup(FXManager.PopupEvent.Check);
    }

    public void CompletePromotion(string pieceTypeString)
    {
        PieceType chosenType = PieceType.Queen;

        switch (pieceTypeString)
        {
            case "Queen": chosenType = PieceType.Queen; break;
            case "Rook": chosenType = PieceType.Rook; break;
            case "Bishop": chosenType = PieceType.Bishop; break;
            case "Knight": chosenType = PieceType.Knight; break;
        }

        pieceToPromote.type = chosenType;
        boardView.UpdateVisualPiece(promoX, promoY, promoX, promoY, pieceToPromote);

        promotionUI.SetActive(false);
        isWaitingForPromotion = false;
        pieceToPromote = null;

        TryShowPromotionPopup();

        CheckGameEndAndPassTurn();
    }

    private void CheckGameEndAndPassTurn()
    {
        TeamColor nextColor = (turnManager.currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;
        bool enemyHasMoves = MovementLogic.HasAnyValidMove(logicalBoard, nextColor);
        bool isCheck = MovementLogic.IsKingInCheck(logicalBoard, nextColor);

        if (!enemyHasMoves)
        {
            if (isCheck)
                turnManager.DeclareCheckmate(turnManager.currentTurn);
            else
                turnManager.DeclareStalemate();
        }
        else
        {
            if (isCheck)
                TryShowCheckPopup();
            turnManager.PassTurn();
            RecordCurrentPosition();
            TryStartAiTurn();
        }
    }

    private void RecordCurrentPosition()
    {
        ulong hash = BoardHash.Compute(logicalBoard, turnManager.currentTurn);
        if (positionHistory.TryGetValue(hash, out int count))
            positionHistory[hash] = count + 1;
        else
            positionHistory[hash] = 1;
    }

    private void TryStartAiTurn()
    {
        if (!isVsComputer || turnManager.isGameOver || aiController == null)
            return;

        TeamColor aiColor = GetAiTeam();
        if (turnManager.currentTurn == aiColor)
            aiController.RequestMove(aiColor);
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
