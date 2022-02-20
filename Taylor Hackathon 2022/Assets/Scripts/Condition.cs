using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Condition
{
    public Vector2 DesiredPosition { get; set; }

    bool MeetsCondition(int[] arr);
    void OnSelect();
    void OnDeselect();
    public void AdjustColorIndicator(bool pass);
}
