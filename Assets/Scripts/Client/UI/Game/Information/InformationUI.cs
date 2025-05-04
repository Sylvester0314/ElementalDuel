using System.Collections.Generic;
using System.Linq;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;
using IADictionary = System.Collections.Generic.Dictionary<
    InformationUI.InformationType, 
    AbstractInformationComponent
>;

public class InformationUI : MonoBehaviour
{
    public Global global;

    [Header("Prefab References")] 
    public SimpleCard simpleCardPrefab;
    public CardInformation cardInformationPrefab;
    public GameObject characterStatusInfoPrefab;
    public RulesExplanation rulesExplanationPrefab;
    public SkillInformation skillInformationPrefab;

    [Header("In Game Data")]
    public int prevRules;

    private IADictionary _current;
    private readonly List<InformationType> _skillRemoveTypes = new ()
    {
        InformationType.CardFace,
        InformationType.CardInformation,
        InformationType.DetailedRules
    };
    
    public enum InformationType
    {
        CardFace,
        CardInformation,
        StatusList,
        DetailedRules,
        SkillInformation
    }

    public void Start()
    {
        _current = new IADictionary();
    }

    private void CloseExtraInformationBar(InformationType type, bool immediately = false)
    {
        if (!_current.TryGetValue(type, out var node))
            return;
        
        node.DestroySelf(immediately);
        _current.Remove(type);
    }

    public static int HashCode<T>(T data)
    {
        if (data is not HashSet<string> set)
            return -1;
        return set.Aggregate(0, (current, item) => current ^ item.GetHashCode());
    }

    private void SetInformationBarByType<T>(InformationType type, AbstractInformationComponent prefab, T value)
    {
        if (_current.TryGetValue(type, out var component))
        {
            if (type == InformationType.DetailedRules && HashCode(value) == prevRules)
                CloseExtraInformationBar(type);
            else
                component.SetInformation(value);
        }
        else
        {
            var newNode = Instantiate(prefab);
            newNode.SetParent(this);
            newNode.SetInformation(value);
            newNode.FadeIn();
            _current[type] = newNode;
        }
    }

    public void Display(AbstractCard card)
    {
        if (card is DeckCard or PlayableActionCard)
            CloseExtraInformationBar(InformationType.StatusList);

        CloseExtraInformationBar(InformationType.SkillInformation, true);
        CloseExtraInformationBar(InformationType.DetailedRules);
        
        // If selecting card is not in Switch Card phase, do not display the card face
        if (card is not DeckCard)
            SetInformationBarByType(InformationType.CardFace, simpleCardPrefab, card);
        
        SetInformationBarByType(InformationType.CardInformation, cardInformationPrefab, card);
    }

    public void Display(HashSet<string> rules)
    {
        if (rules.Count == 0)
            return;
        
        SetInformationBarByType(InformationType.DetailedRules, rulesExplanationPrefab, rules);
    }

    public void Display(CharacterSkill skill)
    {
        if (skill.Type == SkillType.SwitchActive)
            return;
        
        CloseAll(_skillRemoveTypes, true);
        SetInformationBarByType(InformationType.SkillInformation, skillInformationPrefab, skill);
    }
    
    public void CloseAll(List<InformationType> targets = null, bool immediately = false)
    {
        if (targets == null)
        {
            _current.Clear();
            StaticMisc.DestroyAllChildren(transform);
            return;
        }
        
        var removed = new List<InformationType>();
        
        foreach (var (type, component) in _current)
        {
            if (!targets.Contains(type))
                continue;

            removed.Add(type);
            component.DestroySelf(immediately);
        }
        
        removed.ForEach(type => _current.Remove(type));
    }
}