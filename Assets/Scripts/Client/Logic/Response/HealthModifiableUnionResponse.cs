using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.GameLogic;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class ModifiedTarget : INetworkSerializable, IEquatable<ModifiedTarget>, IComparable<ModifiedTarget>
    {
        public string Id;
        public bool Main;
        public int Amount;
        public Element Type;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Main);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Amount);
        }
        
        public bool Equals(ModifiedTarget other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Id == other.Id &&
                   Type == other.Type &&
                   Main == other.Main &&
                   Amount == other.Amount;
        }

        public int CompareTo(ModifiedTarget other)
        {
            return other.Type.CompareTo(Type);
        }
    }
    
    public class HealthModifiableUnionResponse : BaseResponse, IEquatable<HealthModifiableUnionResponse>
    {
        public string SourceId;
        public List<ModifiedTarget> Targets;
        
        public HealthModifiableUnionResponse()
        {
            Targets = new List<ModifiedTarget>();
        }

        public HealthModifiableUnionResponse(string source, List<AttributeModifiableEvent> events) 
        {
            SourceId = source;
            Targets = events
                .Select(e =>
                {
                    var modified = new ModifiedTarget
                    {
                        Amount = e.Amount,
                        Type = Element.None,
                        Id = e.Target.UniqueId
                    };

                    if (e is DamageEvent damage)
                    {
                        modified.Amount *= -1;
                        modified.Type = damage.ElementType;
                        modified.Main = damage.IsMainTarget;
                    }

                    return modified;
                })
                .ToList();
        }

        public override async void Process()
        {
            if (Targets.Count == 0)
            {
                base.Process();
                return;
            }

            var mainTarget = Targets.Where(target => target.Main).FirstOrDefault();
            var completion = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(SourceId) || mainTarget == null)
                DisplayFeedbacks(completion);
            else
            {
                var source = Global.GetEntity(SourceId);
                var target = Global.GetCharacter(mainTarget.Id);

                if (target != null)
                {
                    Global.attackAnimating = true;
                    source.DoAction(target, mainTarget.Type, () => DisplayFeedbacks(completion));
                } 
            }

            await completion.Task;
            await Task.Delay(10);
                
            Global.attackAnimating = false;
            
            base.Process();
        }

        private async void DisplayFeedbacks(TaskCompletionSource<bool> completion)
        {
            var groupedTarget = Targets
                .GroupBy(target => target.Id)
                .Select(grouping => grouping
                    .GroupBy(target => target.Type)
                    .Select(subGrouping => new ModifiedTarget
                    {
                        Type = subGrouping.Key,
                        Amount = subGrouping.Sum(modified => modified.Amount),
                        Id = grouping.Key,
                        Main = subGrouping.Any(modified => modified.Main)
                    })
                    .ToList()
                )
                .ToList();

            // Each character has only taken damage once
            if (!groupedTarget.Any(list => list.Count > 1))
            {
                var batch = groupedTarget
                    .SelectMany(targets => targets)
                    .ToList();
                
                HandleBatch(batch, true, () => completion.TrySetResult(true));
                return;
            }
            
            groupedTarget.ForEach(modified => modified.Sort());
            
            var batches = new List<List<ModifiedTarget>>();
            
            while (groupedTarget.Sum(list => list.Count) != 0)
            {
                var batch = groupedTarget
                    .Where(list => list.Count != 0)
                    .Select(list => list.Pop())
                    .ToList();
                
                batches.Add(batch);
            }

            var batchCount = batches.Count;
            for (var i = 0; i < batchCount; i++)
            {
                var handleFinished = new TaskCompletionSource<bool>();
                var isLastBatch = i == batchCount - 1;
                HandleBatch(batches[i], isLastBatch, () => handleFinished.SetResult(true));

                await handleFinished.Task;
                await Task.Delay(10);
            }
            
            completion.TrySetResult(true);
        }

        private void HandleBatch(List<ModifiedTarget> targets, bool isLastBatch, Action onComplete = null)
        {
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var entity = Global.GetCharacter(target.Id);
                if (entity == null)
                    return;
                
                var feedback = entity.feedback;

                feedback.OnStart = () =>
                {
                    entity.ModifyHealth(target.Amount);

                    if (target.Type is Element.Piercing or Element.Physical or Element.None)
                        return;
                    
                    var applicationComponent = entity.applications;
                    var applied = applicationComponent.applications;
                    var incoming = target.Type.ToApplication();

                    var reaction = ReactionLogic.GetReaction(applied, incoming, out var remaining);

                    if (reaction != ElementalReaction.None)
                        applicationComponent.ReactionAnimation(
                            applied, incoming,
                            () => applicationComponent.SetApplications(remaining)
                        );
                    else
                        applicationComponent.SetApplications(remaining);
                };
    
                if (isLastBatch)
                    feedback.OnStartClose = entity.SwitchToDefeatedStatus;
                if (i == targets.Count - 1)
                    feedback.OnComplete = onComplete;
                
                feedback.Display(target.Amount, target.Type);
            }
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref SourceId);
            NetCodeMisc.SerializeList(serializer, ref Targets);
        }

        public bool Equals(HealthModifiableUnionResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return SourceId.Equals(other.SourceId) &&
                   Targets.SequenceEqual(other.Targets);
        }
    }
}