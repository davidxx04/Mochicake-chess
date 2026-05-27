using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseColorController : MonoBehaviour
{
    [Header("Referencias")]
    public MatchConfig matchConfig;

    private bool isLeftCardWhite;

    private void Start()
    {
        isLeftCardWhite = Random.value > 0.5f;
    }

    public void SelectLeftCard()
    {
        matchConfig.isPlayingWhite = isLeftCardWhite;
        LoadBoardScene();
    }

    public void SelectRightCard()
    {
        matchConfig.isPlayingWhite = !isLeftCardWhite;
        LoadBoardScene();
    }

    private void LoadBoardScene()
    {
        SceneManager.LoadScene("Board");
    }
}