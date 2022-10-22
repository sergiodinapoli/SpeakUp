using RimWorld;
using System.Linq;
using Verse;

namespace SpeakUp
{
    using static DialogManager;
    public class Talk
    {
        public static int count => SpeakUpSettings.linesPerConversation;
        public static int interval => SpeakUpSettings.ticksBetweenLines;

        public int
            latestReplyCount = 0,
            expireTick = 0;
        public int remainingReplies => SpeakUpSettings.linesPerConversation - latestReplyCount;

        public Pawn nextInitiator, nextRecipient;

        private string tagToContinue = "continue";

        public Talk(string tag)
        {
            nextInitiator = Initiator;
            nextRecipient = Recipient;
            Reply(tag);
        }

        public void MakeReply(InteractionDef intDef, bool swap = true)
        {
            var time = GenTicks.TicksGame + interval;
            if (swap) SwapRoles();
            expireTick = time + 1;
            latestReplyCount += 1;
            if (Scheduled.Any(x => x.Emitter == nextInitiator && x.IntDef == intDef && x.Iteration == latestReplyCount))
            {
                var talks = Scheduled.Select(x => x.Talk).Distinct().Where(y => y.nextInitiator == nextInitiator || y.nextRecipient == nextInitiator).Count();
                Log.Error($"[SpeakUp] {nextInitiator} tried to repeat a reply for {intDef} while talking to {nextRecipient}.\n" +
                    $"{nextInitiator} is participating in {talks} current talks");
                return;
            }
            Scheduled.Add(new Statement(nextInitiator, nextRecipient, time, intDef, this, latestReplyCount));
        }

        public void Reply(string tag)
        {
            if (SpeakUpSettings.sameRegionRestriction && Initiator.GetRegion() != Recipient.GetRegion()) return;
            if (remainingReplies > 0)
            {
                bool continuing = tag == tagToContinue;
                InteractionDef intDef = continuing ? lastInteractionDef : DefDatabase<InteractionDef>.GetNamed(tag, false);
                if (intDef != null) MakeReply(intDef, !continuing);
                else Log.Warning($"[SpeakUp] {nextInitiator} talked about {tag}, but there isn't an appropriate interactionDef to respond.");
            }
        }

        private void SwapRoles()
        {
            Pawn swapped = nextInitiator;
            nextInitiator = nextRecipient;
            nextRecipient = swapped;
        }
    }
}
