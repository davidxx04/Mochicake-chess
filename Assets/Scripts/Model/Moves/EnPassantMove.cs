using UnityEngine;

public class EnPassantMove : Move
{
    private LogicalPiece capturedPawn;

    public EnPassantMove(LogicalPiece pawn, int sX, int sY, int tX, int tY)
        : base(pawn, sX, sY, tX, tY) { }

    public override void ApplyLogic(BoardModel board)
    {
        MovePieceOnGrid(board);
        capturedPawn = board.grid[targetX, startY];
        board.grid[targetX, startY] = null;
    }

    public override void UndoLogic(BoardModel board)
    {
        RestorePieceOnGrid(board);
        board.grid[targetX, startY] = capturedPawn;
    }

    public override bool Execute(BoardModel board, BoardView view)
    {
        pieceToMove.hasMoved = true;
        board.lastDoublePawnPush = new Vector2Int(-1, -1);

        MovePieceOnGrid(board);
        board.grid[targetX, startY] = null;
        view.UpdateVisualPiece(startX, startY, targetX, targetY, pieceToMove);
        view.DestroyVisualPiece(targetX, startY);

        return false;
    }
}
