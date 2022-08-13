using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryUI : MonoBehaviour
{
    public void HandleUpdate(Action onSelected, Action onBack)
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
        else if(Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
    }
}
