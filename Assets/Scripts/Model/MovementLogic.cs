using System.Collections.Generic;
using UnityEngine;

public static class MovementLogic
{
    // ==============================================================================
    // --- PARTE 1: EL FILTRO PRINCIPAL (Mejorado en rendimiento) ---
    // ==============================================================================

    public static List<Vector2Int> GetValidMoves(BoardModel board, int startX, int startY)
    {
        List<Vector2Int> legalMoves = new List<Vector2Int>();
        LogicalPiece piece = board.grid[startX, startY];

        if (piece == null) return legalMoves;

        List<Vector2Int> pseudoLegalMoves = GetPseudoLegalMoves(board, startX, startY);

        // MEJORA: Buscamos al Rey UNA SOLA VEZ antes del bucle (Ahorramos miles de operaciones)
        Vector2Int originalKingPos = FindKingPosition(board, piece.team);

        foreach (Vector2Int target in pseudoLegalMoves)
        {
            if (!DoesMoveLeaveKingInCheck(board, startX, startY, target.x, target.y, piece.team, originalKingPos))
            {
                legalMoves.Add(target);
            }
        }

        return legalMoves;
    }

    // Le hemos añadido el parámetro originalKingPos a la función
    private static bool DoesMoveLeaveKingInCheck(BoardModel board, int startX, int startY, int targetX, int targetY, TeamColor myColor, Vector2Int originalKingPos)
    {
        LogicalPiece pieceToMove = board.grid[startX, startY];
        LogicalPiece pieceAtTarget = board.grid[targetX, targetY];

        // 1. Make (Simular)
        board.grid[targetX, targetY] = pieceToMove;
        board.grid[startX, startY] = null;

        // 2. MEJORA DE RENDIMIENTO: ¿Dónde está el Rey ahora?
        // Si la pieza que muevo es el Rey, su nueva posición es el target. Si no, es la que ya teníamos guardada.
        Vector2Int currentKingPos = (pieceToMove.type == PieceType.King) ? new Vector2Int(targetX, targetY) : originalKingPos;

        // 3. Radar
        bool isKingInCheck = false;
        if (currentKingPos.x != -1)
        {
            isKingInCheck = IsSquareUnderAttack(board, currentKingPos.x, currentKingPos.y, myColor);
        }

        // 4. Unmake (Deshacer)
        board.grid[startX, startY] = pieceToMove;
        board.grid[targetX, targetY] = pieceAtTarget;

        return isKingInCheck;
    }

    private static Vector2Int FindKingPosition(BoardModel board, TeamColor myColor)
    {
        // Esta función ahora solo se ejecuta 1 sola vez por pieza tocada, en lugar de 20 o 30 veces.
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece != null && piece.team == myColor && piece.type == PieceType.King)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    // ==============================================================================
    // --- PARTE 2: GENERACIÓN DE MOVIMIENTOS (Tu código original renombrado) ---
    // ==============================================================================

