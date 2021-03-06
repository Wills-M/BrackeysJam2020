﻿using System.Collections;
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
    private Graphic pauseMenu;

    [SerializeField]
    private Graphic menuScreen;

    public bool paused = false;

    [SerializeField]
    private SoundType pauseGame;

    public void SetPaused(bool paused)
    {
        this.paused = paused;

        // Play sound effect when game paused
        if (paused)
            SoundController.Instance.PlaySoundEffect(pauseGame);
        // Close controls panel when exiting pause menu
        else
            SetControlsPanelActive(false);

        // Pause/resume time
        Time.timeScale = paused ? 0f : 1f;

        // Hide/show menu
        menuScreen.gameObject.SetActive(paused);

        // Focus/unfocus background music
        SoundController.Instance.FocusMusicVolume(!paused);
    }

    protected override void HandleInput()
    {
        // Ignore input unless paused
        if(paused)
            base.HandleInput();
    }

    protected override void SetControlsPanelActive(bool active)
    {
        base.SetControlsPanelActive(active);
        pauseMenu.gameObject.SetActive(!active);
    }

    public void ResetButton()
    {
        SetPaused(false);
        LevelManager.Instance.ResetLevel();
    }

    public override void ExitButton()
    {
        SetPaused(false);
        SceneManager.LoadScene("MainMenu");
    }
}
