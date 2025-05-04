using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.UI.Misc.Transition;
using Shared.Classes;
using Shared.Misc;
using UnityEngine;

public class PlayerInformationDisplay : MonoBehaviour
{
    [Header("Components")] 
    public NameBar nameBar;
    public List<SimpleCharacter> characters;
    public ConfirmButton buildButton;
    public ConfirmButton startButton;

    public void Initialize(
        PlayerData playerData, Action startAction = null,
        string entry = null,   Action buildAction = null
    )
    {
        nameBar.Initialize(playerData);

        if (startAction == null)
            return;

        buildButton.Callback = buildAction;
        startButton.Callback = startAction;
        startButton.textEvent.SetEntry(entry);
    }

    public void SetReadyStyle(bool isOwner, bool status)
    {
        var statusStr = status ? "prepared" : "preparing";

        var statusEntry = $"namebar_status_{statusStr}";
        nameBar.preparedIcon.SetActive(status);
        nameBar.statusEvent.SetEntry(statusEntry);

        var operationEntry = $"{statusStr}_for_game";
        if (!isOwner)
            startButton.textEvent.SetEntry(operationEntry);
    }

    public DeckData SetCharacters(List<DeckData> decks)
    {
        var active = decks.Where(deck => deck.isUsing).First();
        
        for (var i = 0; i < 3; i++)
        {
            var asset = active.characters[i];
            characters[i].SetCardFace(asset);
        }

        return active;
    }

    public void SetTransitionDisplay(GamePlayerInformation information)
    {
        nameBar.playerName.text = information.Name;
        nameBar.avatar.sprite = information.Avatar;

        for (var i = 0; i < 3; i++)
            characters[i].SetCardFace(information.Assets[i]);
    }
}