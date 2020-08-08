using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TurnCounter : Singleton<TurnCounter>
{
    private Text counterText;

    public void SetTurn(int turn)
    {
        if(!counterText) counterText = GetComponent<Text>();
        counterText.text = "Turn: " + turn;
    }
}
