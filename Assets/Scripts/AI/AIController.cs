using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private MatchController matchController;
    [SerializeField] [Range(1, 4)] private int searchDepth = 3;

    private CancellationTokenSource searchCts;
    private Coroutine searchCoroutine;

    public bool IsThinking { get; private set; }

    private void Awake()
    {
        if (matchController == null)
            matchController = GetComponent<MatchController>();
    }

    private void OnDestroy()
    {
        CancelSearch();
    }

    public void RequestMove(TeamColor aiColor)
    {
        if (matchController == null)
        {
            Debug.LogError("[AIController] matchController no asignado.");
            return;
        }

        if (!matchController.IsVsComputerActive())
            return;

        CancelSearch();
        searchCoroutine = StartCoroutine(RequestMoveCoroutine(aiColor));
    }

    public void CancelSearch()
    {
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
            searchCoroutine = null;
        }

        searchCts?.Cancel();
        searchCts?.Dispose();
        searchCts = null;
        IsThinking = false;

        if (matchController != null)
            matchController.SetAiThinking(false);
    }

    private IEnumerator RequestMoveCoroutine(TeamColor aiColor)
    {
        searchCts = new CancellationTokenSource();
        CancellationToken token = searchCts.Token;

        IsThinking = true;
        matchController.SetAiThinking(true);

        BoardModel clone = matchController.CloneBoard();
        int depth = searchDepth;

        SearchResult result = default;
        bool completed = false;
        Exception searchError = null;

        Task.Run(() =>
        {
            try
            {
                var positionHistory = matchController.GetPositionHistory();
                result = ChessEngine.FindBestMove(clone, aiColor, depth, positionHistory, token);
            }
            catch (OperationCanceledException)
            {
                // Búsqueda cancelada.
            }
            catch (Exception ex)
            {
                searchError = ex;
            }
            finally
            {
                completed = true;
            }
        }, token);

        while (!completed)
            yield return null;

        IsThinking = false;
        matchController.SetAiThinking(false);
        searchCoroutine = null;

        if (searchError != null)
        {
            Debug.LogException(searchError);
            yield break;
        }

        if (token.IsCancellationRequested)
            yield break;

        if (!matchController.ShouldApplyAiMove(aiColor))
            yield break;

        if (result.hasMove)
            matchController.ApplyAiMove(result);
        else
            Debug.LogWarning("[AIController] La búsqueda no devolvió ningún movimiento legal.");
    }
}
