using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
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
        PlayerPrefs.SetString("GameMode", "Survival");
        int round = PlayerPrefs.GetInt("Round");
        int nextScene = round <= 10 ? 5 :
        10 < round && round < 50 ? 6 : 7;
        loader.LoadScene(nextScene);
    }

    public void InProgress()
    {
        loader.LoadScene(10);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}
