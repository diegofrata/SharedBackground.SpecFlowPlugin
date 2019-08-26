using System.Text.RegularExpressions;

namespace SharedBackground.SpecFlowPlugin.Grammars
{
    public class ScenarioGrammar : IGrammar
    {
        static readonly Regex ScenarioRegex = new Regex("the scenario '(?<Scenario>.+?)' (?:(?:of|in) '(?<Feature>.+?)' )?(?:has been|is) executed");
        
        public bool IsMatch(string text) => ScenarioRegex.IsMatch(text);
        public (string Feature, string Scenario)? Match(string text)
        {
            var match = ScenarioRegex.Match(text);

            if (!match.Success)
                return default;

            return (match.Groups["Feature"].Value, match.Groups["Scenario"].Value);
        }
    }
}