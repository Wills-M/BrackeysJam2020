using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MainMenu
{
    #region Singleton

    public static PauseMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    [SerializeField]
    private Graphic menuPanel;

    public bool paused = false;

    public void SetPaused(bool paused)
    {
        this.paused = paused;

        // Pause/resume time
        Time.timeScale = paused ? 0f : 1f;

        // Hide/show menu
        menuPanel.gameObject.SetActive(paused);

        // Close controls panel when exiting pause menu
        if(!paused)
            SetControlsPanelActive(false);
    }

    protected override void HandleInput()
    {
        base.HandleInput();

        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(false);
    }

    public void ResetButton()
    {
        if (LevelManager.Instance)
            LevelManager.Instance.ResetLevel();
        else // This invalidates the reason to have a levelmanager but I don't feel like adding levelmanager to every scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
