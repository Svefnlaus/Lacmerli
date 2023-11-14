using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;

    private int currentScene { get { return SceneManager.GetActiveScene().buildIndex; } }
    private int previousScene { get { return PlayerPrefs.HasKey("PreviousScene") ? PlayerPrefs.GetInt("PreviousScene") : 0; } }

    private void Awake()
    {
        Time.timeScale = 1;
    }

    public void ReturnToMenu()
    {
        loader.LoadScene(0);
    }

    public void ResetScene()
    {
        loader.LoadScene(currentScene);
    }

    public void Retry()
    {
        loader.LoadScene(previousScene);
    }

    public void PlayTutorialMode()
    {
        PlayerPrefs.SetString("GameMode", "Tutorial");
        loader.LoadScene(1);
    }

    public void PlayArcadeMode()
    {
        PlayerPrefs.SetString("GameMode", "Arcade");
        loader.LoadScene(2);
    }

    public void PlayLightsOutMode()
    {
        PlayerPrefs.SetInt("Round", 1);
        PlayerPrefs.SetString("GameMode", "Survival");
        loader.LoadScene(3);
    }

    public void InProgress()
    {
        loader.LoadScene(8);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}
