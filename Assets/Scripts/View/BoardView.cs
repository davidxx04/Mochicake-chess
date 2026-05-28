using UnityEngine;
using System.Collections.Generic; // Necesario para usar List<>

public class BoardView : MonoBehaviour
{
    [Header("Referencias Prefabs")]
    public GameObject squarePrefab;
    public GameObject piecePrefab;

    [Header("Configuración")]
    public MatchConfig matchConfig;
    public PieceTheme pieceTheme;

    [Header("Estética")]
    public Color lightSquareColor = new Color(0.9f, 0.9f, 0.8f);
    public Color darkSquareColor = new Color(0.3f, 0.5f, 0.3f);

    [Header("Interacción Visual")]
    public Color highlightColor = new Color(0.4f, 0.8f, 0.4f); // Verde para la pieza seleccionada
    public Color validMoveColor = new Color(0.4f, 0.7f, 0.9f); // Azulito para a dónde puede ir

    private GameObject[,] visualSquares = new GameObject[8, 8];

    public void InitializeView(BoardModel model)
    {
        DrawBoard();
        DrawPieces(model);
    }

    private void DrawBoard()
    {
        bool isWhite = matchConfig.isPlayingWhite;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square = Instantiate(squarePrefab, this.transform);

                int visualX = isWhite ? x : 7 - x;
                int visualY = isWhite ? y : 7 - y;
                square.transform.position = new Vector2(visualX - 3.5f, visualY - 3.5f);

                bool isLightSquare = (x + y) % 2 != 0;
                SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
                sr.color = isLightSquare ? lightSquareColor : darkSquareColor;

                square.name = $"Square_{x}_{y}";
                visualSquares[x, y] = square; // Guardamos la referencia para iluminarla luego
            }
        }
    }

    private void DrawPieces(BoardModel model)
    {
        bool isWhite = matchConfig.isPlayingWhite;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece pieceData = model.grid[x, y];

                if (pieceData != null)
                {
                    GameObject pieceGo = Instantiate(piecePrefab, this.transform);

                    int visualX = isWhite ? x : 7 - x;
                    int visualY = isWhite ? y : 7 - y;
                    pieceGo.transform.position = new Vector2(visualX - 3.5f, visualY - 3.5f);

                    SpriteRenderer sr = pieceGo.GetComponent<SpriteRenderer>();
                    sr.sprite = pieceTheme.GetSprite(pieceData.type, pieceData.team);

                    pieceGo.name = $"{pieceData.team}_{pieceData.type}_{x}_{y}";
                }
            }
        }
    }

    // --- NUEVOS MÉTODOS DE ILUMINACIÓN ---

    public void HighlightSquare(int logicalX, int logicalY)
    {
        SpriteRenderer sr = visualSquares[logicalX, logicalY].GetComponent<SpriteRenderer>();
        sr.color = highlightColor;
    }

    public void HighlightValidMoves(List<Vector2Int> validMoves)
    {
        foreach (Vector2Int move in validMoves)
        {
            SpriteRenderer sr = visualSquares[move.x, move.y].GetComponent<SpriteRenderer>();
            sr.color = validMoveColor;
        }
    }

    public void ResetAllSquareColors()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                bool isLightSquare = (x + y) % 2 != 0;
                SpriteRenderer sr = visualSquares[x, y].GetComponent<SpriteRenderer>();
                sr.color = isLightSquare ? lightSquareColor : darkSquareColor;
            }
        }
    }
}