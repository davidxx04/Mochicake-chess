public class BoardModel
{

    public LogicalPiece[,] grid = new LogicalPiece[8, 8];

    // note: we don´t want to move the settings logic inside here because we want to keep the logic simple and accurate with real chess
    // we will want to manage the settings of the board in the view, but the core logic has to be clear and always the same.
    public void SetupClassicBoard()
    {
        // Set pawns
        for (int x = 0; x < 8; x++)
        {
            grid[x, 1] = new LogicalPiece(TeamColor.White, PieceType.Pawn);
            grid[x, 6] = new LogicalPiece(TeamColor.Black, PieceType.Pawn);
        }

        // Set major pieces for White (Row 0)
        grid[0, 0] = new LogicalPiece(TeamColor.White, PieceType.Rook);   // Left
        grid[7, 0] = new LogicalPiece(TeamColor.White, PieceType.Rook);
        grid[1, 0] = new LogicalPiece(TeamColor.White, PieceType.Knight); // Left
        grid[6, 0] = new LogicalPiece(TeamColor.White, PieceType.Knight);
        grid[2, 0] = new LogicalPiece(TeamColor.White, PieceType.Bishop); // Left
        grid[5, 0] = new LogicalPiece(TeamColor.White, PieceType.Bishop);
        grid[3, 0] = new LogicalPiece(TeamColor.White, PieceType.Queen);
        grid[4, 0] = new LogicalPiece(TeamColor.White, PieceType.King); 

        // Set major pieces for Black (Row 7)
        grid[0, 7] = new LogicalPiece(TeamColor.Black, PieceType.Rook);
        grid[7, 7] = new LogicalPiece(TeamColor.Black, PieceType.Rook);
        grid[1, 7] = new LogicalPiece(TeamColor.Black, PieceType.Knight);
        grid[6, 7] = new LogicalPiece(TeamColor.Black, PieceType.Knight);
        grid[2, 7] = new LogicalPiece(TeamColor.Black, PieceType.Bishop);
        grid[5, 7] = new LogicalPiece(TeamColor.Black, PieceType.Bishop);
        grid[3, 7] = new LogicalPiece(TeamColor.Black, PieceType.Queen);
        grid[4, 7] = new LogicalPiece(TeamColor.Black, PieceType.King);
    }
    public void MovePiece(int startX, int startY, int endX, int endY)
    {
        LogicalPiece pieceToMove = grid[startX, startY];
        grid[endX, endY] = pieceToMove; // Can overwrite previous piece (capture)
        grid[startX, startY] = null;
    }

}