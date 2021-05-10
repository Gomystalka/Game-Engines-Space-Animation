using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>
public class NodeRenderer
{
    private readonly Dictionary<StyledElement, GUIStyle> _styles;

    public NodeRenderer(Dictionary<StyledElement, GUIStyle> styles) {
        _styles = styles;
    }

    #region Node Drawing

    public virtual void DrawNode(Node node)
    {
        switch (node.type)
        {
            case NodeType.Page:
                DrawPageNode(node);
                break;
            case NodeType.State:
                DrawStateNode(node);
                break;
            case NodeType.Choice:
                DrawChoiceNode(node);
                break;
            default:
                break;
        }
    }

    public virtual void DrawJoints(Node node, Vector2 mousePosition)
    {
        switch (node.type)
        {
            case NodeType.Page:
                DrawPageJoints(node, mousePosition);
                break;
            case NodeType.State:
                DrawStateJoints(node, mousePosition);
                break;
            case NodeType.Choice:
                DrawChoiceJoints(node, mousePosition);
                break;
            default:
                break;
        }
        GUIStyle s = new GUIStyle(GUI.skin.box);
        s.normal.textColor = Color.yellow;
        s.alignment = TextAnchor.MiddleCenter;
        s.fontSize = 32;

        if (VisualNovelEditor.ShowDebugFaces)
        {
            foreach (NodeJoint joint in node.joints)
            {
                GUI.Label(new Rect(joint.bounds.center.x - 25, joint.bounds.center.y - 25, 50, 50), (joint.connectionPoint != null && joint.connectionPoint.IsValid()) ? ":3" : ":(", s);
            }
        }
    }

    public virtual void DrawExtras(Node node)
    {
        switch (node.type)
        {
            case NodeType.Page:
                DrawPageExtras(node);
                break;
            case NodeType.State:
                DrawStateExtras(node);
                break;
            case NodeType.Choice:
                DrawChoiceExtras(node);
                break;
            default:
                break;
        }
    }

    #region Page Node

