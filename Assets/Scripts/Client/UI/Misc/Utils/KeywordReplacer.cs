using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Shared.Enums;
using UnityEngine;
using UnityEngine.Localization.Components;

public class KeywordReplacer : MonoBehaviour
{
    public bool addHyperlink = false;

    public TextMeshProUGUI text;
    public LocalizeStringEvent stringEvent;

    public HashSet<string> UniqueKeywords;
    public HashSet<string> UsingCards;

    private const string Pattern = @"\$\[(.*?)\]";

    public void Awake()
    {
        stringEvent.OnUpdateString.AddListener(OnUpdateString);
        stringEvent.RefreshString();
    }

    private static string GetLocalizedValue(string key)
    {
        return key[0] switch
        {
            'K' => ResourceLoader.GetLocalizedValue("Keyword", key + "_name"),
            'C' => ResourceLoader.GetLocalizedCard("card_name_" + key[1..]),
            _ => string.Empty
        };
    }

    private static string AddUnderline(string text)
    {
        const string pattern = @"(<color=#[0-9A-F]{6}>|<sprite=\d+>|^)([^<]+)(</color>|$)";
        const string replacement = "$1<u>$2</u>$3";
        return Regex.Replace(text, pattern, replacement);
    }

    private void ClearHashSet()
    {
        if (UniqueKeywords == null)
        {
            UniqueKeywords = new HashSet<string>();
            UsingCards = new HashSet<string>();
        }

        UniqueKeywords.Clear();
        UsingCards.Clear();
    }

    private string AddHyperlink(string content, string keyword)
    {
        return addHyperlink
            ? $"<link=\"{keyword}\">{content}</link>"
            : content;
    }

    private string ProcessString(string content, bool isStart = true)
    {
        if (isStart)
            ClearHashSet();

        var matches = Regex.Matches(content, Pattern);

        var keywords = new List<string>();
        foreach (Match match in matches)
            if (match.Groups.Count > 1)
                keywords.Add(match.Groups[1].Value);

        foreach (var keyword in keywords)
        {
            var key = $"$[{keyword}]";
            var value = GetLocalizedValue(keyword);

            if (keyword[0] == 'C')
            {
                value = $"<color=#FFFFFF>{value}</color>";
                if (addHyperlink)
                    value = AddUnderline(value);
                UsingCards.Add(keyword);
            }
            else if (!UniqueKeywords.Contains(keyword))
            {
                value = AddUnderline(value);
                UniqueKeywords.Add(keyword);
            }

            var index = content.IndexOf(key, StringComparison.Ordinal);
            if (index != -1)
                content = content[..index] +
                          AddHyperlink(value, keyword) +
                          content[(index + key.Length)..];
        }

        return content;
    }

    public string ProcessStringWithSkillData(string content, int damage, Element element)
    {
        ClearHashSet();

        if (damage != -1)
            content = content.Replace("$[Damage]", damage.ToString());

        if (element != Element.None)
        {
            var key = $"K10{(int)element}";
            var value = GetLocalizedValue(key);

            value = AddHyperlink(AddUnderline(value), key);
            content = content.Replace("$[Element]", value);
            UniqueKeywords.Add(key);
        }

        content = ProcessString(content, false);
        return content;
    }

    public void SetText(string content)
    {
        text.text = content;
    }

    private void OnUpdateString(string content)
    {
        content = ProcessString(content);
        SetText(content);
    }
}