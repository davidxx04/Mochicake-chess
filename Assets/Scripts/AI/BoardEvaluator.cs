using UnityEngine;

public static class BoardEvaluator
{
    public const int CheckmateScore = 100_000;
    public const int RepetitionPenalty = 8_000;

    private const int PawnValue = 100;
    private const int KnightValue = 320;
    private const int BishopValue = 330;
    private const int RookValue = 500;
    private const int QueenValue = 900;
    private const int KingValue = 20_000;

    public static int Evaluate(BoardModel board, TeamColor aiColor)
    {
        if (IsInsufficientMaterial(board))
            return 0;

        int material = EvaluateMaterial(board, aiColor);
        int positional = EvaluatePositional(board, aiColor, material);

        return material + positional;
    }

    private static int EvaluateMaterial(BoardModel board, TeamColor aiColor)
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

    private static int EvaluatePositional(BoardModel board, TeamColor aiColor, int materialScore)
    {
        int sign = materialScore >= 0 ? 1 : -1;
        int absMaterial = Mathf.Abs(materialScore);

        if (absMaterial < PawnValue)
            return 0;

        int score = 0;
        Vector2Int aiKing = FindKing(board, aiColor);
        Vector2Int enemyKing = FindKing(board, aiColor == TeamColor.White ? TeamColor.Black : TeamColor.White);

        if (aiKing.x >= 0 && enemyKing.x >= 0)
        {
            int kingDistance = ManhattanDistance(aiKing, enemyKing);
            score += sign * (14 - kingDistance) * 8;
        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece == null || piece.type != PieceType.Pawn) continue;

                int advancement = piece.team == TeamColor.White ? y : 7 - y;
                int pawnScore = advancement * advancement * 3;
                score += piece.team == aiColor ? pawnScore : -pawnScore;
            }
        }

        return score;
    }

    public static bool IsInsufficientMaterial(BoardModel board)
    {
        int whitePawns = 0, blackPawns = 0;
        int whiteMinors = 0, blackMinors = 0;
        bool whiteBishop = false, blackBishop = false;
        bool whiteKnight = false, blackKnight = false;
        bool whiteRook = false, blackRook = false;
        bool whiteQueen = false, blackQueen = false;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece == null || piece.type == PieceType.King) continue;

                if (piece.team == TeamColor.White)
                {
                    switch (piece.type)
                    {
                        case PieceType.Pawn: whitePawns++; break;
                        case PieceType.Knight: whiteKnight = true; whiteMinors++; break;
                        case PieceType.Bishop: whiteBishop = true; whiteMinors++; break;
                        case PieceType.Rook: whiteRook = true; break;
                        case PieceType.Queen: whiteQueen = true; break;
                    }
                }
                else
                {
                    switch (piece.type)
                    {
                        case PieceType.Pawn: blackPawns++; break;
                        case PieceType.Knight: blackKnight = true; blackMinors++; break;
                        case PieceType.Bishop: blackBishop = true; blackMinors++; break;
                        case PieceType.Rook: blackRook = true; break;
                        case PieceType.Queen: blackQueen = true; break;
                    }
                }
            }
        }

        if (whiteQueen || blackQueen || whiteRook || blackRook)
            return false;

        if (whitePawns > 0 || blackPawns > 0)
            return false;

        if (whiteMinors == 0 && blackMinors == 0)
            return true;

        if (whiteMinors <= 1 && blackMinors == 0)
            return whiteBishop || whiteKnight;

        if (blackMinors <= 1 && whiteMinors == 0)
            return blackBishop || blackKnight;

        if (whiteMinors == 1 && blackMinors == 1 && whiteBishop && blackBishop)
            return true;

        return false;
    }

    private static Vector2Int FindKing(BoardModel board, TeamColor team)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece != null && piece.team == team && piece.type == PieceType.King)
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    private static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
