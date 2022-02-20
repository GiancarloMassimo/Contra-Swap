using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameSumCondition : MonoBehaviour, Condition
{
    [SerializeField]
    Color failColor, passColor;

    [SerializeField]
    SpriteRenderer colorIndicator;

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

    public bool MeetsCondition(int[] arr)
    {
        return true;
    }

    public void AdjustColorIndicator(bool pass)
    {
        if (pass)
        {
            colorIndicator.color = passColor;
        }
        else
        {
            colorIndicator.color = failColor;
        }
    }

}
