using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RulesExplanation : AbstractInformationComponent
{
    public RuleItem rulePrefab;
    public RectTransform rulesList;

    private List<RuleItem> _rules;

    public void Awake()
    {
        _rules = new List<RuleItem>();
    }

    private void CreateRules(List<string> keywords, bool isFirst = true)
    {
        foreach (var keyword in keywords)
        {
            var instance = Instantiate(rulePrefab);
            instance.SetInformation((keyword, isFirst));
            instance.SetParent(rulesList.transform);
            _rules.Add(instance);
            isFirst = false;
        }
    }

    private void ReplaceRules(List<string> keywords)
    {
        var rulesCount = _rules.Count;
        var keywordsCount = keywords.Count;

        for (var i = 0; i < rulesCount; i++)
        {
            var rule = _rules[i];
            if (i < keywordsCount)
                rule.SetRuleInfo(keywords[i]);
            else
                Destroy(rule.gameObject);
        }

        if (rulesCount > keywordsCount)
        {
            _rules.RemoveRange(keywordsCount, rulesCount - keywordsCount);
            return;            
        }

        var extraRule = keywords.GetRange(rulesCount, keywordsCount - rulesCount);
        if (rulesCount < keywordsCount)
            CreateRules(extraRule, false);
    }

    public override void SetInformation<T>(T data)
    {
        if (data is not HashSet<string> keywords)
            return;

        ui.prevRules = InformationUI.HashCode(keywords);

        if (_rules.Count == 0)
            CreateRules(keywords.ToList());
        else
            ReplaceRules(keywords.ToList());
        
        ForceRebuildLayoutImmediate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rulesList);
    }
}