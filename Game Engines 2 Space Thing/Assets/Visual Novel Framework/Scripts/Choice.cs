using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

[System.Serializable]
public class Choice {
    public const float Size = 24f;
    public const float Padding = 50f;
    public const int Break = -2;

    [SerializeField] public NodeJoint joint;
    [SerializeField] public Rect bounds;
    public bool active;
    public string choiceText, oldChoiceText;
}
