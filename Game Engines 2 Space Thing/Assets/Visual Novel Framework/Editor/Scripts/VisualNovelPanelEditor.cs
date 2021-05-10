using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Made by Tomasz Galka C18740411
/// </summary>

[CustomEditor(typeof(VisualNovelPanel))]
public class VisualNovelPanelEditor : Editor {
    private GameObject _novelPanelPrefab;

    private void LoadPrefab()
    {
        if (!_novelPanelPrefab)
        {
            _novelPanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Visual Novel Framework/Editor/Prefabs/Novel Panel.prefab");
            if (!_novelPanelPrefab)
            {
                Debug.LogError("Failed to load Visual Novel Panel prefab!");
            }
        }
    }

    [MenuItem("Visual Novel Framework/GameObject/Visual Novel Panel")]
    public static void Create()
    {
        VisualNovelPanelEditor instance = CreateInstance<VisualNovelPanelEditor>();
        instance.LoadPrefab();
        instance.SpawnPrefab();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

    }

    private void SpawnPrefab() {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Canvas c = null;
        if (canvases == null || canvases.Length == 0)
        {
            c = CreateNewCanvas();
        }
        else {
            c = canvases[0];
        }
        GameObject panel = Instantiate(_novelPanelPrefab, Vector3.zero, Quaternion.identity);
        panel.name = "New VN Panel";
        panel.transform.SetParent(c.transform, false);
    }

    private Canvas CreateNewCanvas() {
        GameObject c = new GameObject("Canvas", new System.Type[] { typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster) });
        Canvas canvas = c.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if(FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", new System.Type[] { typeof(EventSystem), typeof(StandaloneInputModule)});
        //Instantiate(c);
        //Instantiate(eventSystem) ;
        return canvas;
    }
}
