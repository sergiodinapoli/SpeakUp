using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace SpeakUp
{
    using static DialogManager;
    //Isolate pawn variables for future use and then reset
    [HarmonyPatch(typeof(PlayLogEntry_Interaction), nameof(PlayLogEntry_Interaction.ToGameStringFromPOV_Worker))]
    public static class PlayLogEntry_Interaction_ToGameStringFromPOV_Worker
    {
        private static FieldInfo intDefInfo = AccessTools.Field(typeof(PlayLogEntry_Interaction), "intDef");

        private static void Prefix(PlayLogEntry_Interaction __instance, Pawn ___initiator, Pawn ___recipient)
        {
            lastInteractionDef = (InteractionDef)intDefInfo.GetValue(__instance);
            Initiator = ___initiator;
            Recipient = ___recipient;

            //Maybe change to non spawned for talk of dead pawns
            if (Gossipee == null) FindGossipee();
        }

        private static void Postfix()
        {
            Initiator = (Recipient = ( Gossipee = null));
            RuleEntry_ValidateConstantConstraints.validationFeedback = false;
            if (SpeakUpSettings.toggleTalkBack) talkBack = false;
        }
    }
}