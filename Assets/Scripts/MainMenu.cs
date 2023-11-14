using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenu : ButtonManager
{
    [SerializeField] private Animator mainMenu;
    [SerializeField] private Animator settings;
    [SerializeField] private Animator selectionWindow;
    [SerializeField] private AudioSource whoosh;

    private static bool windowIsUp;
    private static bool settingsIsUp;
    private static bool selectingGameMode;

    private void Awake()
    {
        windowIsUp = false;
        selectingGameMode = false;
    }

    private void LateUpdate()
    {
        if (!windowIsUp && Input.anyKeyDown) FadeIn();
        else if (windowIsUp && !selectingGameMode && !settingsIsUp && Input.GetKeyDown(KeyCode.Escape)) FadeOut();
        else if (windowIsUp && selectingGameMode && Input.GetKeyDown(KeyCode.Escape)) Return();
        else if (windowIsUp && settingsIsUp && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void FadeOut()
    {
        whoosh.Play();
        mainMenu.SetTrigger("CloseMenu");
        windowIsUp = false;
    }

    public void FadeIn()
    {
        whoosh.Play();
        mainMenu.SetTrigger("OpenMenu");
        windowIsUp = true;
    }

    public void Open()
    {
        whoosh.Play();
        settings.SetTrigger("OpenSettings");
        settingsIsUp = true;
    }

    public void Close()
    {
        whoosh.Play();
        settings.SetTrigger("CloseSettings");
        settingsIsUp = false;
    }

    public void PopUp()
    {
        whoosh.Play();
        selectionWindow.SetTrigger("OpenSelection");
        selectingGameMode = true;
    }

    public void Return()
    {
        whoosh.Play();
        selectionWindow.SetTrigger("CloseSelection");
        selectingGameMode = false;
    }
}