    private static List<Vector2Int> GetPseudoLegalMoves(BoardModel board, int startX, int startY)
    {
        List<Vector2Int> pseudoMoves = new List<Vector2Int>();
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
                    {
                        pseudoMoves.Add(new Vector2Int(targetX, targetY));
                    }
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
                    {
                        pseudoMoves.Add(new Vector2Int(targetX, targetY));
                    }
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
                        pseudoMoves.Add(new Vector2Int(startX, forwardY));

                        bool isStartingPos = (piece.team == TeamColor.White && startY == 1) ||
                                             (piece.team == TeamColor.Black && startY == 6);

                        if (isStartingPos)
                        {
                            int doubleForwardY = startY + (direction * 2);
                            if (board.grid[startX, doubleForwardY] == null)
                            {
                                pseudoMoves.Add(new Vector2Int(startX, doubleForwardY));
                            }
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
                        {
                            pseudoMoves.Add(new Vector2Int(targetX, targetY));
                        }
                    }
                }
                break;
        }

        return pseudoMoves;
    }

    private static List<Vector2Int> GetSlidingMoves(BoardModel board, LogicalPiece piece, int startX, int startY, Vector2Int[] directions)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

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
                    moves.Add(new Vector2Int(currentX, currentY));
                    currentX += directions[i].x;
                    currentY += directions[i].y;
                }
                else
                {
                    if (pieceAtTarget.team != piece.team) moves.Add(new Vector2Int(currentX, currentY));
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

    // ==============================================================================
    // --- PARTE 3: EL RADAR DE AMENAZAS ---
    // ==============================================================================

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
                    {
                        return true;
                    }
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

    // ==============================================================================
    // --- PARTE 4: EL VEREDICTO FINAL (NUEVO) ---
    // ==============================================================================

    /// <summary>
    /// Comprueba si un equipo tiene AL MENOS un movimiento legal posible. 
    /// (Súper optimizado: en cuanto encuentra uno, corta el bucle y devuelve true).
    /// </summary>
    public static bool HasAnyValidMove(BoardModel board, TeamColor playerColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];

                // Si la pieza es de este jugador...
                if (piece != null && piece.team == playerColor)
                {
                    // Pedimos sus movimientos legales
                    List<Vector2Int> moves = GetValidMoves(board, x, y);

                    // Si tiene al menos 1, el jugador aún puede jugar. ¡Cortamos la búsqueda!
                    if (moves.Count > 0)
                    {
                        return true;
                    }
                }
            }
        }

        // Si ha mirado TODAS sus piezas y ninguna tiene movimientos, está bloqueado.
        return false;
    }

    /// <summary>
    /// Función auxiliar limpia para que el Árbitro pregunte si un Rey está en Jaque
    /// </summary>
    public static bool IsKingInCheck(BoardModel board, TeamColor kingColor)
    {
        Vector2Int kingPos = FindKingPosition(board, kingColor);
        if (kingPos.x != -1)
        {
            return IsSquareUnderAttack(board, kingPos.x, kingPos.y, kingColor);
        }
        return false;
    }

    // ==============================================================================
    // --- LÓGICA DE ENROQUE (Helpers Limpios) ---
    // ==============================================================================

    private static List<Vector2Int> GetCastlingMoves(BoardModel board, LogicalPiece king, int startX, int startY)
    {
        List<Vector2Int> castlingMoves = new List<Vector2Int>();

        // Regla 1 y 3: El rey no puede haberse movido, ni puede estar en Jaque para poder enrocar.
        if (king.hasMoved) return castlingMoves;
        if (IsSquareUnderAttack(board, startX, startY, king.team)) return castlingMoves;

        // Intentamos el Enroque Corto (Hacia la derecha, Torre en X=7)
        if (CanCastleKingside(board, king.team, startY))
        {
            castlingMoves.Add(new Vector2Int(startX + 2, startY));
        }

        // Intentamos el Enroque Largo (Hacia la izquierda, Torre en X=0)
        if (CanCastleQueenside(board, king.team, startY))
        {
            castlingMoves.Add(new Vector2Int(startX - 2, startY));
        }

        return castlingMoves;
    }

    private static bool CanCastleKingside(BoardModel board, TeamColor team, int y)
    {
        LogicalPiece rook = board.grid[7, y];
        // ¿Hay una torre sana en su sitio?
        if (rook == null || rook.type != PieceType.Rook || rook.team != team || rook.hasMoved) return false;

        // ¿Las casillas intermedias están vacías? (X=5 y X=6)
        if (board.grid[5, y] != null || board.grid[6, y] != null) return false;

        // ¿Alguna de esas casillas está bajo el fuego enemigo?
        if (IsSquareUnderAttack(board, 5, y, team) || IsSquareUnderAttack(board, 6, y, team)) return false;

        return true;
    }

    private static bool CanCastleQueenside(BoardModel board, TeamColor team, int y)
    {
        LogicalPiece rook = board.grid[0, y];
        // ¿Hay una torre sana en su sitio?
        if (rook == null || rook.type != PieceType.Rook || rook.team != team || rook.hasMoved) return false;

        // ¿Las casillas intermedias están vacías? (X=1, X=2 y X=3)
        if (board.grid[1, y] != null || board.grid[2, y] != null || board.grid[3, y] != null) return false;

        // El Rey pasa por X=3 y X=2 (No hace falta mirar X=1 para los jaques según las reglas)
        if (IsSquareUnderAttack(board, 2, y, team) || IsSquareUnderAttack(board, 3, y, team)) return false;

        return true;
    }
}