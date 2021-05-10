using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

public class VisualNovelPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image advanceArrow;
    public Image characterField;
    public VisualNovel visualNovel;
    public TextMeshProUGUI[] textFields;
    public RawImage background;
    public Image panel;
    public bool waitForInputAtLastPage = true;

    public NovelEvent onNovelStart;
    public NovelEvent onPageEnd;
    public NovelEvent onNovelEnd;
    public ChoiceEvent onChoice;

    private int _currentPageIndex = 0;
    private int _textCharacterIndex = 0;
    public Node CurrentPage { get; set; }
    private bool _canAdvance;
    private float _timer = 0;
    private bool _canSkip = true;

    private bool _skipAllCutscenes;

    private StringBuilder _textBuilder = new StringBuilder();

    private char[] _textCharacters;

    public bool NovelEnded { get; set; }

    private StringBuilder _choiceBuilder;

    private Vector2 _characterStartPosition;

    private void Start()
    {
        _characterStartPosition = characterField.transform.localPosition;
        //_textFields = GetComponentsInChildren<TextMeshProUGUI>();
        //_background = GetComponentInChildren<RawImage>();

        LoadVisualNovel(visualNovel);
    }

    private void Update()
    {
        if (!visualNovel || !visualNovel.valid || !visualNovel.hasData || NovelEnded) return;

        if (Application.isEditor && Input.GetKeyUp(KeyCode.O) && CutsceneManager.playingCutscene)
        {
            _skipAllCutscenes = !_skipAllCutscenes;
        }

        if (_timer >= 0.01f / CurrentPage.speed && !_canAdvance)
        {
            _timer = 0;

            if (_textCharacterIndex < _textCharacters.Length)
            {
                _textBuilder.Append(_textCharacters[_textCharacterIndex]);
                _textCharacterIndex++;
            }
            else
            {
                if (onPageEnd != null)
                {
                    onPageEnd.Invoke(this);
                }
                _timer = 0;

                if (!waitForInputAtLastPage)
                {
                    if (NextPageIsLast())
                    {
                        EndNovel();
                        return;
                    }
                }

                if (CurrentPage.type != NodeType.Choice)
                {
                    _canAdvance = true;
                }
                else {
                    DisplayChoices();
                }
            }
            UpdateText(1, _textBuilder.ToString());
        }
        else {
            if (!_canAdvance) {
                _timer += Time.unscaledDeltaTime;
            }
        }
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0) || _skipAllCutscenes)
        {
            if (_canAdvance)
            {
                SetPage(_currentPageIndex + 1);
                _canAdvance = false;
            }
            else
            {
                if(_canSkip)
                    Skip();
            }
            _canSkip = true;
        }

        if (advanceArrow == null) return;
        if (!_canAdvance && advanceArrow.gameObject.activeInHierarchy)
        {
            advanceArrow.gameObject.SetActive(false);
        }
        else if (_canAdvance && !advanceArrow.gameObject.activeInHierarchy) {
            advanceArrow.gameObject.SetActive(true);
        }
    }

    private bool NextPageIsLast() {
        if (_currentPageIndex + 1 < visualNovel.nodePath.Length)
        {
            return visualNovel.nodes[visualNovel.nodePath[_currentPageIndex + 1]].isEndNode;
        }
        return true;
    }

    private void DisplayChoices()
    {
        if (ChoicesShown()) return;
        _choiceBuilder = new StringBuilder();
        string space = "        ";
        foreach (Choice choice in CurrentPage.choices)
        {
            if (choice.active)
                _choiceBuilder.Append(space + "[<link>" + choice.choiceText + "</link>]");
        }
        _textBuilder.Append("\n" + _choiceBuilder.ToString());
        UpdateText(1, _textBuilder.ToString());
    }

    private bool ChoicesShown()
    {
        return _choiceBuilder != null;
    }

    private void Skip()
    {
        if (ChoicesShown()) return;
        _textBuilder.Clear();
        _textBuilder.Append(_textCharacters);
        _textCharacterIndex = _textCharacters.Length;
        _timer = 0;
        UpdateText(1, _textBuilder.ToString());

        if (!waitForInputAtLastPage)
        {
            if (NextPageIsLast())
            {
                EndNovel();
                return;
            }
        }
        if (CurrentPage.type != NodeType.Choice)
            _canAdvance = true;
    }

    private void UpdateText(int index, string text)
    {
        textFields[index].text = text;
    }

    private void InitializeText(string text)
    {
        _textCharacterIndex = 0;
        _textBuilder.Clear();
        _textCharacters = text.ToCharArray();
    }

    private void SetPage(int index)
    {
        if (index < visualNovel.nodePath.Length)
        {
            Node n = visualNovel.nodes[visualNovel.nodePath[index]];
            if (n.isEndNode) {
                EndNovel();
                return;
            }
            _currentPageIndex = index;
            LoadPageData();
        }
        else {
            EndNovel();
        }
    }

    private void EndNovel() {
        NovelEnded = true;
        _skipAllCutscenes = false;
        if (onNovelEnd != null)
        {
            onNovelEnd.Invoke(this);
        }
    }

    private void LoadPageData() {
        CurrentPage = visualNovel.nodes[visualNovel.nodePath[_currentPageIndex]];
        if (CurrentPage.character == "")
        {
            textFields[1].fontStyle = FontStyles.Italic;
        }
        else
        {
            textFields[1].fontStyle = FontStyles.Normal;
        }

        background.texture = CurrentPage.background;
        if (!CurrentPage.background)
        {
            background.color = new Color32(0, 0, 0, 0);
        }
        else {
            background.color = Color.white;
        }
        if (background.texture == null)
        {
            background.color = Color.black;
        }
        else
        {
            background.color = Color.white;
        }
        if (advanceArrow)
        {
            //advanceArrow.color = InvertColor(_background.color);
        }
        panel.color = CurrentPage.panelColor;
        if (characterField)
        {
            if (CurrentPage.characterSprite)
            {
                characterField.color = Color.white;
                //characterField.SetNativeSize();
                characterField.sprite = CurrentPage.characterSprite;
                characterField.transform.localScale = CurrentPage.characterSize;
                characterField.transform.localPosition = _characterStartPosition + CurrentPage.characterOffset;
            }
            else {
                characterField.color = new Color32(0, 0, 0, 0);
            }
        }

        UpdateText(0, CurrentPage.character == "" ? " " : CurrentPage.character);
        InitializeText(CurrentPage.text);
    }

    public void LoadVisualNovel(VisualNovel novel)
    {
        ResetNovel();
        visualNovel = novel;
        if (!novel || !novel.valid || !novel.hasData)
        {
            textFields[0].text = "You";
            textFields[1].text = "It seems like there is no visual novel instance assigned to this panel or the specified instance has no data...";
        }
        else
        {
            SetPage(1);
            if (onNovelStart != null)
            {
                onNovelStart.Invoke(this);
            }
        }
    }

    public void ResetNovel()
    {
        _canSkip = true;
        _currentPageIndex = 0;
        _textCharacterIndex = 0;
        CurrentPage = null;
        _canAdvance = false;
        _textBuilder = new StringBuilder();
        _textCharacters = null;
        _choiceBuilder = null;
        NovelEnded = false;
        _timer = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData) {

    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!ChoicesShown()) return;
        int wordIndex = TMP_TextUtilities.FindIntersectingLink(textFields[1], Input.mousePosition, null);
        if (wordIndex != -1)
        {
            string selection = textFields[1].textInfo.linkInfo[wordIndex].GetLinkText();
            foreach (Choice choice in CurrentPage.choices) {
                if (choice.choiceText.Equals(selection)) {
                    SelectChoice(choice);
                    break;
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) {

    }

    private void SelectChoice(Choice choice) {
        if (choice == null) return;
        if (onChoice != null) {
            onChoice.Invoke(choice);
        }
        int nextPage = choice.joint.connectionPoint.OwnerID;
        int pathIndex = IndexOfNodeInPath(nextPage);
        if (pathIndex != -1) {
            _canSkip = false;
            SetPage(pathIndex);
            _choiceBuilder = null;
            _canAdvance = false;
        }
        //_choiceBuilder = null;
        //_currentPageIndex = nextPage;

        //LoadPageData();
        //_canAdvance = false;
    }

    private int IndexOfNodeInPath(int nodeIndex) {
        for (int i = 0; i < visualNovel.nodePath.Length; i++) {
            if (nodeIndex == visualNovel.nodePath[i]) {
                return i;
            }
        }
        return -1;
    }
}

[System.Serializable]
public class NovelEvent : UnityEvent<VisualNovelPanel> {
    //Stub
}

[System.Serializable]
public class ChoiceEvent : UnityEvent<Choice> {
    //Stub
}
