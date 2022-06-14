using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Grammar;

namespace SpeakUp
{
    //Expands the rule constraint check to go beyond the constants
    [HarmonyPatch(typeof(GrammarResolver.RuleEntry), "ValidateConstantConstraints")]
    public class RuleEntry_ValidateConstantConstraints
    {
        public static bool validationFeedback = false;

        //NOTE: Tynan called the parameter for this method "constraints", but actually it means "constants".
        //The real constraints are at rule.constantConstraints. Very confusing!

        private static bool Prefix(GrammarResolver.RuleEntry __instance, ref bool __result, Dictionary<string, string> constraints, ref bool ___constantConstraintsChecked, ref bool ___constantConstraintsValid)
		{
			var currentRules = GrammarResolver_RandomPossiblyResolvableEntry.CurrentRules;
			var constants = constraints; //see note above
			var actualConstraints = __instance.rule.constantConstraints;
			__result = ValidateRulesConstraints(actualConstraints, currentRules, ref ___constantConstraintsChecked, ref ___constantConstraintsValid);
            if (validationFeedback)
            {
                string result = __result ? "success" : "failed";
                StringBuilder feedback = new StringBuilder();
                feedback.Append($"{result.ToUpper()} validating constraints for {__instance.rule.keyword}:");
                if (actualConstraints != null)
                {
                    feedback.AppendInNewLine($"{actualConstraints.Select(x => $"\"{x.key} {x.type.ToString().ToLower()} {x.value}\"").ToStringSafeEnumerable()}");
                }

                feedback.AppendInNewLine($"The rule text is \"{__instance.rule}\".");
                feedback.AppendInNewLine($"\nChecked against {currentRules.Count()} rules:\n" +
                    $"{(currentRules.EnumerableNullOrEmpty() ? "none" : currentRules.Select(x => $"{x.Key}: {x.Value.ResolveTags()}").ToLineList())}");
                Log.Message(feedback.ToString());
                validationFeedback = false;
            }
            return false;
		}

		private static bool ValidateRulesConstraints(List<Rule.ConstantConstraint> constraints, List<KeyValuePair<string, string>> rules, ref bool ConstraintsChecked, ref bool ConstraintsValid)
		{
			if (!ConstraintsChecked)
			{
				ConstraintsValid = true;
				if (constraints != null)
				{
					for (int i = 0; i < constraints.Count; i++)
					{
						Rule.ConstantConstraint constraint = constraints[i];
						bool match = false;
						foreach (var entry in rules.Where(x => x.Key == constraint.key))
						{
							string text = entry.Value ?? "";
							float value = 0f;
							float expected = 0f;
							bool ruleIsvalid = !text.NullOrEmpty() && !constraint.value.NullOrEmpty() && float.TryParse(text, out value) && float.TryParse(constraint.value, out expected);
							switch (constraint.type)
							{
								case Rule.ConstantConstraint.Type.Equal:
									match = text.EqualsIgnoreCase(constraint.value);
									break;
								case Rule.ConstantConstraint.Type.NotEqual:
									match = !text.EqualsIgnoreCase(constraint.value);
									break;
								case Rule.ConstantConstraint.Type.Less:
									match = (ruleIsvalid && value < expected);
									break;
								case Rule.ConstantConstraint.Type.Greater:
									match = (ruleIsvalid && value > expected);
									break;
								case Rule.ConstantConstraint.Type.LessOrEqual:
									match = (ruleIsvalid && value <= expected);
									break;
								case Rule.ConstantConstraint.Type.GreaterOrEqual:
									match = (ruleIsvalid && value >= expected);
									break;
								default:
									Log.Error("Unknown ConstantConstraint type: " + constraint.type, false);
									match = false;
									break;
							}
							if (match) break;
						}
						if (!match)
						{
							ConstraintsValid = false;
							break;
						}
					}
				}
				ConstraintsChecked = true;
			}
			return ConstraintsValid;
		}
	}
}