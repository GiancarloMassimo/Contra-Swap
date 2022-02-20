using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    AudioSource win, lose, swap, select;

    public void Win()
    {
        win.Play();
    }
    public void Lose()
    {
        lose.Play();
    }
    public void Swap()
    {
        swap.Play();
    }
    public void Select()
    {
        select.Play();
    }
}
