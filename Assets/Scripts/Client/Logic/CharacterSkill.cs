using Client.Logic.Request;
using Server.GameLogic;
using Shared.Classes;
using Shared.Enums;

public class CharacterSkill
{
    public string BelongsId;
    public Global Global;
    
    public SkillAsset Asset;
    public CostLogic CostLogic;
    public ResourceMatchedResult Matched;
    
    public SkillType Type => Asset.skillType;
    public string Key => Asset.skillName;

    private CharacterSkill(SkillAsset asset)
    {
        Asset = asset;
        CostLogic = new CostLogic(Asset.costs);
    }
    
    public static CharacterSkill FormCharacter(CharacterCard character, SkillAsset asset)
        => new (asset)
        {
            Global = character.Global,
            BelongsId = character.uniqueId
        };

    public static CharacterSkill FromStatic(Global global, SkillAsset asset)
        => new (asset)
        {
            Global = global,
            BelongsId = string.Empty,
        };

    public void RequestPreview()
    {
        var request = UseSkillRequest.Preview(BelongsId, Key);
        var wrapper = ActionRequestWrapper.Create(request);
        
        Global.manager.RequestServerRpc(wrapper);
    }

    public void RequestUse()
    {
        var selecting = Global.diceFunction.GetSelectingDices();
        var target = Global.previewingMainTarget.uniqueId;
        var request = UseSkillRequest.Use(BelongsId, Key, selecting, target);
        var wrapper = ActionRequestWrapper.Create(request);
        
        Global.manager.RequestServerRpc(wrapper);
    }
}