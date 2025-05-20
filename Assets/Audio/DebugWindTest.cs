using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindTest : MonoBehaviour
{
    public WindVisualController wind;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            wind.ShowWind();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            wind.HideWind();
        }
    }
}

