using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Image outcomeImage;
    [SerializeField] private Button playAgainButton;

    [Header("Sprites")]
    [SerializeField] private Sprite victorySprite;
    [SerializeField] private Sprite drawSprite;

    [Header("Colores")]
    [SerializeField] private Color whiteWinColor = new Color(0.95f, 0.93f, 0.85f, 1f);
    [SerializeField] private Color blackWinColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("Escena")]
    [SerializeField] private string restartSceneName = "MainMenu";

    private Coroutine runningAnimation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.OnCheckmate += HandleCheckmate;
            turnManager.OnStalemate += HandleStalemate;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnCheckmate -= HandleCheckmate;
            turnManager.OnStalemate -= HandleStalemate;
        }
    }

    private void OnDestroy()
    {
        if (playAgainButton != null)
            playAgainButton.onClick.RemoveListener(RestartGame);
    }

    public void RestartGame()
    {
        if (!string.IsNullOrEmpty(restartSceneName))
            SceneManager.LoadScene(restartSceneName);
    }

    public void ShowVictoryScreen(TeamColor winner)
    {
        if (winnerText != null)
        {
            winnerText.text = winner == TeamColor.White ? "WHITE WINS!" : "BLACK WINS!";
            winnerText.color = winner == TeamColor.White ? whiteWinColor : blackWinColor;
        }

        if (outcomeImage != null)
            outcomeImage.sprite = victorySprite;

        ShowPanel();
    }

    public void ShowVictoryScreen(string winnerName)
    {
        if (winnerText != null)
            winnerText.text = winnerName + " WINS!";

        if (outcomeImage != null)
            outcomeImage.sprite = victorySprite;

        ShowPanel();
    }

    public void ShowDrawScreen()
    {
        if (winnerText != null)
            winnerText.text = "DRAW";

        if (outcomeImage != null)
            outcomeImage.sprite = drawSprite;

        ShowPanel();
    }

    private void HandleCheckmate(TeamColor winner)
    {
        ShowVictoryScreen(winner);
    }

    private void HandleStalemate()
    {
        ShowDrawScreen();
    }

    private void ShowPanel()
    {
        if (victoryPanel == null || canvasGroup == null || contentContainer == null)
            return;

        victoryPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        contentContainer.localScale = Vector3.one * 0.8f;

        if (runningAnimation != null)
            StopCoroutine(runningAnimation);
        runningAnimation = StartCoroutine(AnimatePanel());
    }

    private IEnumerator AnimatePanel()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            if (contentContainer != null)
                contentContainer.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        if (contentContainer != null)
            contentContainer.localScale = Vector3.one;
    }

    private static string FormatWinnerName(TeamColor winner)
    {
        return winner == TeamColor.White ? "WHITE" : "BLACK";
    }
}
