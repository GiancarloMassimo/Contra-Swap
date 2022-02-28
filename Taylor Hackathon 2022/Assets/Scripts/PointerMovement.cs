using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerMovement : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
