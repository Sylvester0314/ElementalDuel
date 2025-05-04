using Server.GameLogic;

namespace Shared.Handler
{
    public interface ICostHandler : IEventGenerator
    {
        public CostLogic Cost { get; }

        public void CalculateActualCost();

        public bool EvaluateUsable();
    }
}