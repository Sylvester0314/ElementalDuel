using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class WaitingText : MonoBehaviour
{
    public TextMeshProUGUI contentText;
    public LocalizeStringEvent content;
    public TextMeshProUGUI ellipsis;

    private int _number;
    private Coroutine _coroutine;

    public static List<string> EllipsisList = new() { "", ".", "..", "..." };

    private IEnumerator SetEllipsis()
    {
        while (true)
        {
            ellipsis.text = EllipsisList[_number];
            _number = (_number + 1) % EllipsisList.Count;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Active(string entry)
    {
        content.SetEntry(entry);
        StartEllipsis();
    }

    public void Display(string text, bool showEllipsis)
    {
        contentText.text = text;
        
        if (!showEllipsis)
            ellipsis.gameObject.SetActive(false);
        else
            StartEllipsis();
    }

    public void Inactive()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private void StartEllipsis()
    {
        _number = 0;
        _coroutine = StartCoroutine(SetEllipsis());
        ellipsis.gameObject.SetActive(true);
    }
}