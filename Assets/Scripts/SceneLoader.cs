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
        Player.finishLoadingScene = false;
        yield return null;

        loading = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);
        loadingValue = 0;

        while (loadingSlider.value != 1)
        {
            float progress = Mathf.Clamp01(loading.progress / 0.9f);
            loadingValue = Mathf.MoveTowards(loadingValue, progress, loadingSpeed * Time.deltaTime); //, ref loadingVelocity, 0.01f <<== SmoothDamp
            loadingSlider.value = Mathf.Clamp01(loadingValue);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        yield return new WaitForSeconds(loadingDelay);

        LightingBehavior.targetSize = 0;
        Player.finishLoadingScene = true;

        Destroy(this.gameObject);
    }
}
