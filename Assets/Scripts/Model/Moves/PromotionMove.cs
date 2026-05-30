using UnityEngine;

public class PromotionMove : Move
{
    public PromotionMove(LogicalPiece pawn, int sX, int sY, int tX, int tY)
        : base(pawn, sX, sY, tX, tY) { }

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

        return true;
    }
}
