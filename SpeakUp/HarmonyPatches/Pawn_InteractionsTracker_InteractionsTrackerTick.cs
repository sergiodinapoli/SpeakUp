using HarmonyLib;
using RimWorld;
using Verse;

namespace SpeakUp
{
    //Fires a new line if scheduled.
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.InteractionsTrackerTick))]
    static class Pawn_InteractionsTracker_InteractionsTrackerTick
    {
        static void Postfix(Pawn ___pawn)
        {
            if (___pawn.RaceProps.Humanlike && ___pawn.interactions != null)
            {
                Statement statement = null;
                var scheduled = DialogManager.Scheduled; //Bring onto the stack
                var tick = Current.gameInt.tickManager.ticksGameInt;
                for (int i = DialogManager.ScheduledCount; i-- > 0;)
                {
                    var def = scheduled[i];
                    if (def.Timing <= tick && def.Emitter == ___pawn)
                    {
                        statement = def;
                        break;
                    }
                }
                if (statement != null) DialogManager.FireStatement(statement);
            }
        }
    }
}