using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.Grammar;

namespace SpeakUp
{
    //Insert custom rules
    [HarmonyPatch(typeof(GrammarResolver), nameof(GrammarResolver.Resolve))]
    class GrammarResolver_Resolve
    {
        private static FieldInfo rulesInfo = AccessTools.Field(typeof(GrammarRequest), "rules");

        public static bool Prefix(ref string __result, object __instance, string rootKeyword, GrammarRequest request, string debugLabel = null, bool forceLog = false, string untranslatedRootKeyword = null, List<string> extraTags = null, List<string> outTags = null, bool capitalizeFirstSentence = true)
        {
            if (rootKeyword != "r_logentry") return true;
            List<Rule> rules = (List<Rule>)rulesInfo.GetValue(request);
            if (rules.NullOrEmpty()) return true;
            var newRules = ExtraGrammarUtility.ExtraRules();
            if (newRules.EnumerableNullOrEmpty()) return true;
            rules.AddRange(newRules);

            //Always use untranslated dialogues so patches folder is not overwritten by rimworld language files
            if (LanguageDatabase.activeLanguage != LanguageDatabase.defaultLanguage)
            {
                __result = GrammarResolver.ResolveUnsafe(untranslatedRootKeyword ?? rootKeyword, request, debugLabel, forceLog, true, extraTags, outTags, capitalizeFirstSentence);
                return false;
            }
            return true;
        }
    }
}
