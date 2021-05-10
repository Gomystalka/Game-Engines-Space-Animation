using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

[System.Serializable]
public class Node
{
    [System.NonSerialized] public List<Node> nodes = new List<Node>();
    public const int ChoiceLimit = 10;
    public static int LastNodeIndex { get; set; }
    public static float EditorWindowTabHeight = 21f;
    public static Vector2 Size = new Vector2(180, 120);
    public static Vector2 JointSize = new Vector2(10f, 10f);

    public bool ShowDebugFaces { get; set; }
    public bool DrawTangents { get; set; }

    public string nodeName;
    public Rect window, startWindow;

    //public Dictionary<StyledElement, GUIStyle> Styles { get; set; }

    public string Title { get; set; }
    public int Index { get; set; }

    public bool isStartNode;
    public bool isEndNode;
    public NodeType type;

    //Page Node
    public Vector2 scrollPosition = Vector2.zero, characterScrollPosition = Vector2.zero;
    public string text = "Your text goes here...", character = "Character goes here...", oldText, oldCharacter;
    public Color panelColor = Color.magenta;
    public float speed = 0.5f;
    public Texture2D background;
    public Sprite characterSprite;
    public Vector2 characterOffset;
    public Vector2 characterSize = Vector2.one;
    public bool offsetCharacter;
    public bool backgroundPreviewShown;

    //Choice Node
    public int choiceCount, oldChoiceCount;
    public bool choicesShown;
    [SerializeField] public Choice[] choices = new Choice[ChoiceLimit];

    [SerializeField] public List<NodeJoint> connectionOrder;

    public bool Outputting
    {
        get
        {
            foreach (NodeJoint j in joints)
            {
                if (j == null) continue;
                if (j.IsOutput)
                {
                    return true;
                }
            }
            return false;
        }
    }

    //public NodeJoint ConnectionTo 
    [SerializeField] public NodeJoint[] joints;

    //private NodeJoint _topJoint, _bottomJoint, _leftJoint, _rightJoint;

    //Joint Properties
    public NodeJoint TopJoint
    {
        get
        {
            joints[0].info = NodeJoint.NodeInfo.Top;
            joints[0].bounds = new Rect(window.x + (window.width / 2f) - (JointSize.x / 2f), window.y - JointSize.y, JointSize.x, JointSize.y);
            return joints[0];
            //return new Rect(Window.x + (Window.width / 2f) - (JointSize.x / 2f), Window.y - JointSize.y - (JointSize.y), JointSize.x, JointSize.y);
        }
    }

    public NodeJoint BottomJoint
    {
        get
        {
            joints[1].info = NodeJoint.NodeInfo.Bottom;
            joints[1].bounds = new Rect(window.x + (window.width / 2f) - (JointSize.x / 2f), (window.y + window.height), JointSize.x, JointSize.y);
            return joints[1];
        }
    }

    public NodeJoint LeftJoint
    {
        get
        {
            joints[2].info = NodeJoint.NodeInfo.Left;
            joints[2].bounds = new Rect(window.x - JointSize.x, window.y + ((window.height / 2f) - (JointSize.y / 2f)), JointSize.x, JointSize.y);
            return joints[2];
            //return new Rect(Window.x + (Window.width / 2f) - (JointSize.x / 2f), Window.y - JointSize.y - (JointSize.y), JointSize.x, JointSize.y);
        }
    }

    public NodeJoint RightJoint
    {
        get
        {
            joints[3].info = NodeJoint.NodeInfo.Right;
            joints[3].bounds = new Rect(window.x + window.width, window.y + ((window.height / 2f) - (JointSize.y / 2f)), JointSize.x, JointSize.y);
            return joints[3];
        }
    }

    /*
    public void AdaptArrayToLength() {
        if (this.choices == null) return;
        Choice[] choices = new Choice[choiceCount];
        for (int i = 0; i < this.choices.Length; i++) {
            if (i < choices.Length)
            {
                choices[i] = this.choices[i];
                choices[i].joint = new NodeJoint() {
                    ownerID = Index,
                    bounds = Rect.zero
                };
            }
        }
        this.choices = choices;
    }
    */

