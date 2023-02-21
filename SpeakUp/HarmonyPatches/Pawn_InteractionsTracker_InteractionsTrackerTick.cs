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
            if (___pawn.def.race.intelligence == Intelligence.Humanlike && ___pawn.interactions != null)
            {
                Statement statement = null;
                var length = DialogManager.Scheduled.Count;
                for (int i = 0; i < length; i++)
                {
                    var def = DialogManager.Scheduled[i];
                    if (def.Timing <= Current.gameInt.tickManager.ticksGameInt && def.Emitter == ___pawn)
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