using System;
using System.Threading.Tasks;
using DG.Tweening;

namespace Client.Logic.Request
{
    public class DeclareEndRequest : BaseRequest, IEquatable<DeclareEndRequest>
    {
        public override async Task Process()
        {
            var tm = Game.TurnManager;
            if (!tm.Check(RequesterId, UniqueId))
                return;
            
            // var wrappers = tm.DeclareEndRound(RequesterId);
            
            var wrappers = tm.DeclareEndRound(RequesterId, out var roundEnd);
            
            Response(wrappers);
            // Response(wrappers[1]);
            // Response(wrappers[2]);

            tm.AfterAction(true);
            
            await base.Process();
            
            if (roundEnd)
                DOVirtual.DelayedCall(0.5f, Game.StartNewRound);
        }

        public bool Equals(DeclareEndRequest other)
        {
            return !ReferenceEquals(other, null) && base.Equals(other);
        }

        public override string ToString()
        {
            return $"DeclareEndRequest(Requester: {RequesterId})";
        }
    }
}