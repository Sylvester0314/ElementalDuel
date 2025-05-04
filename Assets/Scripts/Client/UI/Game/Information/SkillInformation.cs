
using UnityEngine.UI;

public class SkillInformation : AbstractInformationComponent
{
    public ScrollRect scrollRect;
    public SkillItem item;
    
    public override void SetInformation<T>(T data)
    {
        if (data is not CharacterSkill skill)
            return;
        
        if (item.CheckSame(skill))
            return;
        
        item.SetInformation((skill, false));
        item.SetDetailInformation();
        item.SetInfoBoxAttribute(scrollRect, this);
        item.SetDetailShowStatus(true);
    }
}