using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace SpeakUp
{
    using static DialogManager;
    //Enables reply mechanism when a pawn starts an interaction
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
    public static class Pawn_InteractionsTracker_TryInteractWith
    {
        public static void Prefix(Pawn recipient, ref InteractionDef intDef)
        {
            if (SpeakUpSettings.toggleTalkBack) talkBack = true;
            if (Prefs.LogVerbose) Log.Message($"[SpeakUp] {recipient} is interacting with {intDef.defName}");
        }
    }
}