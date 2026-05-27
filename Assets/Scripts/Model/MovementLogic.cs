using System.Collections.Generic;
using UnityEngine;

public static class MovementLogic
{
    public static List<Vector2Int> GetValidMoves(BoardModel board, int startX, int startY)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        LogicalPiece piece = board.grid[startX, startY];

        // clicking in a empty square means nothing happens
        if (piece == null) return validMoves;

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
                    {
                        validMoves.Add(new Vector2Int(targetX, targetY));
                    }
                }
                break;

            case PieceType.Rook:
                Vector2Int[] rookDirections = new Vector2Int[]
                {
                    new Vector2Int(0, 1), new Vector2Int(0, -1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0)
                };
                validMoves.AddRange(GetSlidingMoves(board, piece, startX, startY, rookDirections));
                break;

            case PieceType.Bishop:
                Vector2Int[] bishopDirections = new Vector2Int[]
                {
                    new Vector2Int(1, 1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };
                validMoves.AddRange(GetSlidingMoves(board, piece, startX, startY, bishopDirections));
                break;

            case PieceType.Queen:
                Vector2Int[] queenDirections = new Vector2Int[]
                {
                    new Vector2Int(0, 1), new Vector2Int(0, -1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(1, 1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(-1, -1)
                };
                validMoves.AddRange(GetSlidingMoves(board, piece, startX, startY, queenDirections));
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
                    {
                        validMoves.Add(new Vector2Int(targetX, targetY));
                    }
                }
                break;

            case PieceType.Pawn:
                int direction = (piece.team == TeamColor.White) ? 1 : -1;
                int forwardY = startY + direction;

                if (forwardY >= 0 && forwardY < 8)
                {
                    
                    if (board.grid[startX, forwardY] == null)
                    {
                        validMoves.Add(new Vector2Int(startX, forwardY));

                        // double movement forward if conditions are met
                        bool isStartingPos = (piece.team == TeamColor.White && startY == 1) ||
                                             (piece.team == TeamColor.Black && startY == 6);

                        if (isStartingPos)
                        {
                            int doubleForwardY = startY + (direction * 2);
                            if (board.grid[startX, doubleForwardY] == null)
                            {
                                validMoves.Add(new Vector2Int(startX, doubleForwardY));
                            }
                        }
                    }
                }

                // Diagonal captures
                int[] captureX = new int[] { -1, 1 };

                for (int i = 0; i < captureX.Length; i++)
                {
                    int targetX = startX + captureX[i];
                    int targetY = startY + direction;

                    if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8)
                    {
                        LogicalPiece pieceAtTarget = board.grid[targetX, targetY];

                        if (pieceAtTarget != null && pieceAtTarget.team != piece.team)
                        {
                            validMoves.Add(new Vector2Int(targetX, targetY));
                        }
                    }
                }
                break;
        }

        return validMoves;
    }

    // Auxiliar function for slide pieces (Rook & Bishop & Queen)
    private static List<Vector2Int> GetSlidingMoves(BoardModel board, LogicalPiece piece, int startX, int startY, Vector2Int[] directions)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        for (int i = 0; i < directions.Length; i++)
        {
            int currentX = startX + directions[i].x;
            int currentY = startY + directions[i].y;

            while (true)
            {
                if (currentX < 0 || currentX >= 8 || currentY < 0 || currentY >= 8)
                {
                    break;
                }

                LogicalPiece pieceAtTarget = board.grid[currentX, currentY];

                if (pieceAtTarget == null)
                {
                    moves.Add(new Vector2Int(currentX, currentY));
                    currentX += directions[i].x;
                    currentY += directions[i].y;
                }
                else
                {
                    if (pieceAtTarget.team != piece.team)
                    {
                        moves.Add(new Vector2Int(currentX, currentY));
                    }
                    break;
                }
            }
        }

        return moves;
    }

    // Auxiliar function to validate specific positions (knight & king) (not used 4 pawns cause they have specific conditions)
    private static bool IsValidSquare(BoardModel board, TeamColor myPieceTeam, int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return false;
        LogicalPiece pieceAtTarget = board.grid[x, y];
        if (pieceAtTarget != null && pieceAtTarget.team == myPieceTeam) return false;
        return true;
    }
}