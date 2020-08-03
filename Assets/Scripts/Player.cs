using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            Move(new Vector2(-1, 0));
        if (Input.GetKeyDown(KeyCode.D))
            Move(new Vector2(1, 0));
    }

}
