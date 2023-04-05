using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace SpeakUp
{
    using static DialogManager;
    //Isolate pawn variables for future use and then reset
    [HarmonyPatch(typeof(PlayLogEntry_Interaction), nameof(PlayLogEntry_Interaction.ToGameStringFromPOV_Worker))]
    public static class PlayLogEntry_Interaction_ToGameStringFromPOV_Worker
    {

        private static void Prefix(PlayLogEntry_Interaction __instance, Pawn ___initiator, Pawn ___recipient)
        {
            lastInteractionDef = __instance.intDef;
            Initiator = ___initiator;
            Recipient = ___recipient;
        }

        private static void Postfix()
        {
            Initiator = (Recipient = null);
            RuleEntry_ValidateConstantConstraints.validationFeedback = false;
            if (SpeakUpSettings.toggleTalkBack) talkBack = false;
        }
    }
}