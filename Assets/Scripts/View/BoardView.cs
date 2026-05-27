using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Referencias Prefabs")]
    public GameObject squarePrefab;
    public GameObject piecePrefab; // <-- NUEVO: Arrastra aquí el VisualPiece

    [Header("Configuración")]
    public MatchConfig matchConfig;
    public PieceTheme pieceTheme; // <-- NUEVO: Arrastra aquí el ClassicTheme

    [Header("Estética")]
    public Color lightSquareColor = new Color(0.9f, 0.9f, 0.8f);
    public Color darkSquareColor = new Color(0.3f, 0.5f, 0.3f);

    private BoardModel logicalBoard; // brain

    private void Start()
    {
        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();

        DrawBoard();
        DrawPieces();
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
            }
        }
    }

    private void DrawPieces()
    {
        bool isWhite = matchConfig.isPlayingWhite;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                LogicalPiece pieceData = logicalBoard.grid[x, y];

                if (pieceData != null)
                {
                    // 1. Instanciamos el visual
                    GameObject pieceGo = Instantiate(piecePrefab, this.transform);

                    // 2. Calculamos posición (exactamente igual que las casillas)
                    int visualX = isWhite ? x : 7 - x;
                    int visualY = isWhite ? y : 7 - y;
                    pieceGo.transform.position = new Vector2(visualX - 3.5f, visualY - 3.5f);

                    // 3. Le ponemos el "disfraz" correcto usando el Theme
                    SpriteRenderer sr = pieceGo.GetComponent<SpriteRenderer>();
                    sr.sprite = pieceTheme.GetSprite(pieceData.type, pieceData.team);

                    pieceGo.name = $"{pieceData.team}_{pieceData.type}_{x}_{y}";
                }
            }
        }
    }
}