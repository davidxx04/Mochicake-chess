public static class BoardEvaluator
{
    public const int CheckmateScore = 100_000;

    private const int PawnValue = 10;
    private const int KnightValue = 30;
    private const int BishopValue = 30;
    private const int RookValue = 50;
    private const int QueenValue = 90;
    private const int KingValue = 9000;

    public static int Evaluate(BoardModel board, TeamColor aiColor)
    {
        int score = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece == null) continue;

                int value = GetPieceValue(piece.type);
                score += piece.team == aiColor ? value : -value;
            }
        }

        return score;
    }

    private static int GetPieceValue(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return PawnValue;
            case PieceType.Knight: return KnightValue;
            case PieceType.Bishop: return BishopValue;
            case PieceType.Rook: return RookValue;
            case PieceType.Queen: return QueenValue;
            case PieceType.King: return KingValue;
            default: return 0;
        }
    }
}
