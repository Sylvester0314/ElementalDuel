using System.Collections.Generic;
using Server.GameLogic;
using Shared.Logic.Statuses;

namespace Shared.Handler
{
    public interface IEventStringify
    {
        public string EntityName { get; }
        public string LocalizedName => ResourceLoader.GetLocalizedCard(EntityName);
    }
    
    public interface IEventSource : IEventStringify
    {
        public string UniqueId { get; set; }
        public PlayerLogic Belongs { get; }
        public PlayerLogic Opponent => Belongs.Opponent;
    }

    public interface IEventTarget : IEventSource { }

    public interface IEventGenerator : IEventStringify
    {
        public string Key { get; }
        public PlayerLogic Belongs { get; }
    }

    public interface IEventModifiable
    {
        public List<Status> TriggeredStatuses { get; }
    }
}