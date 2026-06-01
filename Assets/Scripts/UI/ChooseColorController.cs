using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseColorController : MonoBehaviour
{
    [Header("Referencias")]
    public MatchConfig matchConfig;

    [Header("Cartas")]
    [SerializeField] private CardFlipper leftCardFlipper;
    [SerializeField] private CardFlipper rightCardFlipper;
    [SerializeField] private Sprite whiteKingSprite;
    [SerializeField] private Sprite blackKingSprite;
    [SerializeField] private bool revealBoth = true;

    [Header("Escena")]
    [SerializeField] private string boardSceneName = "Board";

    private bool isLeftCardWhite;
    private bool hasSelected;

    private void Start()
    {
        isLeftCardWhite = Random.value > 0.5f;
    }

    public void SelectLeftCard()
    {
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayClick();
        if (hasSelected)
            return;
        hasSelected = true;

        matchConfig.isPlayingWhite = isLeftCardWhite;
        Sprite leftSprite = isLeftCardWhite ? whiteKingSprite : blackKingSprite;
        Sprite rightSprite = isLeftCardWhite ? blackKingSprite : whiteKingSprite;

        if (revealBoth && rightCardFlipper != null)
            rightCardFlipper.FlipCard(rightSprite, null);

        if (leftCardFlipper != null)
            leftCardFlipper.FlipCard(leftSprite, LoadBoardScene);
        else
            LoadBoardScene();
    }

    public void SelectRightCard()
    {
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayClick();
        if (hasSelected)
            return;
        hasSelected = true;

        matchConfig.isPlayingWhite = !isLeftCardWhite;
        Sprite rightSprite = isLeftCardWhite ? blackKingSprite : whiteKingSprite;
        Sprite leftSprite = isLeftCardWhite ? whiteKingSprite : blackKingSprite;

        if (revealBoth && leftCardFlipper != null)
            leftCardFlipper.FlipCard(leftSprite, null);

        if (rightCardFlipper != null)
            rightCardFlipper.FlipCard(rightSprite, LoadBoardScene);
        else
            LoadBoardScene();
    }

    private void LoadBoardScene()
    {
        SceneManager.LoadScene(boardSceneName);
    }

}