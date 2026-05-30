using System.Collections.Generic;
using UnityEngine;

public static class MovementLogic
{
    public static List<Move> GetValidMoves(BoardModel board, int startX, int startY)
    {
        List<Move> legalMoves = new List<Move>();
        LogicalPiece piece = board.grid[startX, startY];

        if (piece == null) return legalMoves;

        List<Move> pseudoLegalMoves = GetPseudoLegalMoves(board, startX, startY);
        Vector2Int originalKingPos = FindKingPosition(board, piece.team);

        foreach (Move move in pseudoLegalMoves)
        {
            if (!DoesMoveLeaveKingInCheck(board, move, originalKingPos))
                legalMoves.Add(move);
        }

        return legalMoves;
    }

    private static bool DoesMoveLeaveKingInCheck(BoardModel board, Move move, Vector2Int originalKingPos)
    {
        move.ApplyLogic(board);

        Vector2Int currentKingPos = (move.pieceToMove.type == PieceType.King)
            ? new Vector2Int(move.targetX, move.targetY)
            : originalKingPos;

        bool isKingInCheck = false;
        if (currentKingPos.x != -1)
            isKingInCheck = IsSquareUnderAttack(board, currentKingPos.x, currentKingPos.y, move.pieceToMove.team);

        move.UndoLogic(board);
        return isKingInCheck;
    }

