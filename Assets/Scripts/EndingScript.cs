using System.Collections;
using UnityEngine;

public class EndingScript : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;
    [SerializeField] private float delay;
    [SerializeField] private float creditsLength;

    private bool processing;

    private void Update()
    {
        if (GetComponent<RectTransform>().anchoredPosition.y >= creditsLength && !processing) StartCoroutine(ReturnToTitleScreen());
    }

    private IEnumerator ReturnToTitleScreen()
    {
        processing = true;
        yield return new WaitForSeconds(delay);
        loader.LoadScene(0);
    }
}
