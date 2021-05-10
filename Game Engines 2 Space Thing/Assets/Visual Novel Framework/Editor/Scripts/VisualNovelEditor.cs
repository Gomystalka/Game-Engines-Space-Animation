using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>
public class VisualNovelEditor : EditorWindow
{
    private Vector2 _gridScale = Vector2.one;


    public static VisualNovelEditor instance;
    public static bool DrawTangents { get; set; }
    public static bool ShowDebugFaces { get; set; }
    public static bool ShowMousePosition { get; set; }

    public Dictionary<StyledElement, GUIStyle> styles = new Dictionary<StyledElement, GUIStyle>();
    private bool _stylesLoaded;

    public const int CellSpacing = 15, BigCellSpacing = CellSpacing * 10;
    public const float EditorWindowTabHeight = 21f;
    public const float NodeWindowTabHeight = 20f;
    private Vector2 _editArea = new Vector2(10000f, 10000f);
    [SerializeField] private List<Node> _nodes = new List<Node>();
    private static Vector2 _scrollPosition = new Vector2(0f, 0f);
    private static Rect _backgroundRect = Rect.zero, _toolbarRect = Rect.zero;
    private NodeRenderer nodeRenderer;

    private static VisualNovelEditor _editorInstance;
    [SerializeField] private Texture2D _editorBackground;

    private VisualNovel _visualNovel;
    private bool _visualNovelLoaded;

    [SerializeField] private Texture2D _gridTexture;

    private Vector2 _mouse = Vector2.zero;

    private bool _creatingConnection;
    private Node _selectedNode;
    private int _jointIndex;

    private void OnEnable()
    {
        instance = this;
        _stylesLoaded = false;
        _gridTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Visual Novel Framework/Editor/Textures/Grid.png");
        _editorBackground = CreateBackgroundTexture(new Color32(33, 33, 33, 255));
        //_gridTexture.Resize((int)_editArea.x, (int)_editArea.y);

    }

    private void LoadStyles() {
        if (_stylesLoaded) return;
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.normal.textColor = Color.white;
        styles.Add(StyledElement.Label, s);

        s = new GUIStyle(GUI.skin.label);
        s.normal.textColor = Color.white;
        s.fontSize = 21;
        s.alignment = TextAnchor.MiddleCenter;
        styles.Add(StyledElement.StateNode, s);

        s = new GUIStyle(GUI.skin.button);
        //s.normal.background = CreateBackgroundTexture(new Color32(62, 62, 62, 255));
        s.normal.textColor = Color.white;
        styles.Add(StyledElement.Button, s);

        s = new GUIStyle(GUI.skin.textArea);
        s.normal.background = CreateBackgroundTexture(new Color32(81, 84, 90, 255));
        s.normal.textColor = Color.white;
        styles.Add(StyledElement.TextArea, s);
        styles.Add(StyledElement.TextField, s);

        s = new GUIStyle(GUI.skin.window);
        //s.normal.background = CreateBackgroundTexture(new Color32(255, 255, 0, 255));
        s.normal.textColor = Color.white;
        styles.Add(StyledElement.Window, s);

        s = new GUIStyle(EditorStyles.objectField);
        styles.Add(StyledElement.ObjectFieldOriginal, s);
        s.normal.textColor = Color.white;
        s.normal.background = CreateBackgroundTexture(new Color32(72, 72, 72, 255));
        styles.Add(StyledElement.ObjectField, s);

        s = new GUIStyle(EditorStyles.toggle);
        s.normal.background = CreateBackgroundTexture(new Color32(81, 84, 90, 255));
        s.normal.textColor = Color.white;
        styles.Add(StyledElement.CheckBox, s);

        s = new GUIStyle(GUI.skin.box);
        s.normal.background = CreateBackgroundTexture(new Color32(40, 42, 44, 160));
        s.normal.textColor = Color.white;
        styles.Add(StyledElement.Box, s);

        nodeRenderer = new NodeRenderer(styles);
        _stylesLoaded = true;
    }

    //[MenuItem("Help/Me/I/Love/Hentai/Way/Too/Much/Extremely Experimental/Visual Novel Editor/New Visual Novel")]
    public static void Start() {
        //_gridTexture = (Texture2D)EditorGUIUtility.Load("Grid.png");
        _editorInstance = GetWindow<VisualNovelEditor>();
        _editorInstance.CreateStateNodes();
        _editorInstance.titleContent = new GUIContent("Visual Novel Editor");

    }

