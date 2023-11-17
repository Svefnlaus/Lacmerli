using UnityEngine;

public class PauseMenu : ButtonManager
{
    private Animator animator;
    private AudioSource soundFX;
    private bool menuIsUp;
    private bool rcwIsUp;
    private bool mmcwIsUp;
    private bool ecwIsUp;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        soundFX = GetComponent<AudioSource>();
        menuIsUp = false;
        rcwIsUp = false;
        mmcwIsUp = false;
        ecwIsUp = false;
    }

    private void Update()
    {
        if (Player.isDead) return;
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
        soundFX.Play();
        menuIsUp = true;
    }

    public void Resume()
    {
        // Resume
        Time.timeScale = 1;

        animator.SetTrigger("CloseMenu");
        soundFX.Play();
        menuIsUp = false;
    }

    // RCW - Restart Confirmation Window
    public void OpenRCW()
    {
        animator.SetTrigger("OpenRCW");
        soundFX.Play();
        rcwIsUp = true;
    }

    public void CloseRCW()
    {
        animator.SetTrigger("CloseRCW");
        soundFX.Play();
        rcwIsUp = false;
    }

    // MMCW - Main Menu Confirmation Window
    public void OpenMMCW()
    {
        animator.SetTrigger("OpenMMCW");
        soundFX.Play();
        mmcwIsUp = true;
    }

    public void CloseMMCW()
    {
        animator.SetTrigger("CloseMMCW");
        soundFX.Play();
        mmcwIsUp = false;
    }

    // ECW - Exit Confirmation Window
    public void OpenECW()
    {
        animator.SetTrigger("OpenECW");
        soundFX.Play();
        ecwIsUp = true;
    }

    public void CloseECW()
    {
        animator.SetTrigger("CloseECW");
        soundFX.Play();
        ecwIsUp = false;
    }
}
