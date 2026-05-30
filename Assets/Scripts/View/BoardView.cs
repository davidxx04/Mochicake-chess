using UnityEngine;
using System.Collections.Generic;

public class BoardView : MonoBehaviour
{
    [Header("Referencias Prefabs")]
    public GameObject squarePrefab;
    public GameObject piecePrefab;

    [Header("Configuración")]
    public MatchConfig matchConfig;
    public PieceTheme pieceTheme;

    [Header("Interacción Visual")]
    public Color highlightColor = new Color(0.4f, 0.8f, 0.4f, 0.8f); // Verde semitransparente
    public Color validMoveColor = new Color(0.4f, 0.7f, 0.9f, 0.8f); // Azul semitransparente

    // --- ESTADO INTERNO ---
    private GameObject[,] visualSquares = new GameObject[8, 8];
    private GameObject[,] visualPieces = new GameObject[8, 8];
    private bool isWhitePlayer; // Variable global cacheada (¡Mejora de rendimiento!)

    public void InitializeView(BoardModel model)
    {
        isWhitePlayer = matchConfig.isPlayingWhite; // Lo leemos una sola vez
        DrawBoard();
        DrawPieces(model);
    }

    private void DrawBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square = Instantiate(squarePrefab, this.transform);
                square.transform.position = GetRealWorldPosition(x, y); // Uso del Helper

                SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
                sr.color = Color.clear; // Invisible por defecto (dejamos ver el asset)

                square.name = $"Square_{x}_{y}";
                visualSquares[x, y] = square;
            }
        }
    }

    private void DrawPieces(BoardModel model)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece pieceData = model.grid[x, y];

                if (pieceData != null)
                {
                    GameObject pieceGo = Instantiate(piecePrefab, this.transform);
                    pieceGo.transform.position = GetRealWorldPosition(x, y); // Uso del Helper

                    SpriteRenderer sr = pieceGo.GetComponent<SpriteRenderer>();
                    sr.sprite = pieceTheme.GetSprite(pieceData.type, pieceData.team);

                    pieceGo.name = $"{pieceData.team}_{pieceData.type}_{x}_{y}";
                    visualPieces[x, y] = pieceGo;
                }
            }
        }
    }

    // --- INTERACCIÓN Y MOVIMIENTO ---

    public void HighlightSquare(int logicalX, int logicalY)
    {
        SpriteRenderer sr = visualSquares[logicalX, logicalY].GetComponent<SpriteRenderer>();
        sr.color = highlightColor;
    }

    public void HighlightValidMoves(List<Move> validMoves)
    {
        foreach (Move move in validMoves)
        {
            SpriteRenderer sr = visualSquares[move.targetX, move.targetY].GetComponent<SpriteRenderer>();
            sr.color = validMoveColor;
        }
    }

    public void ResetAllSquareColors()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                SpriteRenderer sr = visualSquares[x, y].GetComponent<SpriteRenderer>();
                sr.color = Color.clear; // Las apagamos volviéndolas invisibles
            }
        }
    }

    public void UpdateVisualPiece(int startX, int startY, int targetX, int targetY, LogicalPiece logicalPiece)
    {
        // 1. Solo hacemos la lógica de movimiento y destrucción si la pieza realmente cambia de casilla
        if (startX != targetX || startY != targetY)
        {
            if (visualPieces[targetX, targetY] != null)
            {
                Destroy(visualPieces[targetX, targetY]);
            }

            GameObject movingPiece = visualPieces[startX, startY];
            visualPieces[targetX, targetY] = movingPiece;
            visualPieces[startX, startY] = null;

            movingPiece.transform.position = GetRealWorldPosition(targetX, targetY);
        }

        // 2. Pase lo que pase (se haya movido o solo haya promocionado en el sitio), 
        // le actualizamos el disfraz a la pieza que está en el destino.
        if (visualPieces[targetX, targetY] != null)
        {
            SpriteRenderer sr = visualPieces[targetX, targetY].GetComponent<SpriteRenderer>();
            sr.sprite = pieceTheme.GetSprite(logicalPiece.type, logicalPiece.team);
        }
    }
    // --- HELPER METODS (DRY) ---

    // Este método concentra todas las matemáticas de rotación y centrado de la cámara
    private Vector2 GetRealWorldPosition(int logicalX, int logicalY)
    {
        int visualX = isWhitePlayer ? logicalX : 7 - logicalX;
        int visualY = isWhitePlayer ? logicalY : 7 - logicalY;
        return new Vector2(visualX - 3.5f, visualY - 3.5f);
    }

    // only used for pawn Passant captures
    public void DestroyVisualPiece(int x, int y)
    {
        if (visualPieces[x, y] != null)
        {
            Destroy(visualPieces[x, y]);
            visualPieces[x, y] = null;
        }
    }
}