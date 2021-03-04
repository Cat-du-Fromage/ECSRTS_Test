using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class SelectionCanvasMono : MonoBehaviour
{
    public static SelectionCanvasMono instance;

    public RectTransform selectionBox;

    private void Awake()
    {
        instance = this; // Allow to take object/rectTransform from this class
    }
}
