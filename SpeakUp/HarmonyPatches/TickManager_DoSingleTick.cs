using HarmonyLib;
using Verse;

namespace SpeakUp
{
    using static DialogManager;

    //Cleans up expired talks
    [HarmonyPatch(typeof(TickManager), "DoSingleTick")]
    internal static class TickManager_DoSingleTick
    {
        private static void Postfix()
        {
            if (CurrentTalks.Count > 0 && Current.gameInt.tickManager.ticksGameInt % 60 == 0)
            {
                CleanUp(); 
            }
        }
    }
}