    private static Vector2Int FindKingPosition(BoardModel board, TeamColor myColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece != null && piece.team == myColor && piece.type == PieceType.King)
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    private static List<Move> GetPseudoLegalMoves(BoardModel board, int startX, int startY)
    {
        List<Move> pseudoMoves = new List<Move>();
        LogicalPiece piece = board.grid[startX, startY];

        switch (piece.type)
        {
            case PieceType.Knight:
                Vector2Int[] jumps = new Vector2Int[]
                {
                    new Vector2Int(1, 2), new Vector2Int(2, 1),
                    new Vector2Int(2, -1), new Vector2Int(1, -2),
                    new Vector2Int(-1, -2), new Vector2Int(-2, -1),
                    new Vector2Int(-2, 1), new Vector2Int(-1, 2)
                };

                for (int i = 0; i < jumps.Length; i++)
                {
                    int targetX = startX + jumps[i].x;
                    int targetY = startY + jumps[i].y;

                    if (IsValidSquare(board, piece.team, targetX, targetY))
                        pseudoMoves.Add(CreateMove(piece, startX, startY, targetX, targetY));
                }
                break;

            case PieceType.Rook:
                Vector2Int[] rookDirections = new Vector2Int[]
                {
                    new Vector2Int(0, 1), new Vector2Int(0, -1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0)
                };
                pseudoMoves.AddRange(GetSlidingMoves(board, piece, startX, startY, rookDirections));
                break;

            case PieceType.Bishop:
                Vector2Int[] bishopDirections = new Vector2Int[]
                {
                    new Vector2Int(1, 1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };
                pseudoMoves.AddRange(GetSlidingMoves(board, piece, startX, startY, bishopDirections));
                break;

            case PieceType.Queen:
                Vector2Int[] queenDirections = new Vector2Int[]
                {
                    new Vector2Int(0, 1), new Vector2Int(0, -1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(1, 1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };
                pseudoMoves.AddRange(GetSlidingMoves(board, piece, startX, startY, queenDirections));
                break;

            case PieceType.King:
                Vector2Int[] kingDirections = new Vector2Int[]
                {
                    new Vector2Int(0, 1), new Vector2Int(0, -1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(1, 1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };

                for (int i = 0; i < kingDirections.Length; i++)
                {
                    int targetX = startX + kingDirections[i].x;
                    int targetY = startY + kingDirections[i].y;

                    if (IsValidSquare(board, piece.team, targetX, targetY))
                        pseudoMoves.Add(CreateMove(piece, startX, startY, targetX, targetY));
                }
                pseudoMoves.AddRange(GetCastlingMoves(board, piece, startX, startY));
                break;

            case PieceType.Pawn:
                int direction = (piece.team == TeamColor.White) ? 1 : -1;
                int forwardY = startY + direction;

                if (forwardY >= 0 && forwardY < 8)
                {
                    if (board.grid[startX, forwardY] == null)
                    {
                        pseudoMoves.Add(CreateMove(piece, startX, startY, startX, forwardY));

                        bool isStartingPos = (piece.team == TeamColor.White && startY == 1) ||
                                             (piece.team == TeamColor.Black && startY == 6);

                        if (isStartingPos)
                        {
                            int doubleForwardY = startY + (direction * 2);
                            if (board.grid[startX, doubleForwardY] == null)
                                pseudoMoves.Add(CreateMove(piece, startX, startY, startX, doubleForwardY));
                        }
                    }
                }

                int[] captureX = new int[] { -1, 1 };

                for (int i = 0; i < captureX.Length; i++)
                {
                    int targetX = startX + captureX[i];
                    int targetY = startY + direction;

                    if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8)
                    {
                        LogicalPiece pieceAtTarget = board.grid[targetX, targetY];

                        if (pieceAtTarget != null && pieceAtTarget.team != piece.team)
                            pseudoMoves.Add(CreateMove(piece, startX, startY, targetX, targetY));
                    }
                }

                if (board.lastDoublePawnPush.x != -1)
                {
                    if (Mathf.Abs(board.lastDoublePawnPush.x - startX) == 1 && board.lastDoublePawnPush.y == startY)
                    {
                        pseudoMoves.Add(new EnPassantMove(
                            piece, startX, startY,
                            board.lastDoublePawnPush.x, startY + direction));
                    }
                }
                break;
        }

        return pseudoMoves;
    }

    private static Move CreateMove(LogicalPiece piece, int startX, int startY, int targetX, int targetY)
    {
        if (piece.type == PieceType.King && Mathf.Abs(startX - targetX) == 2)
            return new CastlingMove(piece, startX, startY, targetX, targetY);

        if (piece.type == PieceType.Pawn && IsPromotionSquare(targetY, piece.team))
            return new PromotionMove(piece, startX, startY, targetX, targetY);

        return new NormalMove(piece, startX, startY, targetX, targetY);
    }

    private static bool IsPromotionSquare(int y, TeamColor team)
    {
        return (team == TeamColor.White && y == 7) || (team == TeamColor.Black && y == 0);
    }

    private static List<Move> GetSlidingMoves(BoardModel board, LogicalPiece piece, int startX, int startY, Vector2Int[] directions)
    {
        List<Move> moves = new List<Move>();

        for (int i = 0; i < directions.Length; i++)
        {
            int currentX = startX + directions[i].x;
            int currentY = startY + directions[i].y;

            while (true)
            {
                if (currentX < 0 || currentX >= 8 || currentY < 0 || currentY >= 8) break;

                LogicalPiece pieceAtTarget = board.grid[currentX, currentY];

                if (pieceAtTarget == null)
                {
                    moves.Add(CreateMove(piece, startX, startY, currentX, currentY));
                    currentX += directions[i].x;
                    currentY += directions[i].y;
                }
                else
                {
                    if (pieceAtTarget.team != piece.team)
                        moves.Add(CreateMove(piece, startX, startY, currentX, currentY));
                    break;
                }
            }
        }
        return moves;
    }

    private static bool IsValidSquare(BoardModel board, TeamColor myPieceTeam, int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return false;
        LogicalPiece pieceAtTarget = board.grid[x, y];
        if (pieceAtTarget != null && pieceAtTarget.team == myPieceTeam) return false;
        return true;
    }

    public static bool IsSquareUnderAttack(BoardModel board, int targetX, int targetY, TeamColor myColor)
    {
        if (IsAttackedByKnight(board, targetX, targetY, myColor)) return true;
        if (IsAttackedByPawn(board, targetX, targetY, myColor)) return true;
        if (IsAttackedByKing(board, targetX, targetY, myColor)) return true;

        Vector2Int[] orthogonalDirs = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        if (IsAttackedBySlider(board, targetX, targetY, myColor, orthogonalDirs, PieceType.Rook)) return true;

        Vector2Int[] diagonalDirs = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };
        if (IsAttackedBySlider(board, targetX, targetY, myColor, diagonalDirs, PieceType.Bishop)) return true;

        return false;
    }

    private static bool IsAttackedByKnight(BoardModel board, int targetX, int targetY, TeamColor myColor)
    {
        Vector2Int[] jumps = { new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(2, -1), new Vector2Int(1, -2),
                               new Vector2Int(-1, -2), new Vector2Int(-2, -1), new Vector2Int(-2, 1), new Vector2Int(-1, 2) };

        foreach (var jump in jumps)
        {
            int checkX = targetX + jump.x;
            int checkY = targetY + jump.y;

            if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
            {
                LogicalPiece piece = board.grid[checkX, checkY];
                if (piece != null && piece.team != myColor && piece.type == PieceType.Knight) return true;
            }
        }
        return false;
    }

    private static bool IsAttackedByPawn(BoardModel board, int targetX, int targetY, TeamColor myColor)
    {
        int enemyPawnDir = (myColor == TeamColor.White) ? 1 : -1;

        int[] captureX = { -1, 1 };
        foreach (int offsetX in captureX)
        {
            int checkX = targetX + offsetX;
            int checkY = targetY + enemyPawnDir;

            if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
            {
                LogicalPiece piece = board.grid[checkX, checkY];
                if (piece != null && piece.team != myColor && piece.type == PieceType.Pawn) return true;
            }
        }
        return false;
    }

    private static bool IsAttackedBySlider(BoardModel board, int targetX, int targetY, TeamColor myColor, Vector2Int[] directions, PieceType specificSlider)
    {
        foreach (var dir in directions)
        {
            int currentX = targetX + dir.x;
            int currentY = targetY + dir.y;

            while (currentX >= 0 && currentX < 8 && currentY >= 0 && currentY < 8)
            {
                LogicalPiece piece = board.grid[currentX, currentY];

                if (piece != null)
                {
                    if (piece.team != myColor && (piece.type == specificSlider || piece.type == PieceType.Queen))
                        return true;
                    break;
                }
                currentX += dir.x;
                currentY += dir.y;
            }
        }
        return false;
    }

    private static bool IsAttackedByKing(BoardModel board, int targetX, int targetY, TeamColor myColor)
    {
        Vector2Int[] dirs = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0),
                              new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var dir in dirs)
        {
            int checkX = targetX + dir.x;
            int checkY = targetY + dir.y;

            if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
            {
                LogicalPiece piece = board.grid[checkX, checkY];
                if (piece != null && piece.team != myColor && piece.type == PieceType.King) return true;
            }
        }
        return false;
    }

    public static bool HasAnyValidMove(BoardModel board, TeamColor playerColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];

                if (piece != null && piece.team == playerColor)
                {
                    List<Move> moves = GetValidMoves(board, x, y);
                    if (moves.Count > 0)
                        return true;
                }
            }
        }

        return false;
    }

    public static bool IsKingInCheck(BoardModel board, TeamColor kingColor)
    {
        Vector2Int kingPos = FindKingPosition(board, kingColor);
        if (kingPos.x != -1)
            return IsSquareUnderAttack(board, kingPos.x, kingPos.y, kingColor);
        return false;
    }

    private static List<Move> GetCastlingMoves(BoardModel board, LogicalPiece king, int startX, int startY)
    {
        List<Move> castlingMoves = new List<Move>();

        if (king.hasMoved) return castlingMoves;
        if (IsSquareUnderAttack(board, startX, startY, king.team)) return castlingMoves;

        if (CanCastleKingside(board, king.team, startY))
            castlingMoves.Add(new CastlingMove(king, startX, startY, startX + 2, startY));

        if (CanCastleQueenside(board, king.team, startY))
            castlingMoves.Add(new CastlingMove(king, startX, startY, startX - 2, startY));

        return castlingMoves;
    }

    private static bool CanCastleKingside(BoardModel board, TeamColor team, int y)
    {
        LogicalPiece rook = board.grid[7, y];
        if (rook == null || rook.type != PieceType.Rook || rook.team != team || rook.hasMoved) return false;

        if (board.grid[5, y] != null || board.grid[6, y] != null) return false;

        if (IsSquareUnderAttack(board, 5, y, team) || IsSquareUnderAttack(board, 6, y, team)) return false;

        return true;
    }

    private static bool CanCastleQueenside(BoardModel board, TeamColor team, int y)
    {
        LogicalPiece rook = board.grid[0, y];
        if (rook == null || rook.type != PieceType.Rook || rook.team != team || rook.hasMoved) return false;

        if (board.grid[1, y] != null || board.grid[2, y] != null || board.grid[3, y] != null) return false;

        if (IsSquareUnderAttack(board, 2, y, team) || IsSquareUnderAttack(board, 3, y, team)) return false;

        return true;
    }
}
