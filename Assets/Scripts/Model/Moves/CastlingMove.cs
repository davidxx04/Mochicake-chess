using UnityEngine;

public class CastlingMove : Move
{
    public CastlingMove(LogicalPiece king, int sX, int sY, int tX, int tY)
        : base(king, sX, sY, tX, tY) { }

    private int RookStartX => targetX > startX ? 7 : 0;
    private int RookTargetX => targetX > startX ? targetX - 1 : targetX + 1;

    public override void ApplyLogic(BoardModel board)
    {
        MovePieceOnGrid(board);
    }

    public override void UndoLogic(BoardModel board)
    {
        RestorePieceOnGrid(board);
    }

    public override bool Execute(BoardModel board, BoardView view)
    {
        pieceToMove.hasMoved = true;
        board.lastDoublePawnPush = new Vector2Int(-1, -1);

        MovePieceOnGrid(board);
        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);

        LogicalPiece rook = board.grid[RookStartX, startY];
        rook.hasMoved = true;
        board.grid[RookTargetX, startY] = rook;
        board.grid[RookStartX, startY] = null;
        view.UpdateVisualPiece(RookStartX, startY, RookTargetX, startY, rook);

        return false;
    }
}
