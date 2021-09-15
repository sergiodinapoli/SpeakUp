using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Grammar;

namespace SpeakUp
{
	public static class ExtraGrammarUtility
	{
        const string
            initPrefix = "INITIATOR_",
            reciPrefix = "RECIPIENT_";

        private static Func<SkillRecord, SkillRecord, SkillRecord> AccessHighestPassion = (A, B) =>
        {
            return (A.passion >= B.passion) ? A : B;
        };

        private static Func<SkillRecord, SkillRecord, SkillRecord> AccessHighestSkill = (A, B) =>
        {
            int a = A.levelInt;
            int b = B.levelInt;
            if (a == b) return (A.passion >= B.passion) ? A : B;
            return (a > b) ? A : B;
        };

        private static Func<SkillRecord, SkillRecord, SkillRecord> AccessWorstSkill = (A, B) =>
        {
            int a = A.levelInt;
            int b = B.levelInt;
            if (a == b) return (A.passion <= B.passion) ? A : B;
            return (a < b) ? A : B;
        };

        private static float lookRadius = 5f;
        private static ThingRequestGroup[] subjects = { ThingRequestGroup.Art, ThingRequestGroup.Plant };
        private static List<Rule_String> tempRules = new List<Rule_String>();
        private static List<string> reversibleRelations = new List<string>()
                { "Bond", "Sibling", "Spouse", "Lover", "Fiance", "HalfSibling", "ExSpouse", "ExLover", "Cousin", "CousinOnceRemoved", "SecondCousin", "Kin"
                };
        public enum dayPeriod { morning, afternoon, evening, night }

        public static IEnumerable<Rule> ExtraRules()
        {
            tempRules.Clear();
            Pawn initiator = DialogManager.Initiator;
            if (initiator == null || !initiator.IsValid()) return null;
            Pawn recipient = DialogManager.Recipient;
            try
            {
                ExtraRulesForPawn(initPrefix, initiator, recipient);
                if (recipient.IsValid()) ExtraRulesForPawn(reciPrefix, recipient, initiator);
                ExtraRulesForTime(initiator);
                ExtraRulesForMap(initiator);
            }
            catch (Exception e)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append($"[SpeakUp] Error processing extra rules: {e.Message}");
                msg.AppendInNewLine($"Initator: {initiator}, ");
                msg.Append(recipient.IsValid() ? $"recipient: {recipient}." : "invalid recipient.");
                msg.AppendInNewLine(tempRules.Count() > 0 ? $"Last successful rule: {tempRules.Last()}" : "Zero rules processed.");
                Log.Warning(msg.ToString());
                return null;
            }
            return tempRules;
        }

