using System.Collections.Generic;
using System.Threading;

public static class ChessEngine
{
    public static SearchResult FindBestMove(BoardModel board, TeamColor aiColor, int depth, CancellationToken cancellationToken = default)
    {
        var best = new SearchResult { hasMove = false, score = int.MinValue };
        List<Move> rootMoves = CollectMovesForSide(board, aiColor);

        if (rootMoves.Count == 0)
            return best;

        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;

        foreach (Move move in OrderMoves(board, rootMoves))
        {
            cancellationToken.ThrowIfCancellationRequested();

            move.ApplyLogic(board);
            int score = -Negamax(board, depth - 1, -beta, -alpha, GetOpponent(aiColor), aiColor, cancellationToken);
            move.UndoLogic(board);

            if (score > best.score || !best.hasMove)
            {
                best.hasMove = true;
                best.score = score;
                best.fromX = move.startX;
                best.fromY = move.startY;
                best.toX = move.targetX;
                best.toY = move.targetY;
            }

            if (score > alpha)
                alpha = score;
        }

        return best;
    }

    private static int Negamax(
        BoardModel board,
        int depth,
        int alpha,
        int beta,
        TeamColor sideToMove,
        TeamColor aiColor,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!MovementLogic.HasAnyValidMove(board, sideToMove))
        {
            if (MovementLogic.IsKingInCheck(board, sideToMove))
            {
                return sideToMove == aiColor
                    ? -BoardEvaluator.CheckmateScore + depth
                    : BoardEvaluator.CheckmateScore - depth;
            }
            return 0;
        }

        if (depth <= 0)
            return BoardEvaluator.Evaluate(board, aiColor);

        int best = int.MinValue + 1;
        List<Move> moves = CollectMovesForSide(board, sideToMove);
        TeamColor opponent = GetOpponent(sideToMove);

        foreach (Move move in OrderMoves(board, moves))
        {
            cancellationToken.ThrowIfCancellationRequested();

            move.ApplyLogic(board);
            int score = -Negamax(board, depth - 1, -beta, -alpha, opponent, aiColor, cancellationToken);
            move.UndoLogic(board);

            if (score > best)
                best = score;
            if (score > alpha)
                alpha = score;
            if (alpha >= beta)
                break;
        }

        return best;
    }

    private static List<Move> CollectMovesForSide(BoardModel board, TeamColor side)
    {
        var moves = new List<Move>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece != null && piece.team == side)
                    moves.AddRange(MovementLogic.GetValidMoves(board, x, y));
            }
        }

        return moves;
    }

    private static List<Move> OrderMoves(BoardModel board, List<Move> moves)
    {
        moves.Sort((a, b) =>
        {
            bool aCapture = IsCapture(board, a);
            bool bCapture = IsCapture(board, b);
            if (aCapture != bCapture)
                return bCapture.CompareTo(aCapture);
            return 0;
        });
        return moves;
    }

    private static bool IsCapture(BoardModel board, Move move)
    {
        if (move is EnPassantMove)
            return true;

        LogicalPiece atTarget = board.grid[move.targetX, move.targetY];
        return atTarget != null && atTarget.team != move.pieceToMove.team;
    }

    private static TeamColor GetOpponent(TeamColor team)
    {
        return team == TeamColor.White ? TeamColor.Black : TeamColor.White;
    }
}
