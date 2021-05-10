using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

[System.Serializable]
[CreateAssetMenu(fileName = "New Visual Novel", menuName = "Visual Novel")]
public class VisualNovel : ScriptableObject
{
    public string author;
    public bool hasData;
    public int nodeCount;
    public bool valid;

    [SerializeField] public Node[] nodes;
    [SerializeField] public int[] nodePath;

    private void OnEnable()
    {
        //hideFlags = HideFlags.DontSave;
        //hasData = Nodes == null && Nodes.Count > 0;
    }

    public void Clear()
    {
        nodes = new Node[0];
        nodePath = new int[0];
        nodeCount = 0;
        valid = false;
        hasData = false;
        author = "";
    }
}
