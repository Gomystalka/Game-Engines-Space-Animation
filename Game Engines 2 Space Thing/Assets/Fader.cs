using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{

    private Image _image;
    public GameObject endText;
    public float fadeRate = 0.1f;
    public OneTimeEvent onFadeInEnd;

    private void Start()
    {
        _image = GetComponent<Image>();
        //_image.canvasRenderer.SetAlpha(0);
    }

    public void FadeIn() {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine() {
        Color c = _image.color;
        c.a = 0f;
        _image.color = c;
        while (c.a < 1f) {
            c.a += Time.deltaTime * fadeRate;
            _image.color = c;
            yield return null;
        }
        c.a = 1f;
        _image.color = c;
        onFadeInEnd?.InvokeOneTime();
    }
}
