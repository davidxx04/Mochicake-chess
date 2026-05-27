using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Referencias")]
    // To connect data from the ScriptableObject
    public MatchConfig matchConfig;

    // When pressing the "Play vs Computer" button
    public void PlayVsComputer()
    {
        matchConfig.isVsComputer = true;
        LoadNextScene();
    }

    // When pressing the "Play vs Player" button
    public void PlayVsPlayer()
    {
        matchConfig.isVsComputer = false;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // Le decimos a Unity que cargue una escena llamada "ChooseColor"
        SceneManager.LoadScene("ChooseColor");
    }
}