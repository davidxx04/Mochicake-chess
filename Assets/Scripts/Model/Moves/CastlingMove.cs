using UnityEngine;

public class CastlingMove : Move
{
    private LogicalPiece rook;
    private bool savedRookHasMoved;

    public CastlingMove(LogicalPiece king, int sX, int sY, int tX, int tY)
        : base(king, sX, sY, tX, tY) { }

    private int RookStartX => targetX > startX ? 7 : 0;
    private int RookTargetX => targetX > startX ? targetX - 1 : targetX + 1;

    public override void ApplyLogic(BoardModel board)
    {
        BeginApply(board);
        board.lastDoublePawnPush = new Vector2Int(-1, -1);

        MovePieceOnGrid(board);
        pieceToMove.hasMoved = true;

        rook = board.grid[RookStartX, startY];
        savedRookHasMoved = rook.hasMoved;
        rook.hasMoved = true;
        board.grid[RookTargetX, startY] = rook;
        board.grid[RookStartX, startY] = null;
    }

    public override void UndoLogic(BoardModel board)
    {
        board.grid[RookStartX, startY] = rook;
        board.grid[RookTargetX, startY] = null;
        rook.hasMoved = savedRookHasMoved;

        RestoreApplyState(board);
    }

    public override bool Execute(BoardModel board, BoardView view)
    {
        ApplyLogic(board);
        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);
        view.UpdateVisualPiece(RookStartX, startY, RookTargetX, startY, rook);
        return false;
    }
}
