using System.Collections.Generic;
using System.Threading;

public static class ChessEngine
{
    public static SearchResult FindBestMove(
        BoardModel board,
        TeamColor aiColor,
        int depth,
        IReadOnlyDictionary<ulong, int> gamePositionCounts,
        CancellationToken cancellationToken = default)
    {
        var rootMoves = CollectMovesForSide(board, aiColor);
        if (rootMoves.Count == 0)
            return new SearchResult { hasMove = false };

        var scoredMoves = new List<(Move move, int score)>(rootMoves.Count);
        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;
        var searchPath = new HashSet<ulong>();

        foreach (Move move in OrderMoves(board, rootMoves))
        {
            cancellationToken.ThrowIfCancellationRequested();

            move.ApplyLogic(board);
            TeamColor nextToMove = GetOpponent(aiColor);
            ulong positionHash = BoardHash.Compute(board, nextToMove);

            int repetitionPenalty = GetRepetitionPenalty(gamePositionCounts, searchPath, positionHash);
            int score = -Negamax(
                board,
                depth - 1,
                -beta,
                -alpha,
                nextToMove,
                aiColor,
                searchPath,
                cancellationToken);
            score -= repetitionPenalty;

            move.UndoLogic(board);

            scoredMoves.Add((move, score));

            if (score > alpha)
                alpha = score;
        }

        scoredMoves.Sort((a, b) => b.score.CompareTo(a.score));

        foreach (var (move, _) in scoredMoves)
        {
            move.ApplyLogic(board);
            ulong hash = BoardHash.Compute(board, GetOpponent(aiColor));
            move.UndoLogic(board);

            if (!WouldCauseGameRepetition(gamePositionCounts, hash))
            {
                return new SearchResult
                {
                    hasMove = true,
                    fromX = move.startX,
                    fromY = move.startY,
                    toX = move.targetX,
                    toY = move.targetY,
                    score = scoredMoves[0].score
                };
            }
        }

        var best = scoredMoves[0].move;
        return new SearchResult
        {
            hasMove = true,
            fromX = best.startX,
            fromY = best.startY,
            toX = best.targetX,
            toY = best.targetY,
            score = scoredMoves[0].score
        };
    }

    private static int Negamax(
        BoardModel board,
        int depth,
        int alpha,
        int beta,
        TeamColor sideToMove,
        TeamColor aiColor,
        HashSet<ulong> searchPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ulong positionHash = BoardHash.Compute(board, sideToMove);
        bool repetitionInLine = searchPath.Contains(positionHash);
        searchPath.Add(positionHash);

        if (!MovementLogic.HasAnyValidMove(board, sideToMove))
        {
            searchPath.Remove(positionHash);

            if (MovementLogic.IsKingInCheck(board, sideToMove))
            {
                return sideToMove == aiColor
                    ? -BoardEvaluator.CheckmateScore + depth
                    : BoardEvaluator.CheckmateScore - depth;
            }
            return 0;
        }

        if (depth <= 0)
        {
            searchPath.Remove(positionHash);
            int eval = BoardEvaluator.Evaluate(board, aiColor);
            return repetitionInLine ? eval - BoardEvaluator.RepetitionPenalty : eval;
        }

        int best = int.MinValue + 1;
        List<Move> moves = CollectMovesForSide(board, sideToMove);
        TeamColor opponent = GetOpponent(sideToMove);

        foreach (Move move in OrderMoves(board, moves))
        {
            cancellationToken.ThrowIfCancellationRequested();

            move.ApplyLogic(board);
            int score = -Negamax(board, depth - 1, -beta, -alpha, opponent, aiColor, searchPath, cancellationToken);
            move.UndoLogic(board);

            if (repetitionInLine)
                score -= BoardEvaluator.RepetitionPenalty / 2;

            if (score > best)
                best = score;
            if (score > alpha)
                alpha = score;
            if (alpha >= beta)
                break;
        }

        searchPath.Remove(positionHash);
        return best;
    }

    private static int GetRepetitionPenalty(
        IReadOnlyDictionary<ulong, int> gamePositionCounts,
        HashSet<ulong> searchPath,
        ulong positionHash)
    {
        int penalty = 0;

        if (gamePositionCounts != null && gamePositionCounts.TryGetValue(positionHash, out int gameCount) && gameCount >= 2)
            penalty += BoardEvaluator.RepetitionPenalty;

        if (searchPath.Contains(positionHash))
            penalty += BoardEvaluator.RepetitionPenalty / 2;

        return penalty;
    }

    private static bool WouldCauseGameRepetition(IReadOnlyDictionary<ulong, int> gamePositionCounts, ulong positionHash)
    {
        if (gamePositionCounts == null)
            return false;

        return gamePositionCounts.TryGetValue(positionHash, out int count) && count >= 2;
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

            bool aCheck = GivesCheck(board, a);
            bool bCheck = GivesCheck(board, b);
            if (aCheck != bCheck)
                return bCheck.CompareTo(aCheck);

            return 0;
        });
        return moves;
    }

    private static bool GivesCheck(BoardModel board, Move move)
    {
        move.ApplyLogic(board);
        TeamColor opponent = GetOpponent(move.pieceToMove.team);
        bool inCheck = MovementLogic.IsKingInCheck(board, opponent);
        move.UndoLogic(board);
        return inCheck;
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
