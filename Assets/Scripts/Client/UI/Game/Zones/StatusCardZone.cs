using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Handler;
using Shared.Logic.Statuses;
using Shared.Misc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class StatusCardZone : MonoBehaviour, IStatusesContainer
{
    public Global global;
    public GridLayoutGroup layout;
    public List<StatusCard> cards;

    [Header("In Game Data")] 
    public int currentIndex;
    
    [ShowInInspector]
    public string UniqueId { get; private set; }
    
    public void Initialize(string id, Global g)
    {
        UniqueId = id;
        global = g;
        global.StatusContainers.Add(UniqueId, this);

        for (var i = 0; i < cards.Count; i++)
            cards[i].Initialize(this, i);
    }

    public async Task Append(Status status)
    {
        await cards[currentIndex].Occupy(status);
        
        currentIndex += 1;
    }

    public void Resort(int index)
    {
        layout.enabled = false;
        currentIndex -= 1;
        
        var status = cards[index];
        
        status.gameObject.SetActive(false);

        cards.Remove(status);
        cards.Add(status);

        for (var i = 0; i < cards.Count; i++)
        {
            var target = cards.Find(card => card.index == i);
            cards[i].transform.DOMove(target.transform.position, 0.35f);
        }
        
        status.transform.SetSiblingIndex(3);
        layout.enabled = true;
    }

    public async void Preview(List<Status> statuses)
    {
        var currentStatuses = cards.Select(card => card.uniqueId).ToList();
        var (existed, incoming) = statuses
            .SplitBy(status => currentStatuses.Contains(status.UniqueId));

        foreach (var status in existed)
            await cards.Find(card => card.uniqueId == status.UniqueId).Preview(status);
        
        for (var i = 0; i < incoming.Count; i++)
            await cards[i + currentIndex].Preview(incoming[i]);
    }

    public void CancelPreview()
    {
        for (var i = currentIndex; i < cards.Count; i++)
            cards[i].CancelPreviewStatus();
    }
}