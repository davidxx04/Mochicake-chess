using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Referencias Prefabs")]
    public GameObject squarePrefab;
    public GameObject piecePrefab;

    [Header("Configuracion")]
    public MatchConfig matchConfig;
    public PieceTheme pieceTheme;

    [Header("Interaccion Visual")]
    public Color highlightColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
    public Color validMoveColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
    [SerializeField] private GameObject validMoveMarkerPrefab;

    [Header("Seleccion")]
    [SerializeField] private float selectedLiftOffset = 0.4f;
    [SerializeField] private GameObject selectionPieceGlowPrefab;
    [SerializeField] private GameObject selectionSquareGlowPrefab;

    [Header("Animacion")]
    [SerializeField] private float moveDuration = 0.32f;
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float maxTiltDegrees = 10f;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private AnimationCurve moveEaseCurve;

    private GameObject[,] visualSquares = new GameObject[8, 8];
    private GameObject[,] visualPieces = new GameObject[8, 8];
    private readonly Dictionary<GameObject, Coroutine> activeAnimations = new Dictionary<GameObject, Coroutine>();
    private bool isWhitePlayer;
    private GameObject currentSelectedPiece;
    private Vector3 originalSelectedPos;
    private GameObject currentPieceGlow;
    private GameObject currentSquareGlow;
    private readonly List<GameObject> validMoveMarkers = new List<GameObject>();

    private void Awake()
    {
        if (jumpCurve == null || jumpCurve.length == 0)
        {
            jumpCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(0.5f, 1f, 0f, 0f),
                new Keyframe(1f, 0f, -2f, 0f));
        }

        if (moveEaseCurve == null || moveEaseCurve.length == 0)
        {
            moveEaseCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.75f, 1.06f, 1.5f, 0f),
                new Keyframe(1f, 1f, 0f, 0f));
        }
    }

    public void InitializeView(BoardModel model)
    {
        isWhitePlayer = matchConfig.isPlayingWhite;
        DrawBoard();
        DrawPieces(model);
    }

    private void DrawBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject square = Instantiate(squarePrefab, transform);
                square.transform.position = GetRealWorldPosition(x, y);

                SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
                sr.color = Color.clear;

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
                    GameObject pieceGo = Instantiate(piecePrefab, transform);
                    pieceGo.transform.position = GetRealWorldPosition(x, y);

                    SpriteRenderer sr = pieceGo.GetComponent<SpriteRenderer>();
                    sr.sprite = pieceTheme.GetSprite(pieceData.type, pieceData.team);

                    pieceGo.name = $"{pieceData.team}_{pieceData.type}_{x}_{y}";
                    visualPieces[x, y] = pieceGo;
                }
            }
        }
    }

    public void HighlightSquare(int logicalX, int logicalY)
    {
        SpriteRenderer sr = visualSquares[logicalX, logicalY].GetComponent<SpriteRenderer>();
        sr.color = highlightColor;
    }

    public void HighlightValidMoves(List<Move> validMoves)
    {
        ClearValidMoveMarkers();
        if (validMoveMarkerPrefab == null)
            return;

        for (int i = 0; i < validMoves.Count; i++)
        {
            Move move = validMoves[i];
            GameObject marker = GetValidMoveMarker(i);
            if (marker == null)
                continue;

            marker.transform.position = visualSquares[move.targetX, move.targetY].transform.position;
            marker.transform.rotation = Quaternion.identity;
            marker.SetActive(true);

            SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
            if (sr != null)
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
                sr.color = Color.clear;
            }
        }

        ClearValidMoveMarkers();
    }

    public void ElevateSelectedPiece(int x, int y)
    {
        ResetSelectedPiece();

        GameObject piece = visualPieces[x, y];
        if (piece == null)
            return;

        currentSelectedPiece = piece;
        originalSelectedPos = piece.transform.position;
        piece.transform.position = originalSelectedPos + new Vector3(0f, selectedLiftOffset, 0f);

        if (selectionPieceGlowPrefab != null)
        {
            currentPieceGlow = Instantiate(selectionPieceGlowPrefab, piece.transform);
            currentPieceGlow.transform.localPosition = Vector3.zero;
            currentPieceGlow.transform.localRotation = Quaternion.identity;
            currentPieceGlow.transform.localScale = Vector3.one;

            SpriteRenderer pieceRenderer = piece.GetComponent<SpriteRenderer>();
            SpriteRenderer glowRenderer = currentPieceGlow.GetComponent<SpriteRenderer>();
            if (pieceRenderer != null && glowRenderer != null)
            {
                glowRenderer.sortingLayerID = pieceRenderer.sortingLayerID;
                glowRenderer.sortingOrder = pieceRenderer.sortingOrder - 1;
            }
        }

        if (selectionSquareGlowPrefab != null)
        {
            GameObject square = visualSquares[x, y];
            if (square != null)
            {
                currentSquareGlow = Instantiate(selectionSquareGlowPrefab, transform);
                currentSquareGlow.transform.position = square.transform.position;
                currentSquareGlow.transform.rotation = Quaternion.identity;
            }
        }
    }

    public void ResetSelectedPiece()
    {
        if (currentSelectedPiece != null)
            currentSelectedPiece.transform.position = originalSelectedPos;

        currentSelectedPiece = null;

        if (currentPieceGlow != null)
            Destroy(currentPieceGlow);
        currentPieceGlow = null;

        if (currentSquareGlow != null)
            Destroy(currentSquareGlow);
        currentSquareGlow = null;
    }

    private GameObject GetValidMoveMarker(int index)
    {
        if (validMoveMarkerPrefab == null)
            return null;

        while (validMoveMarkers.Count <= index)
        {
            GameObject marker = Instantiate(validMoveMarkerPrefab, transform);
            marker.SetActive(false);
            validMoveMarkers.Add(marker);
        }

        return validMoveMarkers[index];
    }

    private void ClearValidMoveMarkers()
    {
        for (int i = 0; i < validMoveMarkers.Count; i++)
        {
            if (validMoveMarkers[i] != null)
                validMoveMarkers[i].SetActive(false);
        }
    }

    public void UpdateVisualPiece(int startX, int startY, int targetX, int targetY, LogicalPiece logicalPiece)
    {
        if (startX != targetX || startY != targetY)
        {
            GameObject movingPiece = visualPieces[startX, startY];
            if (movingPiece == null)
                return;

            Vector3 startPos = movingPiece.transform.position;
            Vector3 targetPos = GetRealWorldPosition(targetX, targetY);

            GameObject capturedPiece = visualPieces[targetX, targetY];
            if (capturedPiece != null)
            {
                Vector3 capturePos = capturedPiece.transform.position;
                CancelAnimation(capturedPiece);
                if (FXManager.Instance != null)
                    FXManager.Instance.PlayCaptureParticles(capturePos);
                Destroy(capturedPiece);
            }

            visualPieces[targetX, targetY] = movingPiece;
            visualPieces[startX, startY] = null;

            UpdatePieceSprite(movingPiece, logicalPiece);
            StartPieceAnimation(movingPiece, startPos, targetPos);
        }
        else if (visualPieces[targetX, targetY] != null)
        {
            UpdatePieceSprite(visualPieces[targetX, targetY], logicalPiece);
        }
    }

    public void DestroyVisualPiece(int x, int y)
    {
        if (visualPieces[x, y] == null)
            return;

        GameObject capturedPiece = visualPieces[x, y];
        Vector3 capturePos = capturedPiece.transform.position;
        CancelAnimation(capturedPiece);
        if (FXManager.Instance != null)
            FXManager.Instance.PlayCaptureParticles(capturePos);
        Destroy(capturedPiece);
        visualPieces[x, y] = null;
    }

    private void StartPieceAnimation(GameObject piece, Vector3 startPos, Vector3 targetPos)
    {
        CancelAnimation(piece);
        activeAnimations[piece] = StartCoroutine(AnimatePiece(piece, startPos, targetPos));
    }

    private void CancelAnimation(GameObject piece)
    {
        if (piece == null || !activeAnimations.TryGetValue(piece, out Coroutine running))
            return;

        StopCoroutine(running);
        activeAnimations.Remove(piece);
        ResetPieceTransform(piece);
    }

    private static void ResetPieceTransform(GameObject piece)
    {
        if (piece == null) return;
        piece.transform.rotation = Quaternion.identity;
    }

    private IEnumerator AnimatePiece(GameObject piece, Vector3 startPos, Vector3 targetPos)
    {
        float baseRotationZ = piece.transform.eulerAngles.z;
        float tiltSign = GetTiltSign(startPos, targetPos);

        try
        {
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                if (piece == null)
                    yield break;

                float t = elapsed / moveDuration;
                float easedT = moveEaseCurve.Evaluate(t);
                float arcT = jumpCurve.Evaluate(t);

                Vector3 pos = Vector3.Lerp(startPos, targetPos, easedT);
                pos.y += arcT * jumpHeight;
                piece.transform.position = pos;
                piece.transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ + arcT * maxTiltDegrees * tiltSign);

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (piece != null)
            {
                piece.transform.position = targetPos;
                piece.transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);
            }
        }
        finally
        {
            activeAnimations.Remove(piece);
        }
    }

    private static float GetTiltSign(Vector3 startPos, Vector3 targetPos)
    {
        float dx = targetPos.x - startPos.x;
        if (Mathf.Abs(dx) > 0.01f)
            return Mathf.Sign(dx);

        float dy = targetPos.y - startPos.y;
        return Mathf.Abs(dy) > 0.01f ? Mathf.Sign(dy) : 1f;
    }

    private void UpdatePieceSprite(GameObject piece, LogicalPiece logicalPiece)
    {
        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
        sr.sprite = pieceTheme.GetSprite(logicalPiece.type, logicalPiece.team);
    }

    private Vector2 GetRealWorldPosition(int logicalX, int logicalY)
    {
        int visualX = isWhitePlayer ? logicalX : 7 - logicalX;
        int visualY = isWhitePlayer ? logicalY : 7 - logicalY;
        return new Vector2(visualX - 3.5f, visualY - 3.5f);
    }
}
