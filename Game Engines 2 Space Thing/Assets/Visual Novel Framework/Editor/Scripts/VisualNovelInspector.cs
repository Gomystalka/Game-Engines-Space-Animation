using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

[CustomEditor(typeof(VisualNovel))]
public class VisualNovelInspector : Editor
{
    public const int NovelSeparationMin = 0;
    private bool _nodePathToggle, _separateNovels;
    private int _novelCount = NovelSeparationMin, _novelCountEnd;
    private Vector2 _scrollPosition = Vector2.zero;
    private GUIStyle _style, _arrowStyle;
    private VisualNovelEditor _editorWindow;
    /*
    private readonly GUIStyle _style = new GUIStyle(GUI.skin.window)
    {
        alignment = TextAnchor.MiddleCenter
    };
    private readonly GUIStyle _arrowStyle = new GUIStyle(GUI.skin.label)
    {
        alignment = TextAnchor.MiddleCenter
    };
    */

    public override void OnInspectorGUI()
    {
        if (_style == null || _arrowStyle == null)
        {
            _style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter
            };
            _arrowStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        if (target is VisualNovel visualNovel)
        {
            //base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Author", visualNovel.author);
            EditorGUILayout.Toggle("Has Data", visualNovel.hasData);
            EditorGUILayout.Toggle("Node Path is Valid", visualNovel.valid);
            EditorGUILayout.IntField("Node Count", visualNovel.nodeCount);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Open in Editor"))
            {
                if (!_editorWindow)
                    OpenEditor(visualNovel);
            }


            _separateNovels = EditorGUILayout.Foldout(_separateNovels, "Separate Novels");

            if (_separateNovels) {
                _novelCount = EditorGUILayout.IntField("Start", _novelCount);
                _novelCountEnd = EditorGUILayout.IntField("Count", _novelCountEnd);
                _novelCount = Mathf.Clamp(_novelCount, NovelSeparationMin, visualNovel.nodes.Length - 1);
                _novelCountEnd = Mathf.Clamp(_novelCountEnd, NovelSeparationMin, visualNovel.nodes.Length - 1);
                if (GUILayout.Button("Separate")) {
                    SeparateNovelsAt(_novelCount, _novelCountEnd);
                }
            }
            _nodePathToggle = EditorGUILayout.Foldout(_nodePathToggle, "Node Path");
            //GUILayout.Label("Node Path");
            if (_nodePathToggle)
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                GUILayout.BeginVertical();
                if (visualNovel.valid && visualNovel && visualNovel.nodePath != null && (visualNovel.nodePath[0] == Choice.Break || visualNovel.nodes[visualNovel.nodePath[0]] != null))
                {
                    for (int i = 0; i < visualNovel.nodePath.Length; i++)
                    {
                        GUILayout.Label(visualNovel.nodePath[i] == Choice.Break ? "Choice End" : visualNovel.nodes[visualNovel.nodePath[i]].nodeName, _style, GUILayout.ExpandWidth(true));
                        if (i != visualNovel.nodePath.Length - 1)
                        {
                            GUILayout.Label("V", _arrowStyle, GUILayout.ExpandWidth(true));
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No valid Node Path found.", MessageType.Warning, true);
                    //GUILayout.Label("No Node Path found...");
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }
    }

    private void CreateNovel(int index, Node[] nodes) {
        VisualNovel novel = CreateInstance<VisualNovel>();
        novel.nodes = nodes;
        novel.nodeCount = nodes.Length;
        novel.hasData = index == 0 ? true : false;
        novel.valid = false;
        novel.author = ((VisualNovel)target).author;
        string extension = ".asset";
        string path = AssetDatabase.GetAssetPath(target);
        string name = System.IO.Path.GetFileName(path);
        path = path.Replace(name, "");
        name = name.Replace(extension, "");
        string newPath = AssetDatabase.GenerateUniqueAssetPath(path + name + "_" + index + extension);
        AssetDatabase.CreateAsset(novel, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void SeparateNovelsAt(int start, int count) {
        Node[] originalNodes = ((VisualNovel)target).nodes;
        Node endNode = originalNodes[originalNodes.Length - 1];
        foreach (NodeJoint j in endNode.joints) {
            VisualNovelEditor.ResetNodeJoint(j);
            endNode.window = new Rect(400, 400, endNode.window.width, endNode.window.height);
        }
        Node[] newNodes = Splice(originalNodes, 0, count, endNode);

        Node[] otherNodes = Splice(originalNodes, start, originalNodes.Length - (start), null);
        Debug.Log("New: " + newNodes.Length + " Other: " + otherNodes.Length);
        CreateNovel(0, newNodes);
        CreateNovel(1, otherNodes);
    }

    private Node[] Splice(Node[] array, int start, int count, Node addition) {
        List<Node> nodes = new List<Node>(array);
        nodes = nodes.GetRange(start, count);
        if (addition != null) {
            nodes.Add(addition);
        }
        return nodes.ToArray();
    }

    private void OpenEditor(VisualNovel visualNovel)
    {
        _editorWindow = EditorWindow.GetWindow<VisualNovelEditor>();
        _editorWindow.LoadVisualNovel(visualNovel);
        _editorWindow.Show();
    }
}