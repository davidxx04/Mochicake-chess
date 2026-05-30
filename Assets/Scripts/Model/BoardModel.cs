using UnityEngine;

public class BoardModel
{
    public LogicalPiece[,] grid = new LogicalPiece[8, 8];
    public Vector2Int lastDoublePawnPush = new Vector2Int(-1, -1);

    public void SetupClassicBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            grid[x, 1] = new LogicalPiece(TeamColor.White, PieceType.Pawn);
            grid[x, 6] = new LogicalPiece(TeamColor.Black, PieceType.Pawn);
        }

        grid[0, 0] = new LogicalPiece(TeamColor.White, PieceType.Rook);
        grid[7, 0] = new LogicalPiece(TeamColor.White, PieceType.Rook);
        grid[1, 0] = new LogicalPiece(TeamColor.White, PieceType.Knight);
        grid[6, 0] = new LogicalPiece(TeamColor.White, PieceType.Knight);
        grid[2, 0] = new LogicalPiece(TeamColor.White, PieceType.Bishop);
        grid[5, 0] = new LogicalPiece(TeamColor.White, PieceType.Bishop);
        grid[3, 0] = new LogicalPiece(TeamColor.White, PieceType.Queen);
        grid[4, 0] = new LogicalPiece(TeamColor.White, PieceType.King);

        grid[0, 7] = new LogicalPiece(TeamColor.Black, PieceType.Rook);
        grid[7, 7] = new LogicalPiece(TeamColor.Black, PieceType.Rook);
        grid[1, 7] = new LogicalPiece(TeamColor.Black, PieceType.Knight);
        grid[6, 7] = new LogicalPiece(TeamColor.Black, PieceType.Knight);
        grid[2, 7] = new LogicalPiece(TeamColor.Black, PieceType.Bishop);
        grid[5, 7] = new LogicalPiece(TeamColor.Black, PieceType.Bishop);
        grid[3, 7] = new LogicalPiece(TeamColor.Black, PieceType.Queen);
        grid[4, 7] = new LogicalPiece(TeamColor.Black, PieceType.King);
    }

    public BoardModel Clone()
    {
        var copy = new BoardModel();
        copy.lastDoublePawnPush = lastDoublePawnPush;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                copy.grid[x, y] = grid[x, y]?.Clone();
            }
        }

        return copy;
    }

    public void MovePiece(int startX, int startY, int endX, int endY)
    {
        LogicalPiece pieceToMove = grid[startX, startY];
        grid[endX, endY] = pieceToMove;
        grid[startX, startY] = null;
    }
}
