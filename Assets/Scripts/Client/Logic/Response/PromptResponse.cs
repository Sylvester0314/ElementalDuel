using System;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public enum PromptType
    {
        Header,
        DoubleHeader,
        Signal,
        Action,
        Banner,
        Dialog,
        FixedBanner,
        Round,
        Close,
        Phase
    }

    public class PromptResponse : BaseResponse, IEquatable<PromptResponse>
    {
        public PromptType Type;
        public bool BoolTag;
        public bool UseEntry;
        public string Content;

        private string ParsedContent => UseEntry ? ResourceLoader.GetLocalizedUIText(Content) : Content;

        public PromptResponse() { }

        public PromptResponse(
            PromptType type, string content, bool tag = false, 
            bool useEntry = false) : base(ulong.MaxValue)
        {
            Type = type;
            Content = content;
            BoolTag = tag;
            UseEntry = useEntry;
        }

        #region Factory Methods

        public static PromptResponse Header(string entry)
            => new (PromptType.Header, entry);
        
        public static PromptResponse Header(string main, string sub)
            => new (PromptType.DoubleHeader, $"{main}###{sub}");

        public static PromptResponse Dialog(string entry)
            => new (PromptType.Dialog, entry);
        
        public static PromptResponse Signal(string str, bool showEllipsis, bool useEntry = true)
            => new (PromptType.Signal, str, showEllipsis, useEntry);

        public static PromptResponse FixedBanner(string str, bool useEntry = true)
            => new (PromptType.FixedBanner, str, useEntry: useEntry);
        
        public static PromptResponse Banner(string str, bool autoHide, bool useEntry = true)
            => new (PromptType.Banner, str, autoHide, useEntry);
        
        public static PromptResponse Action(ulong actingId, bool isEnd, bool isContinue = false)
            => new (PromptType.Action, actingId.ToString(), isEnd, isContinue);
        
        public static PromptResponse Round(int round, bool first)
            => new (PromptType.Round, round.ToString(), first);

        public static PromptResponse Close(bool forced)
            => new (PromptType.Close, string.Empty, forced);

        public static PromptResponse Phase(string phase)
        {
            var entry = $"banner_{phase}_phase";
            return new PromptResponse(PromptType.Phase, entry, useEntry: true);
        }

        #endregion

        public override async void Process()
        {
            var prompt = Global.prompt;
            var content = ParsedContent;
            var completion = new TaskCompletionSource<bool>();
            
            Action cb = () => completion.SetResult(true);
            Action delayPrompt = Type switch
            {
                PromptType.Banner => () => prompt.banner.Animate(content, false, BoolTag, cb),
                PromptType.Action => () => prompt.action.Display(ActionParse(), cb),
                PromptType.Round  => () => RoundDisplay(cb),
                PromptType.Phase  => () => PhaseDisplay(content, cb),
                _                 => null
            };

            if (delayPrompt != null)
            {
                delayPrompt.Invoke();
                await completion.Task;
            }

            Action fixedPrompt = Type switch
            {
                PromptType.Header       => () => prompt.header.Display(content),
                PromptType.DoubleHeader => () => prompt.header.Display(DoubleSplit(Content)),
                PromptType.FixedBanner  => () => prompt.banner.FixedAnimate(content),
                PromptType.Signal       => () => prompt.signal.Display(content, BoolTag),
                PromptType.Dialog       => () => prompt.dialog.Display(Content),
                PromptType.Close        => () => prompt.CloseAll(BoolTag),
                _                       => null
            };
            
            fixedPrompt?.Invoke();
            base.Process();
        }

        private void RoundDisplay(Action callback)
        {
            foreach (var player in Global.players)
                player.nameBar.Fade(false, true);

            Global.Acting = BoolTag;
            Global.indicator.Close(true);
            Global.prompt.rounds.Display((Content, BoolTag), callback);
        }
        
        private void PhaseDisplay(string content, Action callback)
        {
            if (Content.Equals("banner_action_phase"))
            {
                foreach (var player in Global.players)
                    player.nameBar.Fade(true, true);
                
                Global.indicator.Open(true);
                Global.indicator.Display();
                Global.diceFunction.gameObject.SetActive(true);
                Global.combatAction.forced = false;
                Global.combatAction.TransferStatus(CombatTransfer.Active);
            }
            
            if (Content.Equals("banner_end_phase"))
            {
                Global.Self.SetEndPhaseStyle();
                Global.Opponent.SetEndPhaseStyle();
                Global.indicator.Hide();
                Global.diceFunction.gameObject.SetActive(false);
                Global.combatAction.ForcedTransferStatus(CombatTransfer.Transparent);
            }
            
            Global.prompt.banner.FixedAnimate(content, true, callback);
        }
        
        private ValueTuple<string, bool, bool> ActionParse()
        {
            var isSelf = NetworkManager.Singleton.LocalClientId.ToString() == Content;
            var turnContinue = UseEntry;
            if (turnContinue)
            {
                var continueEntry = isSelf ? "your" : "oppo";
                return ($"{continueEntry}_turn_continue", isSelf, false);
            }

            var site = isSelf ? 1 : 0;
            var end = BoolTag ? 2 : 0;
            
            var entry = (site | end) switch
            {
                0 => "banner_oppo_turn",
                1 => "banner_your_turn",
                2 => "declare_end_round",
                3 => "ending_my_round",
                _ => string.Empty
            };
            
            return (entry, isSelf, BoolTag);
        }
        
        private static ValueTuple<string, string> DoubleSplit(string content)
        {
            var split = content.Split("###");
            return (split[0], split[1]);
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);

            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Content);
            serializer.SerializeValue(ref BoolTag);
            serializer.SerializeValue(ref UseEntry);
        }

        public bool Equals(PromptResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Type == other.Type &&
                   Content == other.Content &&
                   BoolTag == other.BoolTag &&
                   UseEntry == other.UseEntry;
        }
    }
}