    private void DrawPageNode(Node node)
    {
        if (_styles == null) return;
        GUILayout.BeginVertical(_styles[StyledElement.Box]);

        GUILayout.Label("Character", _styles[StyledElement.Label]);
        node.characterScrollPosition = GUILayout.BeginScrollView(node.characterScrollPosition);
        node.character = GUILayout.TextField(node.character, _styles[StyledElement.TextField]);
        GUILayout.EndScrollView();

        if (node.oldCharacter != node.character) {
            node.oldCharacter = node.character;
            node.ResetSize();
        }

        GUILayout.Label("Page Text", _styles[StyledElement.Label]);
        node.scrollPosition = GUILayout.BeginScrollView(node.scrollPosition);
        node.text = GUILayout.TextArea(node.text, _styles[StyledElement.TextArea]);
        GUILayout.EndScrollView();
        if (node.oldText != node.text) {
            node.oldText = node.text;
            node.ResetSize();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Panel Color", _styles[StyledElement.Label]);
        node.panelColor = EditorGUILayout.ColorField(node.panelColor);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Text Speed", _styles[StyledElement.Label]);
        node.speed = EditorGUILayout.FloatField(node.speed, _styles[StyledElement.TextField]);
        GUILayout.EndHorizontal();

        GUILayout.Space(VisualNovelEditor.EditorWindowTabHeight / 2f);

        GUILayout.Label("Background", _styles[StyledElement.Label]);
        GUILayout.BeginHorizontal();
        //node.background = (Texture2D)EditorGUILayoutExtensions.ObjectFieldPlus<Texture2D>(node.background, typeof(Texture2D), false, _styles[StyledElement.ObjectField], 32134, GUILayout.Width(150f));
        node.background = (Texture2D)EditorGUILayout.ObjectField(node.background, typeof(Texture2D), false, GUILayout.Width(150f));
        EditorGUILayoutExtensions.BeginBackgroundColorGroup(new Color32(72, 72, 72, 255));

        EditorGUI.BeginDisabledGroup(!node.background);
        if (GUILayout.Button(!node.backgroundPreviewShown ? EditorGUIUtility.IconContent("icon dropdown") : new GUIContent(""), _styles[StyledElement.Button], GUILayout.Width(18f), GUILayout.Height(18f)))
        {
            node.backgroundPreviewShown = !node.backgroundPreviewShown;
        }
        EditorGUI.EndDisabledGroup();

        if (node.backgroundPreviewShown)
            if (!node.background)
                node.backgroundPreviewShown = false;

        EditorGUILayoutExtensions.EndBackgroundColorGroup();

        GUILayout.EndHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("Character Sprite", _styles[StyledElement.Label]);
        //node.characterSprite = (Texture2D)EditorGUILayoutExtensions.ObjectFieldPlus<Texture2D>(node.characterSprite, typeof(Texture2D), false, _styles[StyledElement.ObjectField], 75342, GUILayout.Width(150f));
        node.characterSprite = (Sprite)EditorGUILayout.ObjectField(node.characterSprite, typeof(Sprite), false, GUILayout.Width(150f));
        if (node.characterSprite != null)
        {
            bool oldState = node.offsetCharacter;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset Character Position", _styles[StyledElement.Label]);
            node.offsetCharacter = GUILayout.Toggle(node.offsetCharacter, "");
            GUILayout.EndHorizontal();
            if (oldState != node.offsetCharacter) {
                oldState = node.offsetCharacter;
                node.ResetSize();
            }
            if (node.offsetCharacter)
            {
                node.characterOffset = EditorGUILayout.Vector2Field("", node.characterOffset);
            }
            GUILayout.Label("Character Size", _styles[StyledElement.Label]);
            node.characterSize = EditorGUILayout.Vector2Field("", node.characterSize);
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

    private void DrawPageJoints(Node node, Vector2 mousePosition)
    {
        foreach (NodeJoint j in node.joints)
        {
            j.ownerID = node.Index;
            j.ownerNode = node;
            if (j.IsConnected())
            {
                if (j.IsOutput)
                {
                    NodeJoint jnt = null;
                    if (j.connectionPoint.ownerNode != null)
                    {
                        jnt = j.connectionPoint.ownerNode.joints[j.connectionPoint.JointIndex];
                    }
                    else
                    {
                        jnt = VisualNovelEditor.FindJoint(j.connectionPoint.OwnerID, j.connectionPoint.JointIndex);
                    }
                    if (jnt != null)
                    {
                        CalculateAndDrawBezier(j.bounds.center, jnt.bounds.center, j.info, jnt.info);
                    }
                }
                GUI.Label(new Rect(new Vector2(j.bounds.position.x + j.bounds.width + 10f, j.bounds.position.y), new Vector2(50, 50)), j.connectionPoint.OwnerID + " " + (j.IsOutput ? "OUT" : "IN"), EditorGUILayoutExtensions.ChangeTextColor(_styles[StyledElement.Label], Color.white));
            }
        }
        EditorGUI.DrawRect(node.TopJoint.bounds, node.Outputting && !node.TopJoint.IsConnected() ? Color.magenta : node.TopJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);
        EditorGUI.DrawRect(node.BottomJoint.bounds, node.Outputting && !node.BottomJoint.IsConnected() ? Color.magenta : node.BottomJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);
        EditorGUI.DrawRect(node.LeftJoint.bounds, node.Outputting && !node.LeftJoint.IsConnected() ? Color.magenta : node.LeftJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);
        EditorGUI.DrawRect(node.RightJoint.bounds, node.Outputting && !node.RightJoint.IsConnected() ? Color.magenta : node.RightJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);

    }

    private void DrawPageExtras(Node node)
    {
        if (node.backgroundPreviewShown && node.background)
        {
            EditorGUI.DrawPreviewTexture(new Rect(node.window.x, node.window.y + node.window.height + 10f, node.window.width, node.window.height), node.background);
        }
    }
    #endregion

    #region Choice Node
    //private SerializedObject _obj;
    private bool c_canResetSize;

    private void DrawChoiceNode(Node n) {
        DrawPageNode(n);
        GUILayout.BeginVertical(_styles[StyledElement.Box], GUILayout.ExpandHeight(false));
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        n.choicesShown = EditorGUILayout.Foldout(n.choicesShown, "Choices", EditorGUILayoutExtensions.ChangeTextColor(foldoutStyle, Color.white));
        if (n.choices == null)
        {
            n.choices = new Choice[n.choiceCount];
        }
        else {
            if (n.choiceCount != n.oldChoiceCount) {
                n.oldChoiceCount = n.choiceCount;
                n.ResetSize();
            }
        }
        if (n.choicesShown)
        {
            c_canResetSize = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(8f);
            GUILayout.Label("Choice Count", _styles[StyledElement.Label]);
            n.choiceCount = EditorGUILayout.IntField(n.choiceCount, _styles[StyledElement.TextField], GUILayout.MaxWidth(30f));
            n.choiceCount = Mathf.Clamp(n.choiceCount, 1, Node.ChoiceLimit);

            if (n.choiceCount != n.oldChoiceCount && n.oldChoiceCount != 0)
            {
                //Debug.Log("CHANGE FROM " + n.oldChoiceCount + " TO " + n.choiceCount);
                n.oldChoiceCount = n.choiceCount;
                NodeJoint[] lostJoints = n.CalculateLostChoices();
                foreach (NodeJoint joint in lostJoints)
                {
                    VisualNovelEditor.ResetNodeJoint(joint);
                }
            }
            else if (n.oldChoiceCount == 0) {
                n.oldChoiceCount = n.choiceCount;
            }
            GUILayout.EndHorizontal();
            for (int i = 0; i < n.choiceCount; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(8f);
                GUILayout.Label("Choice " + (i + 1), _styles[StyledElement.Label]);
                n.choices[i].choiceText = GUILayout.TextField(n.choices[i].choiceText ?? "", _styles[StyledElement.TextField], GUILayout.MaxWidth(100f), GUILayout.MaxHeight(_styles[StyledElement.TextField].fixedHeight));
                if (n.choices[i].choiceText != n.choices[i].oldChoiceText) {
                    n.choices[i].oldChoiceText = n.choices[i].choiceText;
                    n.ResetSize();
                }
                GUILayout.EndHorizontal();
            }
        }
        else {
            if (c_canResetSize)
            {
                c_canResetSize = false;
                n.ResetSize();
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawChoiceJoints(Node node, Vector2 mousePosition) {
        NodeJoint connectedJoint = null;
        foreach (NodeJoint j in node.joints)
        {
            if (j == null) continue;
            j.ownerID = node.Index;
            j.ownerNode = node;
            if (j.IsConnected())
            {
                if (j.IsOutput)
                {
                    NodeJoint jnt = null;
                    if (j.connectionPoint.ownerNode != null)
                    {
                        jnt = j.connectionPoint.ownerNode.joints[j.connectionPoint.JointIndex];
                    }
                    else
                    {
                        jnt = VisualNovelEditor.FindJoint(j.connectionPoint.OwnerID, j.connectionPoint.JointIndex);
                    }
                    if (jnt != null)
                    {
                        //Choice c = node.FindChoiceFromJoint(j);
                            CalculateAndDrawBezier(j.bounds.center, jnt.bounds.center, j.info, jnt.info);
                    }
                }
                
                if(!j.isChoice) 
                    connectedJoint = j;


                GUI.Label(new Rect(new Vector2(j.bounds.position.x + j.bounds.width + 10f, j.bounds.position.y), new Vector2(50, 50)), j.connectionPoint.OwnerID + " " + (j.IsOutput ? "OUT" : "IN"), EditorGUILayoutExtensions.ChangeTextColor(_styles[StyledElement.Label], Color.white));
            }
        }
        if (node.TopJoint == connectedJoint || connectedJoint == null)
        {
            EditorGUI.DrawRect(node.TopJoint.bounds, node.Outputting && !node.TopJoint.IsConnected() ? Color.magenta : node.TopJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);
            node.TopJoint.disabled = false;
        }
        else {
            if (!node.TopJoint.disabled)
                node.TopJoint.disabled = true;
        }
        if (node.LeftJoint == connectedJoint || connectedJoint == null)
        {
            EditorGUI.DrawRect(node.LeftJoint.bounds, node.Outputting && !node.LeftJoint.IsConnected() ? Color.magenta : node.LeftJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);
            node.LeftJoint.disabled = false;
        }
        else {
            if (!node.LeftJoint.disabled)
                node.LeftJoint.disabled = true;
        }
        if (node.RightJoint == connectedJoint || connectedJoint == null)
        {
            EditorGUI.DrawRect(node.RightJoint.bounds, node.Outputting && !node.RightJoint.IsConnected() ? Color.magenta : node.RightJoint.CheckForHover(mousePosition) ? Color.yellow : Color.red);
            node.RightJoint.disabled = false;
        }
        else {
            if (!node.RightJoint.disabled)
                node.RightJoint.disabled = true;
        }

        node.BottomJoint.disabled = true;

        DrawChoices(node, mousePosition);
    }

    private void DrawChoiceExtras(Node node) {
        if(node.backgroundPreviewShown && node.background)
        {
            EditorGUI.DrawPreviewTexture(new Rect(node.window.x, node.window.y + node.window.height + Choice.Size + 20f, node.startWindow.width + 18f, node.startWindow.width), node.background);
        }
    }

    private void DrawChoices(Node node, Vector2 mousePosition) {
        float x = node.window.center.x - (Choice.Size / 2f);
        float y = node.window.y + node.window.height + 10f;

        //foreach (Choice c in node.choices) { c.active = false; }

        for (int i = 0; i < node.choiceCount; i++)
        {
            x += Choice.Size + Choice.Padding;
            node.choices[i].bounds = new Rect(x - ((Choice.Size + Choice.Padding) / 2f) * (node.choiceCount + 1), y, Choice.Size, Choice.Size);
            node.choices[i].active = true;
            if (node.choices[i].joint != null) {
                node.choices[i].joint.bounds = node.choices[i].bounds;
            }
           // bool hover = node.choices[i].bounds.Contains(mousePosition);
            EditorGUI.DrawRect(node.choices[i].bounds, node.choices[i].joint.CheckForHover(mousePosition) ? Color.yellow : ColorExtensions.Orange);

            GUI.Label((i + 1) > 9 ? node.choices[i].bounds : new Rect(node.choices[i].bounds.x + 1, node.choices[i].bounds.y, node.choices[i].bounds.width, node.choices[i].bounds.height), (i + 1) + "", EditorGUILayoutExtensions.ChangeFontSettings(_styles[StyledElement.Label], Color.white, -1, TextAnchor.MiddleCenter));
        }

        UpdateChoicePaths(node);
    }

    private void UpdateChoicePaths(Node node) {
        int startIndex = 4;
        for (int i = 0; i < node.choices.Length; i++) {
            node.joints[startIndex].bounds = node.choices[i].bounds;
            node.choices[i].joint = node.joints[startIndex];
            startIndex++;
        }
    }
    #endregion

    #region State Node

    private void DrawStateNode(Node node)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(node.isStartNode ? "Start" : node.isEndNode ? "Exit" : "???", _styles[StyledElement.StateNode]);
        GUILayout.EndHorizontal();
    }

    private void DrawStateJoints(Node node, Vector2 mousePosition)
    {
        if (node.isStartNode)
        {
            node.BottomJoint.ownerID = node.Index;
            node.BottomJoint.ownerNode = node;
            EditorGUI.DrawRect(node.BottomJoint.bounds, node.BottomJoint.CheckForHover(mousePosition) ? Color.green : Color.blue);
            if (node.BottomJoint.IsConnected())
            {
                if (node.BottomJoint.IsOutput)
                {
                    NodeJoint jnt = null;
                    if (node.BottomJoint.connectionPoint.ownerNode != null)
                    {
                        jnt = node.BottomJoint.connectionPoint.ownerNode.joints[node.BottomJoint.connectionPoint.JointIndex];
                    }
                    else
                    {
                        jnt = VisualNovelEditor.FindJoint(node.BottomJoint.connectionPoint.OwnerID, node.BottomJoint.connectionPoint.JointIndex);
                    }
                    if (jnt != null)
                        CalculateAndDrawBezier(node.BottomJoint.bounds.center, jnt.bounds.center, node.BottomJoint.info, jnt.info);
                }
            }
        }
        else //End Node
        {
            node.TopJoint.ownerID = node.Index;
            node.TopJoint.ownerNode = node;
            EditorGUI.DrawRect(node.TopJoint.bounds, node.TopJoint.CheckForHover(mousePosition) ? Color.cyan : Color.grey);
            if (node.TopJoint.IsConnected())
            {
                if (node.TopJoint.IsOutput)
                {
                    NodeJoint jnt = null;
                    if (node.TopJoint.connectionPoint.ownerNode != null) {
                        jnt = node.TopJoint.connectionPoint.ownerNode.joints[node.TopJoint.connectionPoint.JointIndex];
                    } else {
                        jnt = VisualNovelEditor.FindJoint(node.TopJoint.connectionPoint.OwnerID, node.TopJoint.connectionPoint.JointIndex);
                    }
                    if (jnt != null)
                        CalculateAndDrawBezier(node.TopJoint.bounds.center, jnt.bounds.center, node.TopJoint.info, jnt.info);
                }
            }
        }
    }

    private void DrawStateExtras(Node node)
    {
        //Stub
    }

    #endregion

    //There's probably some formula for this but I couldn't think of one...
    public void CalculateAndDrawBezier(Vector2 current, Vector2 target, NodeJoint.NodeInfo jointInfo, NodeJoint.NodeInfo connectedJointInfo)
    {
        float dist = Vector2.Distance(current, target) / 2f;
        float currentDeg = 0, targetDeg = 0;

        switch (jointInfo)
        {
            case NodeJoint.NodeInfo.Top:
                if (connectedJointInfo == NodeJoint.NodeInfo.Top)
                {
                    currentDeg = 270;
                    targetDeg = 90;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Bottom)
                {
                    currentDeg = 270;
                    targetDeg = 270;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Left)
                {
                    currentDeg = 270;
                    targetDeg = 0;
                }
                else
                {
                    currentDeg = 270;
                    targetDeg = 180;
                }
                break;
            case NodeJoint.NodeInfo.Bottom:
                if (connectedJointInfo == NodeJoint.NodeInfo.Top)
                {
                    currentDeg = 90;
                    targetDeg = 90;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Bottom)
                {
                    currentDeg = 90;
                    targetDeg = 270;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Left)
                {
                    currentDeg = 90;
                    targetDeg = 0;
                }
                else
                {
                    currentDeg = 90;
                    targetDeg = 180;
                }
                break;
            case NodeJoint.NodeInfo.Left:
                if (connectedJointInfo == NodeJoint.NodeInfo.Top)
                {
                    currentDeg = 180;
                    targetDeg = 90;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Bottom)
                {
                    currentDeg = 180;
                    targetDeg = 270;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Left)
                {
                    currentDeg = 180;
                    targetDeg = 0;
                }
                else
                {
                    currentDeg = 180;
                    targetDeg = 180;
                }
                break;
            default:
                if (connectedJointInfo == NodeJoint.NodeInfo.Top)
                {
                    currentDeg = 0;
                    targetDeg = 90;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Bottom)
                {
                    currentDeg = 0;
                    targetDeg = 270;
                }
                else if (connectedJointInfo == NodeJoint.NodeInfo.Left)
                {
                    currentDeg = 0;
                    targetDeg = 0;
                }
                else
                {
                    currentDeg = 0;
                    targetDeg = 180;
                }
                break;
        }

        Vector2 currentTan = Vector2.zero;
        currentTan = currentTan.Rotate(current + Vector2.right * dist, current, Mathf.Deg2Rad * currentDeg);
        Vector2 targetTan = Vector2.zero;
        targetTan = targetTan.Rotate(target + Vector2.left * dist, target, Mathf.Deg2Rad * targetDeg);

        Handles.DrawBezier(current, target, currentTan, targetTan, Color.green, null, 5f);

        if (VisualNovelEditor.DrawTangents)
        {
            Handles.DrawBezier(current, currentTan, current, currentTan, Color.yellow, null, 3f);
            Handles.DrawBezier(target, targetTan, target, targetTan, Color.yellow, null, 3f);
        }
    }

    #endregion
}
