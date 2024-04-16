using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SpeakUp
{
    public static class DialogManager
    {
        public static List<Statement> Scheduled = new List<Statement>();
        public static int ScheduledCount; //The overhead on the .Count method is significant. Consider refactoring the add/remove methods into a wrapper to handle this count cache
        public static List<Talk> CurrentTalks = new List<Talk>();
        public static Pawn Initiator, Recipient;
        public static InteractionDef lastInteractionDef;
        public static bool talkBack = false;

        public static void Ensue(List<string> tags)
        {
            if (!talkBack && SpeakUpSettings.toggleTalkBack) return;
            List<string> usedTags = new List<string>();
            Talk ongoing = CurrentTalks.Where(x => x.nextInitiator == Initiator || x.nextRecipient == Initiator).FirstOrDefault();
            var tag = tags.First();
            if (ongoing == null) CurrentTalks.Add(new Talk(tag));
            else ongoing.Reply(tag);
            tags.Remove(tag);
            if (!tags.NullOrEmpty())
            {
                Log.Warning($"[SpeakUp] {Initiator} tried to say multiple replies at once! Supressed tags: {tags.ToStringSafeEnumerable()}.");
            }
        }

        public static void CleanUp()
        {
            CurrentTalks.RemoveAll(x => x.expireTick < Current.gameInt.tickManager.ticksGameInt);
        }

        public static void FireStatement(Statement statement)
        {
            var intDef = statement.IntDef;
            intDef.ignoreTimeSinceLastInteraction = true; //temporary, bc RW limit is 120 ticks
            statement.Emitter.interactions.lastInteractionTime -= 120; //Lower the last interaction time to counter a check
            statement.Emitter.interactions.TryInteractWith(statement.Reciever, statement.IntDef);
            if (Prefs.LogVerbose) Log.Message($"[SpeakUp] {statement.Emitter} continues the conversation with {statement.Reciever}, reply #{statement.Iteration} ({statement.IntDef.label}).");
            Scheduled.Remove(statement);
            ScheduledCount = Scheduled.Count;
        }
    }
}