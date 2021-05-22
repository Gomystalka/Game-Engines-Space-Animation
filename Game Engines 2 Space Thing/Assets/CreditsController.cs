using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditsController : MonoBehaviour
{
    public float creditsScrollRate;
    [TextArea] public string[] entries;

    public bool IsScrolling { get; set; }
    public bool Ended { get; private set; }
    public OneTimeEvent onCreditsEnd;

    private TextMeshProUGUI textElement;
    private int _entryIndex;

    private void OnEnable()
    {
        textElement = GetComponent<TextMeshProUGUI>();
        ResetCredits();
        //RollCredits(); 
    }

    private void Update()
    {
        if (!IsScrolling || Ended) return;
        if (_entryIndex + 1 >= entries.Length) {
            Ended = true;
            onCreditsEnd?.InvokeOneTime();
            return;
        }
        float y = textElement.transform.localPosition.y - creditsScrollRate * Time.deltaTime;
        textElement.transform.localPosition = new Vector3(textElement.transform.localPosition.x, y, textElement.transform.localPosition.z);
        //Debug.Log($"Y: {y} | H: {Screen.height}");
        if (y < -Screen.height) {
            textElement.transform.localPosition = new Vector3(textElement.transform.localPosition.x, Screen.height + 10f, textElement.transform.localPosition.z);
            _entryIndex++;
            textElement.text = entries[_entryIndex];
        }
    }

    public void ResetCredits() {
        _entryIndex = 0;
        Ended = false;
        textElement.transform.localPosition = new Vector3(textElement.transform.localPosition.x, Screen.height + 10f, textElement.transform.localPosition.z);
        textElement.text = entries[_entryIndex];
    }

    public void RollCredits() => IsScrolling = true;
}
