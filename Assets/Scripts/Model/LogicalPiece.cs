public class LogicalPiece
{
    public TeamColor team;
    public PieceType type;
    public bool hasMoved = false;

    public LogicalPiece(TeamColor pieceTeam, PieceType pieceType)
    {
        team = pieceTeam;
        type = pieceType;
    }

    public LogicalPiece Clone()
    {
        return new LogicalPiece(team, type) { hasMoved = hasMoved };
    }
}