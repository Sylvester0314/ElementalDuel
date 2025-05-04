using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Handler;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class StatusZone : MonoBehaviour, IStatusesContainer
{
    public Global global;
    public HorizontalLayoutGroup layout;
    public CharacterCard belongs;
    public List<StatusEntity> entities;

    [Header("In Game Data")]
    public Dictionary<string, Status> Statuses;
    
    [ShowInInspector]
    public string UniqueId { get; private set; }

    public void Initialize(string id, Global g)
    {
        Statuses = new Dictionary<string, Status>();
        UniqueId = id;
        
        global = g;
        global.StatusContainers.Add(UniqueId, this);
        
        for (var i = 0; i < entities.Count; i++)
            entities[i].Initialize(this, i);
    }
    
    public async Task Append(Status status)
    {
        await entities[Statuses.Count].Occupy(status);

        Statuses.Add(status.UniqueId, status);
    }
    
    public void Resort(int index)
    {
        layout.enabled = false;
        
        var status = entities[index];
        
        status.gameObject.SetActive(false);

        entities.Remove(status);
        entities.Add(status);

        for (var i = 0; i < entities.Count; i++)
        {
            var target = entities.Find(card => card.index == i);
            entities[i].transform.DOMove(target.transform.position, 0.35f);
        }
        
        status.transform.SetSiblingIndex(3);
        layout.enabled = true;
    }
    
    public void Preview(List<Status> statuses) { }
    
    public void CancelPreview() { }
}