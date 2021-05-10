using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

public class CutsceneManager : MonoBehaviour
{
    public VisualNovelPanel panel;
    public VisualNovel CurrentCutscene { get; set; }

    private List<int> _cutsceneQueue;

    public VisualNovel[] cutscenes;

    public GameObject transitionScreen;
    private Animator _transitionAnim;

    public static bool introPlayed;
    public static bool playingCutscene;

    private int _cutsceneIndex;

    private int _lastCutscene;

    private void Start()
    {
        _transitionAnim = transitionScreen.GetComponent<Animator>();
        transitionScreen.SetActive(false);
        _cutsceneQueue = new List<int>();
        if (panel && CurrentCutscene)
        {
            panel.LoadVisualNovel(CurrentCutscene);
        }

        if (cutscenes.Length == 0 || introPlayed) return;
        introPlayed = true;
        PlayCutscenes(0, 1);
    }

    public void OnCutsceneEnd(VisualNovelPanel panel) {
        Debug.Log("Cutscene ended.");
    }

    public void OnNovelEnd(VisualNovelPanel panel) {
        panel.ResetNovel();
        panel.gameObject.SetActive(false);
    }

    private IEnumerator DelayFadeOut() {
        yield return new WaitForSeconds(2f);
        _transitionAnim.SetBool("FadeOut", true);
        _transitionAnim.SetBool("FadeIn", false);
    }

    public void LoadCutscene() {
        if (_transitionAnim)
        {
            _transitionAnim.SetBool("FadeOut", false);
            _transitionAnim.SetBool("FadeIn", false);
        }
        transitionScreen.SetActive(false);
        if (panel && CurrentCutscene)
        {
            panel.LoadVisualNovel(CurrentCutscene);
        }
        panel.transform.parent.gameObject.SetActive(true);
        playingCutscene = true;
        //SceneManager.LoadScene(1);
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void LoadLevel() {
        if (_cutsceneQueue.Count != 0) {
            //Debug.LogWarning("Cutscene Queue used.");
            int nextCutscene = _cutsceneQueue[0];
            _cutsceneQueue.RemoveAt(0);
            PlayCutscene(nextCutscene);
            _lastCutscene = nextCutscene;
            return;
        }

        transitionScreen.SetActive(true);
        _transitionAnim.SetBool("FadeOut", true);
        panel.transform.parent.gameObject.SetActive(false);
        playingCutscene = false;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void OnChoice(Choice c) {
        if (c.choiceText.ToLower() == "exit")
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    public void NextCutscene()
    {
        playingCutscene = true;
        if (_cutsceneIndex + 1 < cutscenes.Length)
        {
           _cutsceneIndex++;
           CurrentCutscene = cutscenes[_cutsceneIndex];
        }
    }

    public void JumpToCutscene(int cutsceneIndex)
    {
        if (cutsceneIndex < cutscenes.Length)
        {
            _cutsceneIndex = cutsceneIndex;
            CurrentCutscene = cutscenes[cutsceneIndex];
        }
        else
        {
            Debug.LogError("Cutscene of index " + cutsceneIndex + " is not valid!");
        }
    }

    public void GoToCutscene(int index)
    {
        if (index < cutscenes.Length)
        {
            _cutsceneIndex = index;
            CurrentCutscene = cutscenes[_cutsceneIndex];
        }
        else
        {
            Debug.LogError("Cutscene Manager: Cutscene with index " + index + " does not exist within the Cutscene Manager!");
        }
    }

    public void PlayCutscene(int cutsceneIndex)
    {
        JumpToCutscene(cutsceneIndex);
        LoadCutscene();
    }

    public void PlayCutscenes(params int[] cutsceneIndices) {
        if (cutsceneIndices.Length == 0) return;
        int firstCutscene = cutsceneIndices[0];
        for (int i = 1; i < cutsceneIndices.Length; i++) {
            int cutscene = cutsceneIndices[i];
            _cutsceneQueue.Add(cutscene);
            Debug.Log("Added " + cutscene);
        }

        PlayCutscene(firstCutscene);
    }
}
