using System.Collections;
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
    private GameObject controlsPanel;
    private bool controlsOpen;

    private void Start()
    {
        controlsOpen = false;
        SelectOption(0);
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
            if (Input.GetKeyDown(KeyCode.W))
            {
                // Subtraction that loops around to options.count - 1 if it goes below 0
                int newOption = (options.Count + selectedOption - 1) % options.Count;
                SelectOption(newOption);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                // Addition that loops around to 0 if it goes above options.count - 1
                int newOption = (selectedOption + 1) % options.Count;
                SelectOption(newOption);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                options[selectedOption].GetComponent<Button>().onClick.Invoke();
            }
        }
    }

    private void SelectOption(int optionNumber)
    {
        // Set selected colors
        selectedOption = optionNumber;
        foreach (Text text in options)
        {
            text.color = deselectedColor;
        }
        options[optionNumber].color = selectedColor;

        // Move player menu
        playerMenu.transform.SetParent(options[optionNumber].transform, false);
    }

    private IEnumerator StartCoroutine()
    {
        Animator anim = playerMenu.GetComponent<Animator>();

        anim.SetTrigger("Push");

        Transform startTransform = options[selectedOption].transform;

        for (float timer = 0f; timer < pushTime; timer += Time.deltaTime)
        {
            startTransform.position += Vector3.right * pushSpeed;
            yield return null;
        }

        LevelManager.Instance.NextLevel();
    }

    private IEnumerator ExitCoroutine()
    {
        Animator anim = playerMenu.GetComponent<Animator>();

        anim.SetTrigger("Push");

        Transform exitTransform = options[selectedOption].transform;

        for (float timer = 0f; timer < pushTime; timer += Time.deltaTime)
        {
            exitTransform.position += Vector3.right * pushSpeed;
            yield return null;
        }

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

    public void ExitButton()
    {
        StartCoroutine(ExitCoroutine());
    }

    private void SetControlsPanelActive(bool active)
    {
        controlsPanel.SetActive(active);
        controlsOpen = active;
    }

}
