using System.Threading.Tasks;
using Server.GameLogic;
using UnityEngine;

public class Cardholder : MonoBehaviour
{
    public ActionCardAsset asset;
    public int timestamp;
    public int sort;

    public async Task<Cardholder> Initialize(ActionCardInformation information)
    {
        timestamp = information.timestamp;
        asset = await ResourceLoader.LoadSoAsset<ActionCardAsset>(information.name);
        return this;
    }

    public Cardholder SetAttribute(Transform parent, int index)
    {
        sort = index;
        transform.SetParent(parent, false);
        return this;
    }
    
    public Cardholder SetAttribute(Vector3 position, int index)
    {
        sort = index;
        transform.localPosition = position;
        return this;
    }
}