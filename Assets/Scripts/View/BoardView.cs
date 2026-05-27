using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject squarePrefab; 
    public MatchConfig matchConfig; // to get player color

    [Header("Estética")]
    public Color lightSquareColor = new Color(0.9f, 0.9f, 0.8f); // white bone
    public Color darkSquareColor = new Color(0.3f, 0.5f, 0.3f);  // green classic chess

    private void Start()
    {
        DrawBoard();
    }

    private void DrawBoard()
    {
        bool isWhite = matchConfig.isPlayingWhite;

        // to draw the squares
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square = Instantiate(squarePrefab, this.transform);

                // Calculates the camera rotation based on the player´s color
                int visualX = isWhite ? x : 7 - x;
                int visualY = isWhite ? y : 7 - y;

                // to center the board in the screen, so (0,0) means the center of the board
                float posX = visualX - 3.5f;
                float posY = visualY - 3.5f;

                square.transform.position = new Vector2(posX, posY);

                // to color every other square
                bool isLightSquare = (x + y) % 2 != 0;

                // paint
                SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
                sr.color = isLightSquare ? lightSquareColor : darkSquareColor;

                square.name = $"Square_{x}_{y}";
            }
        }
    }
}