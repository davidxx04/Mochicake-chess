using UnityEngine;

public static class BoardHash
{
    private const ulong FnvPrime = 1099511628211UL;
    private const ulong FnvOffset = 14695981039346656037UL;

    public static ulong Compute(BoardModel board, TeamColor sideToMove)
    {
        ulong hash = FnvOffset;
        hash = Mix(hash, (ulong)sideToMove);
        hash = Mix(hash, (ulong)(uint)board.lastDoublePawnPush.x);
        hash = Mix(hash, (ulong)(uint)board.lastDoublePawnPush.y);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece piece = board.grid[x, y];
                if (piece == null) continue;

                hash = Mix(hash, (ulong)x);
                hash = Mix(hash, (ulong)y);
                hash = Mix(hash, (ulong)piece.type);
                hash = Mix(hash, (ulong)piece.team);
                if (piece.hasMoved)
                    hash = Mix(hash, 1UL);
            }
        }

        return hash;
    }

    private static ulong Mix(ulong hash, ulong value)
    {
        return (hash ^ value) * FnvPrime;
    }
}
