using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Check((Vector2)transform.position + Vector2.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Check((Vector2)transform.position + Vector2.right);
        }
    }

    void Check(Vector2 position)
    {
        Collider2D col = Physics2D.OverlapPoint(position);
        if(col)
        {
            Debug.LogFormat("Collided with {0} at position {1}", col.name, position);
        }
    }
}
