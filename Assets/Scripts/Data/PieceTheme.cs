using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceTheme", menuName = "Chess/Piece Theme")]
public class PieceTheme : ScriptableObject
{
    [Header("White Pieces")]
    public Sprite whitePawn;
    public Sprite whiteRook;
    public Sprite whiteKnight;
    public Sprite whiteBishop;
    public Sprite whiteQueen;
    public Sprite whiteKing;

    [Header("Black Pieces")]
    public Sprite blackPawn;
    public Sprite blackRook;
    public Sprite blackKnight;
    public Sprite blackBishop;
    public Sprite blackQueen;
    public Sprite blackKing;

    public Sprite GetSprite(PieceType type, TeamColor team)
    {
        if (team == TeamColor.White)
        {
            switch (type)
            {
                case PieceType.Pawn: return whitePawn;
                case PieceType.Rook: return whiteRook;
                case PieceType.Knight: return whiteKnight;
                case PieceType.Bishop: return whiteBishop;
                case PieceType.Queen: return whiteQueen;
                case PieceType.King: return whiteKing;
            }
        }
        else
        {
            switch (type)
            {
                case PieceType.Pawn: return blackPawn;
                case PieceType.Rook: return blackRook;
                case PieceType.Knight: return blackKnight;
                case PieceType.Bishop: return blackBishop;
                case PieceType.Queen: return blackQueen;
                case PieceType.King: return blackKing;
            }
        }
        return null;
    }
}