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

    private void Start()
    {
        SelectOption(0);
    }

    private void SelectOption(int optionNumber)
    {
        foreach(Text text in options)
        {
            text.color = deselectedColor;
        }
        options[optionNumber].color = selectedColor;
    }

}
