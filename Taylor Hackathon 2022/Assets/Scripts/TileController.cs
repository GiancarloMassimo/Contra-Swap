using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField]
    float swapSpeed;

    Animator anim;

    public Vector2 DesiredPosition { get; set; }

    void Start()
    {
        DesiredPosition = transform.position;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, DesiredPosition, swapSpeed * Time.deltaTime);
    }

    public void OnSelect()
    {
        anim.SetTrigger("Shrink");
    }

    public void OnDeselect()
    {
        anim.SetTrigger("Grow");
    }
}
