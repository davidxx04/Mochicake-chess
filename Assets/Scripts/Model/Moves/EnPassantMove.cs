using UnityEngine;

public class EnPassantMove : Move
{
    private LogicalPiece capturedPawn;

    public EnPassantMove(LogicalPiece pawn, int sX, int sY, int tX, int tY)
        : base(pawn, sX, sY, tX, tY) { }

    public override void ApplyLogic(BoardModel board)
    {
        BeginApply(board);
        board.lastDoublePawnPush = new Vector2Int(-1, -1);

        MovePieceOnGrid(board);
        pieceToMove.hasMoved = true;

        capturedPawn = board.grid[targetX, startY];
        board.grid[targetX, startY] = null;
    }

    public override void UndoLogic(BoardModel board)
    {
        board.grid[targetX, startY] = capturedPawn;
        RestoreApplyState(board);
    }

    public override bool Execute(BoardModel board, BoardView view)
    {
        ApplyLogic(board);
        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);
        view.DestroyVisualPiece(targetX, startY);
        return false;
    }
}
