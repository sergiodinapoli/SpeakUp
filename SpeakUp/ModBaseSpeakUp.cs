using HugsLib;
using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpeakUp
{
    public class ModBaseSpeakUp : ModBase
    {
        public static SettingHandle<int>
            LinesPerConversation,
            IntervalBetweenLines;

        public static SettingHandle<bool>
            SameRegionRestriction,
            ForceNoTranslate,
            ShowGrammarDebug,
            ToggleTalkBack;

        public ModBaseSpeakUp()
        {
            Settings.EntryName = "Speak Up";
        }

        public override string ModIdentifier
        {
            get
            {
                return "SpeakUp";
            }
        }

        public override void DefsLoaded()
        {
            UpdateSettings();

            /*
            //The following is for childhoods and adulthoods. They are story in Unity asset files so we cannot view the XML. Some childhoods/adulthoods have the same generic name but a unique description and identifier. This portion of code creates a text file with ALL childhood and adulthood descriptions, names, and unique identifiers
            string export = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(export, "backstories.txt"), true))
            {
                foreach (KeyValuePair<string, Backstory> story in BackstoryDatabase.allBackstories)
                {
                    String backstory;
                    if (story.Value.slot == BackstorySlot.Childhood)
                    {
                        backstory = "Childhood: ";
                    }
                    else
                    {
                        backstory = "Adulthood: ";
                    }

                    backstory += story.Value.identifier + " = ";
                    backstory += story.Value.baseDesc;
                    outputFile.WriteLine(backstory);
                    outputFile.WriteLine();
                }
            }*/
        }

        public void UpdateSettings()
        {
            LinesPerConversation = Settings.GetHandle<int>("LinesPerConversation", "LinesPerConversation", null, 3);
            IntervalBetweenLines = Settings.GetHandle<int>("TickBetweenLines", "TicksBetweenLines (60 = 1 sec)", null, 60);
            SameRegionRestriction = Settings.GetHandle<bool>("SameRegionRestriction", "SameRegionRestriction", null, true);
            ShowGrammarDebug = Settings.GetHandle<bool>("ShowGrammarDebug", "ShowGrammardebug", null, false);
            ForceNoTranslate = Settings.GetHandle<bool>("ForceNoTranslate", "ForceNoTranslate", "Force rimworld to not translate dialogues between pawns", false);
            ToggleTalkBack = Settings.GetHandle<bool>("ToggleTalkBack", "ToggleTalkBack", "Only turn this on if opening social tab causes lag, can break some mod compatibility.", false);
        }
    }
}