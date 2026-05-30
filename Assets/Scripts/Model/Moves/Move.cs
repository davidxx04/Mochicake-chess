using UnityEngine;

public abstract class Move
{
    public int startX, startY, targetX, targetY;
    public LogicalPiece pieceToMove;

    protected LogicalPiece capturedAtTarget;
    protected Vector2Int savedEnPassantTarget;
    protected bool savedMoverHasMoved;
    protected bool savedCapturedHasMoved;
    protected bool hadCapturedPiece;

    public Move(LogicalPiece piece, int sX, int sY, int tX, int tY)
    {
        pieceToMove = piece;
        startX = sX;
        startY = sY;
        targetX = tX;
        targetY = tY;
    }

    public bool MatchesDestination(int x, int y) => targetX == x && targetY == y;

    public abstract void ApplyLogic(BoardModel board);
    public abstract void UndoLogic(BoardModel board);

    public abstract bool Execute(BoardModel board, BoardView view);

    protected void BeginApply(BoardModel board)
    {
        savedEnPassantTarget = board.lastDoublePawnPush;
        savedMoverHasMoved = pieceToMove.hasMoved;
        hadCapturedPiece = board.grid[targetX, targetY] != null;
        savedCapturedHasMoved = hadCapturedPiece && board.grid[targetX, targetY].hasMoved;
    }

    protected void FinishApply(BoardModel board)
    {
        pieceToMove.hasMoved = true;
        UpdateEnPassantTarget(board);
        MovePieceOnGrid(board);
    }

    protected void RestoreApplyState(BoardModel board)
    {
        RestorePieceOnGrid(board);
        pieceToMove.hasMoved = savedMoverHasMoved;
        board.lastDoublePawnPush = savedEnPassantTarget;
        if (hadCapturedPiece && capturedAtTarget != null)
            capturedAtTarget.hasMoved = savedCapturedHasMoved;
    }

    protected void MovePieceOnGrid(BoardModel board)
    {
        capturedAtTarget = board.grid[targetX, targetY];
        board.grid[targetX, targetY] = pieceToMove;
        board.grid[startX, startY] = null;
    }

    protected void RestorePieceOnGrid(BoardModel board)
    {
        board.grid[startX, startY] = pieceToMove;
        board.grid[targetX, targetY] = capturedAtTarget;
    }

    protected void UpdateEnPassantTarget(BoardModel board)
    {
        if (pieceToMove.type == PieceType.Pawn && Mathf.Abs(targetY - startY) == 2)
            board.lastDoublePawnPush = new Vector2Int(targetX, targetY);
        else
            board.lastDoublePawnPush = new Vector2Int(-1, -1);
    }
}
