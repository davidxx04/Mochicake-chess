public class NormalMove : Move
{
    public NormalMove(LogicalPiece piece, int sX, int sY, int tX, int tY)
        : base(piece, sX, sY, tX, tY) { }

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
        UpdateEnPassantTarget(board);
        MovePieceOnGrid(board);
        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);
        return false;
    }
}
