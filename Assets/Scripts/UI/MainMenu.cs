﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private List<Text> options;
    [SerializeField]
    private Color selectedColor;
    [SerializeField]
    private Color deselectedColor;

    private int selectedOption;

    [SerializeField]
    private GameObject playerMenu;
    
    [SerializeField]
    private float pushTime;
    [SerializeField]
    private float pushSpeed;

    [SerializeField]
    protected GameObject controlsPanel;
    protected bool controlsOpen;

    [Header("Sound Effects")]

    [SerializeField]
    private SoundType moveCursor;

    [SerializeField]
    private SoundType selectOption;

    private void Start()
    {
        controlsOpen = false;

        // Select first option without playing selection sound
        SelectOption(0, false);
    }

    private void Update()
    {
        if (controlsOpen)
        {
            if (Input.anyKeyDown)
            {
                SetControlsPanelActive(false);
            }
        }
        else
        {
            HandleInput();
        }
    }

    protected virtual void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Subtraction that loops around to options.count - 1 if it goes below 0
            int newOption = (options.Count + selectedOption - 1) % options.Count;
            SelectOption(newOption);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Addition that loops around to 0 if it goes above options.count - 1
            int newOption = (selectedOption + 1) % options.Count;
            SelectOption(newOption);
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            SoundController.Instance.PlaySoundEffect(selectOption);
            options[selectedOption].GetComponent<Button>().onClick.Invoke();
        }
    }

    private void SelectOption(int optionNumber, bool playSound = true)
    {
        if(playSound)
            SoundController.Instance.PlaySoundEffect(moveCursor);

        // Set selected colors
        selectedOption = optionNumber;
        foreach (Text text in options)
        {
            text.color = deselectedColor;
        }
        options[optionNumber].color = selectedColor;

        // Move player menu
        if(playerMenu)
            playerMenu.transform.SetParent(options[optionNumber].transform, false);
    }

    private Coroutine pushButtonCoroutine;
    private IEnumerator PushButton(Transform button)
    {
        // Animate player pushing button
        Animator anim = playerMenu.GetComponent<Animator>();
        anim.SetTrigger("Push");

        // Move them across screen
        for (float timer = 0f; timer < pushTime; timer += Time.deltaTime)
        {
            button.position += Vector3.right * pushSpeed * Time.deltaTime;
            yield return null;
        }
        pushButtonCoroutine = null;
    }

    private IEnumerator StartCoroutine()
    {
        pushButtonCoroutine = StartCoroutine(PushButton(options[selectedOption].transform));
        while (pushButtonCoroutine != null)
            yield return null;

        LevelManager.Instance.NextLevel();
    }

    private IEnumerator ExitCoroutine()
    {
        pushButtonCoroutine = StartCoroutine(PushButton(options[selectedOption].transform));
        while (pushButtonCoroutine != null)
            yield return null;

        Application.Quit();
    }

    public void StartButton()
    {
        StartCoroutine(StartCoroutine());
    }

    public void ControlButton()
    {
        SetControlsPanelActive(true);
    }

    public virtual void ExitButton()
    {
        StartCoroutine(ExitCoroutine());
    }

    protected virtual void SetControlsPanelActive(bool active)
    {
        controlsPanel.SetActive(active);
        controlsOpen = active;
    }

}