    public void CreateStateNodes() {
        //StateNode start = CreateInstance<StateNode>();
        Node start = Node.CreateStartNode();
        start.window = new Rect(new Vector2(400f, 10f), new Vector2(200f, 50f));
        _nodes.Add(start);
        Node end = Node.CreateEndNode();
        end.window = new Rect(new Vector2(400f, 600f), new Vector2(200f, 50f));
        _nodes.Add(end);
    }

    public static Node FindNodeByIndex(int index) {
        if (instance == null || index < 0) return null;

        return instance._nodes != null && index < instance._nodes.Count ? instance._nodes[index] : null;
    }

    public static NodeJoint FindJoint(int ownerIndex, int jointIndex) {
        if (instance == null) return null;
        Node n = FindNodeByIndex(ownerIndex);
        if (n == null) return null;
        return jointIndex < n.joints.Length ? n.joints[jointIndex]: null;
    }

    public static int GetLastNodeIndex() {
        if (instance == null) return -1;
        return instance._nodes.Count - 1;
    }

    private void HandleEvents(Event e) {
        if (e.type == EventType.MouseMove) {
            _mouse = e.mousePosition;
            e.Use();
        }

        if (e.type == EventType.MouseUp) {
            switch (e.button) {
                case 0:
                    foreach (Node n in _nodes) {
                        for (int i = 0; i < n.joints.Length; i++) {
                            if (n.joints[i].bounds.Contains(GetMousePositionRelativeToContentRect(e.mousePosition)) && !n.joints[i].disabled) {
                                if (!n.joints[i].IsInput && !n.joints[i].IsOutput || (n.joints[i].isChoice && !n.joints[i].choiceOutputSet) || n.isEndNode)
                                {
                                    if (!_creatingConnection)
                                    {
                                        if (!n.Outputting || (n.joints[i].isChoice && !n.joints[i].choiceOutputSet))
                                        {
                                            _jointIndex = i;
                                            _selectedNode = n;
                                            _creatingConnection = true;
                                        }
                                    }
                                    else
                                    {
                                        if (_selectedNode != n)
                                        {
                                            if (_selectedNode.joints[_jointIndex].isChoice)
                                            {
                                                _selectedNode.joints[_jointIndex].choiceOutputSet = true;

                                            }
                                            else if (n.joints[i].isChoice) {
                                                n.joints[i].choiceOutputSet = true;
                                            }

                                            if (!_selectedNode.joints[_jointIndex].isChoice && !n.joints[i].isChoice)
                                            {
                                                int cIndex = i;
                                                Node[] sortedNodes = SortNodes(_selectedNode, n);


                                                if (sortedNodes[0] == n)
                                                {
                                                    cIndex = _jointIndex;
                                                    _jointIndex = i;
                                                }

                                                sortedNodes[0].joints[_jointIndex].connectionPoint = new NodeConnectionData(sortedNodes[1].Index, cIndex, sortedNodes[1].isStartNode, sortedNodes[1].isEndNode) {
                                                    ownerNode = sortedNodes[1]
                                                }; /*sortedNodes[1].joints[cIndex];*/
                                                sortedNodes[1].joints[cIndex].connectionPoint = new NodeConnectionData(sortedNodes[0].Index, _jointIndex, sortedNodes[0].isStartNode, sortedNodes[0].isEndNode) {
                                                    ownerNode = sortedNodes[0]
                                                };/*sortedNodes[0].joints[_jointIndex];*/
                                                sortedNodes[0].connectionOrder.Add(sortedNodes[0].joints[_jointIndex]);
                                                sortedNodes[1].connectionOrder.Add(sortedNodes[1].joints[cIndex]);
                                                sortedNodes[0].joints[_jointIndex].IsOutput = true;
                                                sortedNodes[1].joints[cIndex].IsInput = true;
                                            }
                                            else {
                                                if (!_selectedNode.joints[_jointIndex].isChoice || !n.joints[i].isChoice) {
                                                    if (_selectedNode.joints[_jointIndex].isChoice) {
                                                        _selectedNode.joints[_jointIndex].connectionPoint = new NodeConnectionData(n.Index, i, n.isStartNode, n.isEndNode) {
                                                            ownerNode = n
                                                        };
                                                        n.joints[i].connectionPoint = new NodeConnectionData(_selectedNode.Index, _jointIndex, _selectedNode.isStartNode, _selectedNode.isEndNode) {
                                                            ownerNode = _selectedNode
                                                        };
                                                        n.joints[i].IsOutput = false;
                                                        n.joints[i].IsInput = true;
                                                        _selectedNode.joints[_jointIndex].IsOutput = true;
                                                        _selectedNode.joints[_jointIndex].IsInput = false;
                                                    }

                                                    if (n.joints[i].isChoice)
                                                    {
                                                        n.joints[i].connectionPoint = new NodeConnectionData(_selectedNode.Index, _jointIndex, _selectedNode.isStartNode, _selectedNode.isEndNode)
                                                        {
                                                            ownerNode = _selectedNode
                                                        };
                                                        _selectedNode.joints[_jointIndex].connectionPoint = new NodeConnectionData(n.Index, i, n.isStartNode, n.isEndNode) {
                                                            ownerNode = n
                                                        };
                                                        _selectedNode.joints[_jointIndex].IsOutput = false;
                                                        _selectedNode.joints[_jointIndex].IsInput = true;
                                                        n.joints[i].IsOutput = true;
                                                        n.joints[i].IsInput = false;
                                                    }
                                                }
                                            }
                                            EndConnection();
                                            //Debug.Log("Connected node: " + _selectedNode.Name + "'s Joint " + _jointIndex + " to " + n.Name + "'s Joint " + i);
                                        }
                                    }
                                }
                                else {
                                    Debug.Log("Is Input: " + n.joints[i].IsInput + " Is Output: " + n.joints[i].IsOutput);
                                }
                                //Debug.LogWarning("Node: " + n.Title + " Joint clicked: " + i);
                            }
                        }
                    }
                    break;
                case 1:
                    if (!_creatingConnection)
                    {
                        Node selectedNode = null;
                        foreach (Node n in _nodes)
                        {
                            if (n == null) continue;
                            for (int i = 0; i < n.joints.Length; i++)
                            {
                                if (n.joints[i].bounds.Contains(GetMousePositionRelativeToContentRect(e.mousePosition)))
                                {
                                    if (n.joints[i].IsConnected())
                                    {
                                        selectedNode = n;
                                        _jointIndex = i;
                                    }
                                    break;
                                }
                            }
                        }

                        MenuSelection item = new MenuSelection()
                        {
                            SelectedNode = selectedNode,
                            MousePosition = GetMousePositionRelativeToContentRect(e.mousePosition)
                        };

                        if (selectedNode != null)
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Unlink"), false, MenuCallback, item.Select(MenuOptions.RemoveConnection));
                            menu.ShowAsContext();
                        }

                        if (selectedNode != null) break;
                        selectedNode = IsNodeClicked(GetMousePositionRelativeToContentRect(e.mousePosition));
                        item.SelectedNode = selectedNode;

                        if (selectedNode == null)
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Add page"), false, MenuCallback, item.Select(MenuOptions.AddPage));
                            menu.AddItem(new GUIContent("Add choice"), false, MenuCallback, item.Select(MenuOptions.AddChoicePage));
                            menu.AddItem(new GUIContent("Unlink all"), false, MenuCallback, item.Select(MenuOptions.RemoveAllConnections));
                            menu.AddItem(new GUIContent("Refresh"), false, MenuCallback, item.Select(MenuOptions.Refresh));
                            menu.ShowAsContext();
                        }
                        else
                        {
                            if (!selectedNode.isStartNode && !selectedNode.isEndNode)
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete page"), false, MenuCallback, item.Select(MenuOptions.RemovePage));
                                menu.ShowAsContext();
                            }
                        }
                    }
                    else {
                        EndConnection();
                    }
                    e.Use();
                    break;
                case 2:
                    e.Use();
                    break;
                default:
                    break;
            }
        }

        if (e.type == EventType.KeyUp) {
            if (e.modifiers == EventModifiers.Control && e.keyCode == KeyCode.T)
            {
                DrawTangents = !DrawTangents;
                e.Use();
            }
            else if (e.modifiers == EventModifiers.Control && e.keyCode == KeyCode.F)
            {
                ShowDebugFaces = !ShowDebugFaces;
                e.Use();
            }
            else if (e.modifiers == EventModifiers.Control && e.keyCode == KeyCode.M) {
                ShowMousePosition = !ShowMousePosition;
                e.Use();
            }

            if (e.keyCode == KeyCode.UpArrow)
            {
                _gridScale += new Vector2(0.1f, 0.1f);
                _gridScale = new Vector2(Mathf.Clamp(_gridScale.x, 0.5f, 5f), Mathf.Clamp(_gridScale.y, 0.5f, 5f));
                e.Use();
            }
            else if (e.keyCode == KeyCode.DownArrow) {
                _gridScale -= new Vector2(0.1f, 0.1f);
                _gridScale = new Vector2(Mathf.Clamp(_gridScale.x, 0.5f, 5f), Mathf.Clamp(_gridScale.y, 0.5f, 5f));
                e.Use();
            }
        }
    }

    private void UpdateChoiceNode(Node node, Vector2 mousePosition) {

    } 

    private Node[] SortNodes(Node n1, Node n2) {
        Node[] nodes = { n1, n2 };
        int n1Index = _nodes.IndexOf(n1);
        int n2Index = _nodes.IndexOf(n2);
        if (n1Index == -1 || n2Index == -1 || n1.Outputting || n2.Outputting || n2.isEndNode) return nodes;
        if (n1Index > n2Index || n1.isEndNode) {
            nodes[0] = n2;
            nodes[1] = n1;
        }

        return nodes;
    }

    private void EndConnection() {
        _selectedNode = null;
        _creatingConnection = false;
        _jointIndex = 0;
    }

    public void MenuCallback(object o) {
        if (o is MenuSelection item) {
            switch (item.Option) {
                case MenuOptions.AddPage:
                    AddNode(item.MousePosition);
                    UpdateIndyChan();
                    break;
                case MenuOptions.AddChoicePage:
                    AddChoiceNode(item.MousePosition);
                    UpdateIndyChan();
                    break;
                case MenuOptions.RemovePage:
                    RemoveNode(item.SelectedNode);
                    UpdateIndyChan();
                    break;
                case MenuOptions.RemoveConnection:
                    ResetNodeJoint(item.SelectedNode.joints[_jointIndex]);
                    break;
                case MenuOptions.RemoveAllConnections:
                    if (EditorUtility.DisplayDialog("Unlink", "This action will irreversibly unlink all nodes!\nContinue?", "Yes", "No")) {
                        foreach (Node n in _nodes) {
                            foreach (NodeJoint j in n.joints) {
                                j.connectionPoint = null;
                            }
                            n.connectionOrder.Clear();
                        }
                    }
                    break;
                case MenuOptions.Refresh:
                    foreach (Node n in _nodes) {
                        n.ResetSize();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void RemoveNode(Node node)
    {
        node.Remove();
        _nodes.Remove(node);
        Repaint();
    }

    public void UpdateIndyChan() {
        foreach (Node n in _nodes)
        {
            n.Index = _nodes.IndexOf(n);
            foreach (NodeJoint j in n.joints)
            {
                if (j != null)
                {
                    if(j.ownerNode != null)
                        j.ownerID = _nodes.IndexOf(j.ownerNode);

                    if (j.connectionPoint != null) {
                        if(j.connectionPoint.ownerNode != null)
                            j.connectionPoint.OwnerID = _nodes.IndexOf(j.connectionPoint.ownerNode);
                    }
                }

            }
        }
    }

    private void AddNode(params Vector2[] positions) {
        Node page = Node.CreatePageNode();
        page.window = new Rect(positions.Length > 0 ? positions[0] : Vector2.zero, Node.Size);
        page.startWindow = page.window;
        _nodes.Insert(_nodes.Count - 1, page);
    }

    private void AddChoiceNode(params Vector2[] position) {
        Node choice = Node.CreateChoiceNode();
        choice.window = new Rect(position.Length > 0 ? position[0] : Vector2.zero, Node.Size);
        choice.startWindow = choice.window;
        choice.InitializeChoices();
        _nodes.Insert(_nodes.Count - 1, choice);
    } 

    private Node IsNodeClicked(Vector2 mousePosition) {
        foreach (Node n in _nodes) {
            if (n.window.Contains(mousePosition)) {
                return n;
            }
        }
        return null;
    }

    private void OnGUI()
    {
        LoadStyles();
        wantsMouseMove = true;
        HandleEvents(Event.current);
        if(_editorBackground != null)
            GUI.DrawTexture(_editArea.ToRect(), _editorBackground, ScaleMode.StretchToFill);
        GUILayout.BeginVertical();
        GUILayout.Label("Visual Novel Asset: ", styles[StyledElement.Label]);
        GUI.backgroundColor = new Color32(72, 72, 72, 255);

        //EditorStyles.objectField.normal = styles[StyledElement.ObjectField].normal;

        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        _visualNovel = (VisualNovel)EditorGUILayout.ObjectField(_visualNovel, typeof(VisualNovel), false);
        if (_visualNovel && !_visualNovelLoaded)
        {
            if (_visualNovel.hasData)
            {
                if (EditorUtility.DisplayDialog("Visual Novel Editor", "The specified visual novel object already contains data, would you like to load it?", "Yes", "No"))
                {
                    //Load current Visual Novel data.
                    LoadVisualNovel(_visualNovel);
                    if (_visualNovel) _visualNovelLoaded = true;
                }
                else
                {
                    _visualNovel = null;
                    _visualNovelLoaded = false;
                }
            }
            else {
                _visualNovelLoaded = true;
            }
        }
        else if (!_visualNovel && _visualNovelLoaded) {
            _visualNovelLoaded = false;
        }

        EditorGUI.BeginDisabledGroup(_visualNovel == null);
        if (GUILayout.Button("Clear", styles[StyledElement.Button]))
        {
            if (EditorUtility.DisplayDialog("Visual Novel Editor", "Clearing will completely remove all data within the specified Visual Novel object.\nAre you sure that you want to proceed?", "Yes", "No"))
            {
                _visualNovel.Clear();
                Repaint();
            }
            //Clear Currently Loaded Visual Novel
        }

        if (GUILayout.Button("Save", styles[StyledElement.Button]))
        {
            //Save Current Visual Novel
            if (_visualNovelLoaded)
            {
                GenerateNodePathForNovel(_visualNovel);
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(_nodes == null || _nodes.Count < 3);
        if (GUILayout.Button("Reset", styles[StyledElement.Button])) {
            if (EditorUtility.DisplayDialog("Visual Novel Editor", "Resetting will remove all nodes and connections!\nContinue?", "Yes", "No")) {
                Reset();
                Repaint();
                return;
            }
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        if (Event.current.type == EventType.Repaint) {
            _toolbarRect = GUILayoutUtility.GetLastRect();
        }

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        GUIStyle imgStyle = new GUIStyle();
        GUILayout.BeginVertical();
        for (int i = 0; i < 5; i++) {
            GUILayout.BeginHorizontal();
            for(int j = 0; j < 5; j++)
            {
                GUILayout.Label(_gridTexture, imgStyle);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        if (Event.current.type == EventType.Repaint)
        {
            _backgroundRect = GUILayoutUtility.GetLastRect();
        }
        DragScroll(Event.current);

        if (_creatingConnection && _selectedNode != null)
        {
            //Handles.color = Color.red;
            Handles.DrawLine(_selectedNode.joints[_jointIndex].bounds.center, GetMousePositionRelativeToContentRect(_mouse));
            Handles.DrawBezier(_selectedNode.joints[_jointIndex].bounds.center, GetMousePositionRelativeToContentRect(_mouse), _selectedNode.joints[_jointIndex].bounds.center, GetMousePositionRelativeToContentRect(_mouse), Color.red, null, 5f);
            //Handles.ArrowHandleCap(12345, GetMousePositionRelativeToContentRect(_mouse), Quaternion.identity, 10f, EventType.Layout);
        }
        //GUIExtensions.BeginScaledGroup(_mouse, _gridScale);
        BeginWindows();
        Color _oldBackground = new Color32(0, 0, 0, 0);
        for (int i = 0; i < _nodes.Count; i++)
        { //This loop can sometimes be in the middle of looping when there are no elements in the array due to the modal dialog Reset creates.
            if (_nodes[i] == null) continue;
            if (_nodes.Count >= i)
            {
                string title = _nodes[i].type == NodeType.Choice ? "Choice Page " + i : "Page " + i;
                _nodes[i].Title = title;
                //_nodes[i].Styles = styles;
                _nodes[i].nodeName = title;
                _nodes[i].Index = i;
                _nodes[i].nodes = _nodes;
                if (_nodes[i].isStartNode || _nodes[i].isEndNode)
                {
                    title = "";
                    _nodes[i].nodeName = _nodes[i].isEndNode ? "Last Page" : "Start Page";
                    if (_oldBackground == new Color32(0, 0, 0, 0))
                        _oldBackground = GUI.backgroundColor;
                    GUI.backgroundColor = _nodes[i].isEndNode ? new Color32(80, 0, 0, 160) : new Color32(0, 80, 0, 150);
                }
                else {
                    //GUI.backgroundColor = new Color32(72, 72, 72, 255);
                    GUI.backgroundColor = _oldBackground;
                }
                GUI.depth = 1;
                _nodes[i].window = GUILayout.Window(i, _nodes[i].window, DrawNode, title, styles[StyledElement.Window]);
                GUI.depth = 0;
            }
        }
        Node.LastNodeIndex = GetLastNodeIndex();
        EndWindows();

        foreach (Node n in _nodes) {
            if (n != null)
            {
                nodeRenderer.DrawExtras(n);
                nodeRenderer.DrawJoints(n, GetMousePositionRelativeToContentRect(_mouse));
                //n.DrawJoints(GetMousePositionRelativeToContentRect(_mouse));
            }
        }

        //ExpandUsableArea(_editArea);
        //GUIExtensions.EndScaledGroup();
        GUILayout.EndScrollView();

        if (ShowMousePosition)
            GUI.Label(new Rect(_mouse + new Vector2(10, 0), new Vector2(100, 16)), "X: " + (int)GetMousePositionRelativeToContentRect(_mouse).x + " Y:" + (int)GetMousePositionRelativeToContentRect(_mouse).y, styles[StyledElement.Label]);

        GUI.Label(new Rect(10, EditorWindowTabHeight + _toolbarRect.height + 5f, 200f, 200f), "Blue - Unlinked Start Joint\nGreen - Linked Start Joint\nRed - Unlinked Joint\nYellow - Linked Joint\nMagenta - Input Only Joint\nOrange - Output Only Joint\nGrey - Unlinked Exit Joint\nCyan - Linked Exit Joint", styles[StyledElement.Label]);
    }

    public void LoadVisualNovel(VisualNovel visualNovel) {
        Reset(false);
        if (visualNovel.nodes != null || !visualNovel.hasData)
        {
            if (visualNovel.hasData)
            {
                if (!_nodes.Contains(null))
                {
                    _nodes = new List<Node>(visualNovel.nodes);
                    Debug.Log("Added " + visualNovel.nodes.Length + " node(s).");
                }
                else {
                    Debug.LogError("Failed to load Visual Novel Asset: " + visualNovel.name + ". There are no nodes present within the node array!");
                }
            }
            _visualNovel = visualNovel;
            _visualNovelLoaded = true;
        }
        else {
            Debug.LogError("Failed to load visual novel! There are no nodes within the internal node list.");
        }
        Repaint();
    }

    public static void ResetNodeJoint(NodeJoint current)
    {
        
        if (current.ownerID == -1) return;
        current.IsInput = false;
        current.IsOutput = false;
        if (current.isChoice) {
            current.choiceOutputSet = false;
        }
        
        if (current.connectionPoint != null)
        {
            //Debug.LogWarning(current.connectionPoint.ToString());
            //connectionPointconnectionPoint = null;
            NodeJoint j = null;
            if (current.connectionPoint.ownerNode != null)
            {
                j = current.connectionPoint.ownerNode.joints[current.connectionPoint.JointIndex];
            }
            else
            {
                j = FindJoint(current.connectionPoint.OwnerID, current.connectionPoint.JointIndex);
            }
            if (j != null)
            {
                if (j.isChoice) {
                    j.choiceOutputSet = false;
                }
                j.connectionPoint = null;
                j.IsInput = false;
                j.IsOutput = false;
            }

            Node n = current.connectionPoint.ownerNode ?? FindNodeByIndex(current.connectionPoint.OwnerID);
            if (n != null && n.connectionOrder != null)
            {
                if (n.connectionOrder.IndexOf(current) != -1)
                    n.connectionOrder.Remove(j);
            }
            current.connectionPoint = null;
        }

        Node o = current.ownerNode ?? FindNodeByIndex(current.ownerID);
        if (o != null && o.connectionOrder != null)
        {
            if (o.connectionOrder.IndexOf(current) != -1)
                o.connectionOrder.Remove(current);
        }
    }

    public void Reset(bool clearObject) {
        _nodes.Clear();
        if (clearObject)
            _visualNovel = null;
        CreateStateNodes();
    }

    public void Reset() {
        Reset(true);
    }

    public void DrawNode(int windowId) {
        nodeRenderer.DrawNode(_nodes[windowId]);
        GUI.DragWindow();
    }

    private void DragScroll(Event e) {
        if (e.type == EventType.MouseDrag && e.button == 2) {
            Vector2 delta = e.delta * -2f;
            _scrollPosition = new Vector2(Mathf.Clamp(_scrollPosition.x + delta.x, 0, _editArea.x), Mathf.Clamp(_scrollPosition.y + delta.y, 0, _editArea.y));

            e.Use();
        }
    }

    private void ExpandUsableArea(Vector2 size) {
        GUILayout.BeginHorizontal();
        GUILayout.Space(size.x);
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Space(size.y);
        GUILayout.EndVertical();
    }

    //Looks weird.
    private void DrawGrid() {
        for (int i = 0; i < position.width + _editArea.x; i += CellSpacing) {
            Handles.color = (i % BigCellSpacing == 0 ? Color.black : (Color)new Color32(60, 60, 60, 180));
            Handles.DrawLine(new Vector2(i, 0), new Vector2(i, position.width + _editArea.x));
        }

        for (int i = 0; i < position.height + _editArea.y; i += CellSpacing) {
            Handles.color = (i % BigCellSpacing == 0 ? Color.black : (Color)new Color32(60, 60, 60, 180));
            Handles.DrawLine(new Vector2(0, i), new Vector2(position.height + _editArea.y, i));
        }
    }

    public static Texture2D CreateBackgroundTexture(Color background) {
        Texture2D t = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        t.SetPixel(0, 0, background);
        t.Apply();
        return t;
    }

    public static Texture2D CreateMonochromeTexture(float width, float height, Color color) {
        Texture2D t = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                t.SetPixel(j, i, color);
            }
        }
        t.Apply();
        return t;
    }

    public static Vector2 GetMousePositionRelativeToContentRect(Vector2 mousePosition) {
        if (_backgroundRect == null || _backgroundRect == Rect.zero) return mousePosition;
        mousePosition += _scrollPosition - new Vector2(0, _toolbarRect.height + EditorWindowTabHeight);
        return mousePosition;
    }

    private void GenerateNodePathForNovel(VisualNovel visualNovel) {
        Node[] nodes = new Node[_nodes.Count];
        for(int i = 0; i < nodes.Length; i++) {
            nodes[i] = _nodes[i];
        }
        visualNovel.nodes = nodes;
        visualNovel.nodeCount = visualNovel.nodes.Length;
        List<int> path = new List<int>();
        visualNovel.valid = true;
        if (visualNovel.nodes.Length > 2)
        {
            if (visualNovel.nodes[0].isStartNode)
            {
                FollowNode(visualNovel.nodes[0], path); //Good old friend called recursion.
                if (path.Count > 0)
                {
                    Node endNode = _nodes[path[path.Count - 1]];
                    Debug.Log("Node Path ended at node: " + _nodes[path[path.Count - 1]].nodeName);

                    if (endNode.isEndNode)
                    {
                        Debug.Log("Node path generated successfully for " + visualNovel.name + ".");
                        visualNovel.nodePath = path.ToArray();
                    }
                    else
                    {
                        Debug.LogError("Failed to generate Node Path for " + visualNovel.name + ". A valid path must end at the exit node! Node Path ended at node: " + _nodes[path[path.Count - 1]].nodeName);
                        visualNovel.valid = false;
                    }
                }
                else
                {
                    Debug.LogError("Failed to create Node Path!");
                    visualNovel.valid = false;
                }
            }
            else
            {
                Debug.LogError("Failed to generate Node Path for " + visualNovel.name + ". A valid path must begin at the start node!");
                visualNovel.valid = false;
            }
        }
        else
        {
            Debug.LogError("Failed to generate Node Path for " + name + ". The specified node list consists of only a start and exit node! This is not allowed.");
            visualNovel.valid = false;
        }
        EditorUtility.SetDirty(visualNovel);

        visualNovel.hasData = true;
        visualNovel.author = System.Environment.UserName;
        Debug.Log("Node Path: " + path.Count);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Visual Novel [" + visualNovel.name + "] saved.");
    }

    private void FollowNode(Node node, List<int> path)
    {
        Node next = null;
        if (!path.Contains(node.Index) || node.type == NodeType.Choice || node.isEndNode)
        {
            path.Add(node.Index);
        }
        else
        {
            Debug.LogError("Failed to create Node Path. A node loop has been detected within [" + node.nodeName + "] node! Node loops using normal pages are not allowed.");
            return;
        }
        foreach (NodeJoint j in node.connectionOrder)
        {
            if (j.IsOutput && j.connectionPoint != null)
            {
                Node owner = j.connectionPoint.ownerNode;
                if (owner != null)
                {
                    next = j.connectionPoint.ownerNode;
                }
                else {
                    next = FindNodeByIndex(j.connectionPoint.OwnerID);
                }
                break;
            }
        }

        if (node.type == NodeType.Choice) {
            foreach (Choice c in node.choices) {
                if (c.active) {
                    Node j = c.joint.connectionPoint.ownerNode ?? FindNodeByIndex(c.joint.connectionPoint.OwnerID);
                    if (j != null)
                    {
                        FollowNode(j, path);
                        if (path[path.Count - 1] != Choice.Break && !_nodes[path[path.Count - 1]].isEndNode)
                        {
                            Debug.LogError("Failed to create Node Path. Every choice must end at the end node! Choice ended at: " + _nodes[path[path.Count - 1]].nodeName);
                            return;
                        }
                        //path.Add(Choice.Break);
                    }
                }
            }
        }

        if (next != null)
        {
            FollowNode(next, path);
        }
    }
}

public static class VectorExtensions {
    public static Vector2 ReduceX(this Vector2 v, float factor)
    {
        v.x -= factor;
        return v;
    }

    public static Vector2 ReduceY(this Vector2 v, float factor) {
        v.y -= factor;
        return v;
    }

    public static Rect ToRect(this Vector2 v) {
        return new Rect(0, 0, v.x, v.y);
    }

    public static Vector2 Rotate(this Vector2 vv, Vector2 v, Vector2 pivot, float radians)
    {
        //GUI.Label(new Rect(v, Vector2.one * 200f), radians * Mathf.Rad2Deg + " DEG", Styles[StyledElement.Label]);
        float x = Mathf.Cos(radians) * (v.x - pivot.x) - Mathf.Sin(radians) * (v.y - pivot.y) + pivot.x;
        float y = Mathf.Sin(radians) * (v.x - pivot.x) + Mathf.Cos(radians) * (v.y - pivot.y) + pivot.y;
        return new Vector2(x, y);
        //return new Vector2(v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians), v.y * Mathf.Cos(radians) + v.x * Mathf.Sin(radians));
    }
}

public class EditorGUILayoutExtensions {
    public static Color TRANSPARENT = new Color(0f, 0f, 0f, 0f);
    private static Color _oldBackground;

    public static Object ObjectFieldPlus<T>(Object obj, System.Type objType, bool allowSceneObjects, GUIStyle style, int fieldID, params GUILayoutOption[] options) where T : Object
    {
        GUILayout.BeginHorizontal();
        GUIContent iconContent = EditorGUIUtility.IconContent(objType.Name + " Icon");
        EditorGUILayout.LabelField(new GUIContent(obj == null ? "None (" + objType.Name + ")" : obj.name, obj != null && obj is Texture ? (Texture)obj : (iconContent.image ?? EditorGUIUtility.IconContent("DefaultAsset Icon").image)), style, options);

        Rect labelRect = GUILayoutUtility.GetLastRect();
        GUILayout.EndHorizontal();
        //Rect buttonRect = new Rect(labelRect.x + labelRect.width - 16f, labelRect.y + 1f, 18f, 16f);

        Event e = Event.current;

        if (e.type == EventType.ExecuteCommand)
        {
            if (e.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == fieldID)
                {
                    obj = EditorGUIUtility.GetObjectPickerObject();
                }
            }
            else if (e.commandName == "ObjectSelectorClosed") {
                return EditorGUIUtility.GetObjectPickerObject() ?? obj;
            }
            e.Use();
        }

        if (e.type == EventType.MouseDown)
        {
            if (e.button == 0)
            {
                if (labelRect.Contains(e.mousePosition))
                {
                    EditorGUIUtility.ShowObjectPicker<T>(obj, allowSceneObjects, "", fieldID);
                    e.Use();
                }
            }
        }
        return obj;
    }

    public static void BeginBackgroundColorGroup(Color c) {
        if (_oldBackground != TRANSPARENT) return;
        _oldBackground = GUI.backgroundColor;
        GUI.backgroundColor = c;
    }

    public static void EndBackgroundColorGroup() {
        if(_oldBackground != TRANSPARENT)
            GUI.backgroundColor = _oldBackground;

        _oldBackground = TRANSPARENT;
    }

    public static GUIStyle ChangeTextColor(GUIStyle style, Color color)
    {
        GUIStyle s = new GUIStyle(style);
        s.normal.textColor = color;
        s.focused.textColor = color;
        s.active.textColor = color;
        //s.hover.textColor = color;
        return s;
    }

    public static GUIStyle ChangeFontSettings(GUIStyle style, Color color, int fontSize, TextAnchor alignment) {
        GUIStyle s = new GUIStyle(style);
        s.normal.textColor = color;
        if(fontSize > 0)
            s.fontSize = fontSize;
        s.alignment = alignment;
        return s;
    }
}

public class GUIExtensions {
    private static Matrix4x4 _oldMatrix;

    public static void BeginScaledGroup(Vector2 pivot, Vector2 scale)
    {
        _oldMatrix = GUI.matrix;
        Matrix4x4 translation = Matrix4x4.TRS(pivot, Quaternion.identity, Vector3.one);
        Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(scale.x, scale.y, 1f));
        GUI.matrix = translation * scaledMatrix * translation.inverse * GUI.matrix;
    }

    public static void EndScaledGroup() {
        GUI.matrix = _oldMatrix;
    }
}

public static class ColorExtensions
{
    public readonly static Color Orange = new Color32(255, 69, 0, 255);
} 

public struct MenuSelection
{
    public Node SelectedNode { get; set; }
    public Vector2 MousePosition { get; set; }
    public MenuOptions Option { get; set;}

    public MenuSelection Select(MenuOptions option) {
        Option = option;
        return this;
    }
}

public enum MenuOptions {
    AddPage,
    AddChoicePage,
    RemovePage,
    MakeConnection,
    RemoveConnection,
    RemoveAllConnections,
    Refresh
}

public enum StyledElement {
    Label,
    Button,
    TextArea,
    TextField,
    Window,
    ObjectField,
    CheckBox,
    Box,
    StateNode,
    ObjectFieldOriginal
}