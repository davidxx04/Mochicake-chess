using UnityEngine;

public class MatchController : MonoBehaviour
{
    [Header("Referencias")]
    public BoardView boardView;

    private BoardModel logicalBoard; // brain (model)

    private void Start()
    {
        // controller creates the brain
        logicalBoard = new BoardModel();
        logicalBoard.SetupClassicBoard();

        // controller also controls the view (that´s why it´s a controller xd)
        boardView.InitializeView(logicalBoard);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        // transforms click into world coordinates
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // screen coordinates -> visual board coordinates -> logical board coordinates
            float clickedX = hit.collider.transform.position.x;
            float clickedY = hit.collider.transform.position.y;

            int visualX = Mathf.RoundToInt(clickedX + 3.5f);
            int visualY = Mathf.RoundToInt(clickedY + 3.5f);

            bool isWhite = boardView.matchConfig.isPlayingWhite;
            int logicalX = isWhite ? visualX : 7 - visualX;
            int logicalY = isWhite ? visualY : 7 - visualY;

            LogicalPiece clickedPiece = logicalBoard.grid[logicalX, logicalY];

            if (clickedPiece != null)
            {
                Debug.Log($"¡Has tocado a: {clickedPiece.team} {clickedPiece.type} en la coordenada lógica [{logicalX}, {logicalY}]!");
            }
            else
            {
                Debug.Log($"Casilla vacía en [{logicalX}, {logicalY}]");
            }
        }
    }
}