using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{

    private Image _image;
    public GameObject endText;

    private void Start()
    {
        _image = GetComponent<Image>();
        //_image.canvasRenderer.SetAlpha(0);
    }

    public void FadeIn() {
        Color c = _image.color;
        c.a = 1f;
        _image.color = c;
        endText.SetActive(true);
    }
}
