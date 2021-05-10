using UnityEngine;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

[System.Serializable]
public class NodeJoint
{
    [SerializeField] public Rect bounds;
    [SerializeField] public NodeInfo info;
    public bool isChoice;
    public bool choiceOutputSet;
    public bool disabled;

    [SerializeField] private bool _input;
    [SerializeField]
    public bool IsInput
    {
        get
        {
            if (_input) _input = connectionPoint != null && connectionPoint.IsValid();
            return _input;
        }

        set
        {
            _input = value;
        }
    }

    [SerializeField] private bool _output;
    [SerializeField]
    public bool IsOutput
    {
        get
        {
            if (_output) _output = connectionPoint != null && connectionPoint.IsValid();
            return _output;
        }

        set
        {
            _output = value;
        }
    }


    [System.NonSerialized] public Node ownerNode;
    [SerializeField] public NodeConnectionData connectionPoint;
    [SerializeField] public int ownerID;

    private void OnEnable()
    {
        //hideFlags = HideFlags.HideAndDontSave;
    }

    public bool CheckForHover(Vector2 mousePosition)
    {
        bool hover = bounds.Contains(mousePosition);
        return hover || IsConnected();
    }

    public bool IsConnected()
    {
        return connectionPoint != null && connectionPoint.IsValid();
    }

    public enum NodeInfo
    {
        Top,
        Bottom,
        Left,
        Right
    }
}

[System.Serializable]
public class NodeConnectionData {
    [System.NonSerialized] public Node ownerNode;
    [SerializeField] private int _ownerId = -1;
    [SerializeField] private int _jointIndex = -1;
    public bool isStartNode;
    public bool isEndNode;

    public int OwnerID {
        get {
            return isStartNode ? 0 : (isEndNode ? Node.LastNodeIndex : _ownerId);
        }

        set {
            _ownerId = value;
        }
    }
    public int JointIndex {
        get {
            return _jointIndex;
        }

        set {
            _jointIndex = value;
        }
    }

    public NodeConnectionData() {
        //Stub
    }

    public NodeConnectionData(int ownerId, int jointIndex, bool isStartNode, bool isEndNode) {
        _ownerId = ownerId;
        _jointIndex = jointIndex;
        this.isStartNode = isStartNode;
        this.isEndNode = isEndNode;
    }

    public bool IsValid() {
        return _ownerId > -1 && _jointIndex > -1;
    }

    public override string ToString()
    {
        return "Owner Index: " + OwnerID + " Joint Index: " + JointIndex + " IsStart: " + isStartNode + " IsEnd: " + isEndNode;
    }
} 