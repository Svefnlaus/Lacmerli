using System.Collections;
using TMPro;
using Unity.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private float loadingDelay;
    [SerializeField] private float loadingSpeed;

    private AsyncOperation loading;

    private bool destroy;
    private float loadingVelocity;
    private float loadingValue;

    private void Awake()
    {
        destroy = false;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (destroy) Destroy(this.gameObject);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(Load(sceneIndex));
    }

    private IEnumerator Load(int sceneIndex)
    {
        Time.timeScale = 1;

        yield return new WaitForSeconds(0.01f);

        Player.finishLoadingScene = false;
        loading = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);
        loadingValue = 0;

        while (loadingSlider.value != 1 || !loading.isDone)
        {
            float progress = Mathf.Clamp01(loading.progress / 0.9f);
            loadingValue = Mathf.SmoothDamp(loadingValue, progress, ref loadingVelocity, 0.01f, loadingSpeed);
            loadingSlider.value = loadingValue;
            yield return null;
        }

        yield return new WaitForSeconds(loadingDelay);
        Player.finishLoadingScene = true;
        destroy = true;
        loadingScreen.SetActive(false);
    }
}
