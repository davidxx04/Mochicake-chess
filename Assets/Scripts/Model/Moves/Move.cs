using UnityEngine;

public abstract class Move
{
    public int startX, startY, targetX, targetY;
    public LogicalPiece pieceToMove;

    protected LogicalPiece capturedAtTarget;

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

    /// <returns>True if the arbiter must pause for promotion UI.</returns>
    public abstract bool Execute(BoardModel board, BoardView view);

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
