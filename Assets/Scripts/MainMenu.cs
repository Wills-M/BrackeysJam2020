using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        SelectOption(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            // Subtraction that loops around to options.count - 1 if it goes below 0
            int newOption = (options.Count + selectedOption - 1) % options.Count ;
            SelectOption(newOption);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Addition that loops around to 0 if it goes above options.count - 1
            int newOption = (selectedOption + 1) % options.Count;
            SelectOption(newOption);
        }
    }

    private void SelectOption(int optionNumber)
    {
        // Set selected colors
        selectedOption = optionNumber;
        foreach(Text text in options)
        {
            text.color = deselectedColor;
        }
        options[optionNumber].color = selectedColor;

        // Move player menu
        playerMenu.transform.SetParent(options[optionNumber].transform, false);
    }

}
