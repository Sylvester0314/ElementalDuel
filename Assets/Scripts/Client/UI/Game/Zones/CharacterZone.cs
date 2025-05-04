using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Enums;

public class CharacterZone : AbstractZone
{
    public Player owner;
    public List<CharacterCard> characters;

    private AutomaticLayout Layout { get; set; }
    private int _activeIndex = -1;

    public CharacterCard Active => _activeIndex == -1 ? null : characters[_activeIndex];
    public Global Global => owner.global;
    
    public async Task SwitchActiveCharacter(string id)
    {
        var completion = new TaskCompletionSource<bool>();
        var prevActive = _activeIndex;
        
        if (_activeIndex != -1)
            characters[_activeIndex].SetActiveStatus(false);

        var to = Global.GetCharacter(id);
        to.SetActiveStatus(true, () => completion.TrySetResult(true));
        _activeIndex = to.index;
        
        if (prevActive != -1)
            Global.combatAction.TransferStatus(CombatTransfer.Active);
        await completion.Task;
    }

    public void Initialize(Player player)
    {
        Layout = GetComponent<AutomaticLayout>();
        characters = new List<CharacterCard>();
        owner = player;
        
        for (var i = 0; i < Layout.count; i++)
        {
            var child = transform.GetChild(i);
            var character= child.GetComponent<CharacterCard>();
            character.index = i;
            characters.Add(character);
        }

        Layout.StaticLayout();
    }
    
    public void ChooseActiveCharacter()
    {
        Global.combatAction.TransferStatus(CombatTransfer.Choose);
        
        var area = Global.GetZone<CharacterZone>(Site.Self);
        var alive = area.characters
            .Where(character => character.currentHealth != 0)
            .ToList();
        
        alive.ForEach(card => card.SwitchToSelectableStatus(false));
        alive.First().SelectCard(Global.combatAction.chooseActiveButton);
    }
}