    public void ResetSize() {
        window = new Rect(window.position, startWindow.size);
    }

    private void AssignJoints()
    {
        if (joints == null || joints.Length <= 0)
        {
            connectionOrder = new List<NodeJoint>();
            joints = new NodeJoint[4];

            for (int i = 0; i < joints.Length; i++)
            {
                //NodeJoint joint = CreateInstance<NodeJoint>();
                NodeJoint joint = new NodeJoint()
                {
                    ownerID = Index,
                    bounds = Rect.zero,
                    ownerNode = this
                };
                joints[i] = joint;
            }
        }
        //if (joints[0] != null) return;
    }

    public void InitializeChoices() {
        if (choices == null || choices[0] != null) return;
        List<NodeJoint> js = new List<NodeJoint>(joints);
        for (int i = 0; i < choices.Length; i++) {
            choices[i] = new Choice();
            if (choices[i].joint == null) {
                choices[i].joint = new NodeJoint()
                {
                    ownerID = Index,
                    bounds = Rect.zero,
                    info = NodeJoint.NodeInfo.Bottom,
                    ownerNode = this,
                    isChoice = true
                };
                if (!js.Contains(choices[i].joint)) {
                    js.Add(choices[i].joint);
                }
            }
        }
        joints = js.ToArray();
    }

    public NodeJoint[] CalculateLostChoices()
    {
        List<NodeJoint> lost = new List<NodeJoint>();
        for (int i = 0; i < choices.Length; i++) {
            if (i >= choiceCount) {
                choices[i].active = false;
                lost.Add(choices[i].joint);
            }
        }
        Debug.LogWarning("To delete: " + lost.Count);
        return lost.ToArray();
    }

    public bool ContainsJoint(NodeJoint joint) {
        List<NodeJoint> js = new List<NodeJoint>(joints);
        return js.IndexOf(joint) != -1;
    }

    public Node()
    {
        AssignJoints();
    }

    public static Node CreatePageNode()
    {
        //Node p = CreateInstance<Node>();
        Node p = new Node()
        {
            isStartNode = false,
            isEndNode = false,
            type = NodeType.Page
        };
        return p;
    }

    public static Node CreateChoiceNode() {
        Node c = new Node()
        {
            isStartNode = false,
            isEndNode = false,
            type = NodeType.Choice,
        };
        return c;
    }

    public static Node CreateStartNode()
    {
        //Node s = CreateInstance<Node>();
        Node s = new Node()
        {
            isStartNode = true,
            isEndNode = false,
            type = NodeType.State
        };
        return s;
    }

    public static Node CreateEndNode()
    {
        //Node e = CreateInstance<Node>();
        Node e = new Node()
        {
            isEndNode = true,
            isStartNode = false,
            type = NodeType.State
        };
        return e;
    }

    public void Remove()
    {
        foreach (NodeJoint j in joints)
        {
            if (j.connectionPoint != null)
            {
                NodeJoint jnt = null;
                if (j.connectionPoint.ownerNode != null)
                {
                    jnt = j.connectionPoint.ownerNode.joints[j.connectionPoint.JointIndex];
                }
                else {
                    jnt = FindJoint(j.connectionPoint.OwnerID, j.connectionPoint.JointIndex);
                }
                if (jnt != null)
                {
                    jnt.choiceOutputSet = false;
                    jnt.connectionPoint = null;
                }
            }
        }
        connectionOrder.Clear();
    }

    public Node FindNodeByIndex(int index)
    {
        if (nodes == null || index < 0) return null;

        return nodes != null && index < nodes.Count ? nodes[index] : null;
    }

    public NodeJoint FindJoint(int ownerIndex, int jointIndex)
    {
        if (nodes == null) return null;
        Node n = FindNodeByIndex(ownerIndex);
        if (n == null) return null;
        return jointIndex < n.joints.Length ? n.joints[jointIndex] : null;
    }
}

public enum NodeType
{
    Page,
    Choice,
    State
}
