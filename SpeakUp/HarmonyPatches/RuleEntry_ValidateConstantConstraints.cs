using HarmonyLib;
using System;
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

			if (!___constantConstraintsChecked)
			{
				___constantConstraintsValid = ValidateRulesConstraints(actualConstraints, currentRules);
				___constantConstraintsChecked = true;
			}

			__result = ___constantConstraintsValid;

			if (validationFeedback)
			{
				string result = __result ? "SUCCESS" : "FAILED";
				StringBuilder feedback = new StringBuilder();
				feedback.Append($"{result} validating constraints for {__instance.rule.keyword}:");
				if (actualConstraints != null)
				{
					feedback.AppendInNewLine($"{actualConstraints.Select(x => $"\"{x.key} {x.type.ToString().ToLower()} {x.value}\"").ToStringSafeEnumerable()}");
				}
				feedback.AppendInNewLine($"The rule text is \"{__instance.rule}\".");
				feedback.AppendInNewLine($"\nChecked against {currentRules.Count} rules:\n" +
					$"{(currentRules.EnumerableNullOrEmpty() ? "none" : currentRules.Select(x => $"{x.Key}: {x.Value.ResolveTags()}").ToLineList())}");
				Log.Message(feedback.ToString());
				validationFeedback = false;
			}

			return false;
		}

		private static bool ValidateRulesConstraints(List<Rule.ConstantConstraint> constraints, List<KeyValuePair<string, string>> rules)
		{
			return constraints == null || constraints.All(constraint =>
			{
				// Is this still needed?
				if (constraint.key == "deserters")
				{
					return true;
				}

				string key = constraint.key;
				string value = constraint.value;
				IEnumerable<string> texts = rules.Where(p => p.Key == key).Select(p => p.Value);
				bool compareAsString() => texts.Any(text => text.EqualsIgnoreCase(value));
				bool compareAsFloat(Func<float, float, bool> f) => texts.Any(text => float.TryParse(text, out float lhs) && float.TryParse(value, out float rhs) && f(lhs, rhs));

				switch (constraint.type)
				{
					case Rule.ConstantConstraint.Type.Equal:
						return compareAsString();
					case Rule.ConstantConstraint.Type.NotEqual:
						return !compareAsString();
					case Rule.ConstantConstraint.Type.Less:
						return compareAsFloat((lhs, rhs) => lhs < rhs);
					case Rule.ConstantConstraint.Type.Greater:
						return compareAsFloat((lhs, rhs) => lhs > rhs);
					case Rule.ConstantConstraint.Type.LessOrEqual:
						return compareAsFloat((lhs, rhs) => lhs <= rhs);
					case Rule.ConstantConstraint.Type.GreaterOrEqual:
						return compareAsFloat((lhs, rhs) => lhs >= rhs);
					default:
						Log.Error($"Unknown ConstantConstraint type: {constraint.type}");

						return false;
				}
			});
		}
	}
}
