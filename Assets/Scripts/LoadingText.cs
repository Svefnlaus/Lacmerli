using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    [SerializeField] private float delay;

    private void Start()
    {
        StartCoroutine(loadingTextAnim());
    }

    private IEnumerator loadingTextAnim()
    {
        string[] loadText = { "Loading", "Loading.", "Loading..", "Loading..." };

        while (true)
        {
            for (int current = 0; current < loadText.Length; current++)
            {
                GetComponent<TMP_Text>().SetText(loadText[current]);
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
