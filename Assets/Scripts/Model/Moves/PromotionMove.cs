using UnityEngine;

public class PromotionMove : Move
{
    private PieceType savedPieceType;

    public PromotionMove(LogicalPiece pawn, int sX, int sY, int tX, int tY)
        : base(pawn, sX, sY, tX, tY) { }

    public override void ApplyLogic(BoardModel board)
    {
        BeginApply(board);
        board.lastDoublePawnPush = new Vector2Int(-1, -1);

        MovePieceOnGrid(board);
        pieceToMove.hasMoved = true;

        savedPieceType = pieceToMove.type;
        pieceToMove.type = PieceType.Queen;
    }

    public override void UndoLogic(BoardModel board)
    {
        pieceToMove.type = savedPieceType;
        RestoreApplyState(board);
    }

    public override bool Execute(BoardModel board, BoardView view)
    {
        BeginApply(board);
        board.lastDoublePawnPush = new Vector2Int(-1, -1);
        MovePieceOnGrid(board);
        pieceToMove.hasMoved = true;

        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);
        return true;
    }
}
