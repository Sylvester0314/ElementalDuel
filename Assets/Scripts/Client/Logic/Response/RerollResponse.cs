using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Server.GameLogic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class RerollResponse : BaseResponse, IEquatable<RerollResponse>
    {
        // greater than 0 => Load scene and start reroll phase with times
        // -1 => Return reroll result
        // -2 => Complete phase and wait opponent
        // -3 => Both complete
        public int RerollTimes;
        public List<DiceLogic> Dices;
        
        public RerollResponse()
        {
            Dices = DiceLogic.EmptyList;
        }

        public RerollResponse(ulong id, List<DiceLogic> dices = null, int times = -1) : base(id)
        {
            RerollTimes = times;
            Dices = dices ?? DiceLogic.EmptyList;
        }

        public override async void Process()
        {
            var reroll = Global.reroll;
            var completion = new TaskCompletionSource<bool>();
            
            if (RerollTimes == -3)
            {
                reroll.WaitingLayout();
                Action unloaded = () => DOVirtual.DelayedCall(0.1f, () =>
                {
                    completion.SetResult(true);
                    Global.indicator.oppoCountdown.diceCount.Count = 8;
                    Global.combatAction.SetStatus(true);
                });
                DOVirtual.DelayedCall(1, () => reroll.Exit(unloaded));

                await completion.Task;
            }
            else if (IsRequester)
            {
                if (RerollTimes == -1)
                    await reroll.RerollAnimation(Dices);
                else if (RerollTimes == -2)
                    reroll.WaitingLayout();
                else
                    Global.OpenRerollScene(Dices, RerollTimes);
            }
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref RerollTimes);
            NetCodeMisc.SerializeList(serializer, ref Dices);
        }

        public bool Equals(RerollResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return RerollTimes == other.RerollTimes &&
                   Dices.Equals(other.Dices);
        }
    }
}