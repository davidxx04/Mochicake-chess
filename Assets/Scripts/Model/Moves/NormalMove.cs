public class NormalMove : Move
{
    public NormalMove(LogicalPiece piece, int sX, int sY, int tX, int tY)
        : base(piece, sX, sY, tX, tY) { }

    public override void ApplyLogic(BoardModel board)
    {
        BeginApply(board);
        FinishApply(board);
    }

    public override void UndoLogic(BoardModel board)
    {
        RestoreApplyState(board);
    }

    public override bool Execute(BoardModel board, BoardView view)
    {
        ApplyLogic(board);
        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);
        return false;
    }
}
