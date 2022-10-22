using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace SpeakUp
{
    //In case we need more elaborate settings
    public class SpeakUpMod : Mod
    {
        SpeakUpSettings settings;

        public SpeakUpMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("jpt.speakup");
            harmony.PatchAll();
            settings = GetSettings<SpeakUpSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();

            listing.Begin(inRect);
            listing.Label("Lines Per Conversation: " + SpeakUpSettings.linesPerConversation.ToString(), -1, "How many lines the pawns will use when talking.");
            SpeakUpSettings.linesPerConversation = (int)Math.Truncate(listing.Slider(SpeakUpSettings.linesPerConversation, 0f, 5f));
            listing.Label("Ticks Between Lines: " + SpeakUpSettings.ticksBetweenLines.ToString(), -1, "How many ticks between two lines");
            SpeakUpSettings.ticksBetweenLines = (int)Math.Truncate(listing.Slider(SpeakUpSettings.ticksBetweenLines, 0f, 120f));

            listing.CheckboxLabeled("Same Region Restriction", ref SpeakUpSettings.sameRegionRestriction, "Restrict pawns from talking when in different rooms.");
            listing.CheckboxLabeled("Force No Translate", ref SpeakUpSettings.forceNoTranslate, "Remove translations from interactions. This allows non english games to see the dialogues, but may cause bugs.");
            listing.CheckboxLabeled("Show Grammar Debug", ref SpeakUpSettings.showGrammarDebug, "Shows grammar traces.");
            listing.CheckboxLabeled("Talk Back", ref SpeakUpSettings.toggleTalkBack, "Toggle pawns talking back to the inital conversation starter. Currently not working.");

            if (listing.ButtonText("Reset"))
            {
                SpeakUpSettings.linesPerConversation = 3;
                SpeakUpSettings.ticksBetweenLines = 60;
                SpeakUpSettings.sameRegionRestriction = true;
                SpeakUpSettings.forceNoTranslate = false;
                SpeakUpSettings.showGrammarDebug = false;
                SpeakUpSettings.toggleTalkBack = false;
            }

            listing.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "SpeakUp";
        }
    }

    public class SpeakUpSettings : ModSettings
    {
        public static int
            linesPerConversation = 3,
            ticksBetweenLines = 60;

        public static bool
            sameRegionRestriction = true,
            forceNoTranslate = false,
            showGrammarDebug = false,
            toggleTalkBack = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref linesPerConversation, "linesPerConversation", 3);
            Scribe_Values.Look(ref ticksBetweenLines, "ticksBetweenLines", 60);
            Scribe_Values.Look(ref sameRegionRestriction, "sameRegionRestriction", true);
            Scribe_Values.Look(ref forceNoTranslate, "forceNoTranslate", false);
            Scribe_Values.Look(ref showGrammarDebug, "showGrammarDebug", false);
            Scribe_Values.Look(ref toggleTalkBack, "toggleTalkBack", false);
            base.ExposeData();
        }
    }
}