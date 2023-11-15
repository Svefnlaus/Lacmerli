using UnityEngine;

public class PauseMenu : ButtonManager
{
    private Animator animator;
    private bool menuIsUp;
    private bool rcwIsUp;
    private bool mmcwIsUp;
    private bool ecwIsUp;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        menuIsUp = false;
        rcwIsUp = false;
        mmcwIsUp = false;
        ecwIsUp = false;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (!menuIsUp) Pause();
        else if (rcwIsUp) CloseRCW();
        else if (mmcwIsUp) CloseMMCW();
        else if (ecwIsUp) CloseECW();
        else Resume();
    }

    public void Pause()
    {
        // Pause
        Time.timeScale = 0;

        animator.SetTrigger("OpenMenu");
        menuIsUp = true;
    }

    public void Resume()
    {
        // Resume
        Time.timeScale = 1;

        animator.SetTrigger("CloseMenu");
        menuIsUp = false;
    }

    // RCW - Restart Confirmation Window
    public void OpenRCW()
    {
        animator.SetTrigger("OpenRCW");
        rcwIsUp = true;
    }

    public void CloseRCW()
    {
        animator.SetTrigger("CloseRCW");
        rcwIsUp = false;
    }

    // MMCW - Main Menu Confirmation Window
    public void OpenMMCW()
    {
        animator.SetTrigger("OpenMMCW");
        mmcwIsUp = true;
    }

    public void CloseMMCW()
    {
        animator.SetTrigger("CloseMMCW");
        mmcwIsUp = false;
    }

    // ECW - Exit Confirmation Window
    public void OpenECW()
    {
        animator.SetTrigger("OpenECW");
        ecwIsUp = true;
    }

    public void CloseECW()
    {
        animator.SetTrigger("CloseECW");
        ecwIsUp = false;
    }
}
