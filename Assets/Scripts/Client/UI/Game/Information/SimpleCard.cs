using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleCard : AbstractInformationComponent
{
    public Image cardFace;
    public TextMeshProUGUI healthValue;
    public GameObject energyList;
    public GameObject health;
    public Energy energyPrefab;

    private void SwitchCardType(bool isCharacter)
    {
        energyList.SetActive(isCharacter);
        health.SetActive(isCharacter);
    }
    
    public override void SetInformation<T>(T data)
    {
        if (data is PlayableActionCard action)
        {
            cardFace.sprite = action.asset.cardImage;
            SwitchCardType(false);            
        }
        
        if (data is StatusCard status)
        {
            cardFace.sprite = status.asset.relatedCard.cardImage;
            SwitchCardType(false);            
        }

        if (data is CharacterCard character)
        {
            SwitchCardType(true);    
        
            var asset = character.asset;
            cardFace.sprite = asset.cardImage;
            healthValue.text = character.currentHealth.ToString();

            StaticMisc.DestroyAllChildren(energyList.transform);
            var energyCount = asset.baseMaxEnergy;
            var activeEnergy = character.currentEnergy;
            for (var i = 0; i < energyCount; i++)
            {
                var instance = Instantiate(energyPrefab, energyList.transform);
                if (i < activeEnergy)
                    instance.SetActiveDisplay(true);
            }
        }
    }
}