        public static void ExtraRulesForPawn(string symbol, Pawn pawn, Pawn other = null)
        {
            //THE PAWN'S MINDSTATE:

            //mood
            MakeRule(symbol + "mood", pawn.needs.mood.CurLevel.ToString());

            //thoughts
            List<Thought> thoughts = new List<Thought>();
            pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
            List<string> texts = new List<string>();
            foreach (var thought in thoughts)
            {
                MakeRule(symbol + "thoughtDefName", thought.def.defName);
                if (thought.CurStage != null)
                {
                    MakeRule(symbol + "thoughtLabel", thought.CurStage.label);
                    if (!thought.CurStage.description.NullOrEmpty()) texts.Add(thought.CurStage.description);
                }
            }
            MakeRule(symbol + "thoughtText", texts.RandomElement());

            //THE PAWN'S RELATIONS
            if (other != null)
            {
                //opinion
                MakeRule(symbol + "opinion", pawn.relations.OpinionOf(other).ToString());

                //relationships
                bool flag1 = true;

                foreach (PawnRelationDef relation in PawnRelationUtility.GetRelations(pawn, other))
                {
                    if (reversibleRelations.Contains(relation.defName))
                    {
                        MakeRule(symbol + "relationship", relation.defName);
                        flag1 = false;
                    }
                }

                foreach (PawnRelationDef relation in PawnRelationUtility.GetRelations(other, pawn))
                {
                    if (!reversibleRelations.Contains(relation.defName))
                    {
                        MakeRule(symbol + "relationship", relation.defName);
                        flag1 = false;
                    }
                }


                if (flag1)
                {
                    MakeRule(symbol + "relationship", "None");
                }
            }

            //THE PAWN'S BIO:

            //traits
            foreach (var trait in pawn.story.traits.allTraits)
            {
                MakeRule(symbol + "trait", trait.Label);
            }

            //best skill
            MakeRule(symbol + "bestSkill", pawn.skills.skills.Aggregate(AccessHighestSkill).def.skillLabel);

            //worst skill
            MakeRule(symbol + "worstSkill", pawn.skills.skills.Aggregate(AccessWorstSkill).def.skillLabel);

            //higher passion
            MakeRule(symbol + "higherPassion", pawn.skills.skills.Aggregate(AccessHighestPassion).def.skillLabel);

            //all skills
            foreach (var skill in pawn.skills.skills)
            {
                MakeRule(symbol + skill.def.label + "_level", skill.levelInt.ToString());
                MakeRule(symbol + skill.def.label + "_passion", skill.passion.ToString());
            }

            //childhood
            MakeRule(symbol + "childhood", pawn.story.childhood?.identifier);

            //adulthood
            MakeRule(symbol + "adulthood", pawn.story.adulthood?.identifier);

            //OTHER PAWN SITUATIONS

            //moving?
            MakeRule(symbol + "moving", pawn.pather.Moving.ToStringYesNo());

            //current activity
            if (pawn.CurJob != null)
            {
                MakeRule(symbol + "jobDefName", pawn.CurJob.def.defName);
                MakeRule(symbol + "jobText", pawn.CurJob.GetReport(pawn));
            }

            //seated?
            if (pawn.Map != null) MakeRule(symbol + "seated", Seated(pawn).ToStringYesNo());
        }

        private static string DayPeriod(Pawn p)
        {
            int hour = GenLocalDate.HourInteger(p);
            if (hour >= 5 && hour < 12) return dayPeriod.morning.ToString();
            if (hour >= 12 && hour < 18) return dayPeriod.afternoon.ToString();
            if (hour >= 18 && hour < 24) return dayPeriod.evening.ToString();
            else return dayPeriod.night.ToString();
        }

        private static void ExtraRulesForMap(Pawn initiator)
        {
            Map map = initiator.Map;
            if (map == null) return;
            IntVec3 pos = initiator.Position;

            //climate
            MakeRule("WEATHER", map.weatherManager.CurWeatherPerceived.label);

            //temperature
            MakeRule("TEMPERATURE", GenTemperature.GetTemperatureForCell(pos, map).ToString());

            //outdoor?
            MakeRule("OUTDOORS", pos.UsesOutdoorTemperature(map).ToStringYesNo());

            //nearest things
            foreach (var group in subjects)
            {
                var thing = GenClosest.ClosestThing_Global(pos, map.listerThings.ThingsInGroup(group), lookRadius);
                if (thing != null) MakeRule($"NEAREST_{group.ToString().ToLower()}", $"{thing.def.label}");
            }
        }

        private static void ExtraRulesForTime(Pawn initiator)
        {
            MakeRule("HOUR", GenLocalDate.HourInteger(initiator).ToString());
            MakeRule("DAYPERIOD", DayPeriod(initiator));
        }

        private static bool IsValid(this Pawn pawn)
        {
            return
                pawn != null &&
                pawn.RaceProps?.Humanlike == true; //Restricted to humanlike, for now.
        }

        private static void MakeRule(string keyword, string output = null)
        {
            if (output.NullOrEmpty())
            {
                if (Prefs.DevMode) Log.Message($"[SpeakUp] Couldn't process {keyword}. Moving on.");
                return;
            }
            tempRules.Add(new Rule_String(keyword, output));
        }

        private static bool Seated(Pawn p)
        {
            Building edifice = p.Position.GetEdifice(p.Map);
            return edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable;
        }
    }
}
