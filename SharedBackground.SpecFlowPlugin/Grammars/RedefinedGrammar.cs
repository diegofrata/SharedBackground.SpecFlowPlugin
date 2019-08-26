using System.Text.RegularExpressions;

namespace SharedBackground.SpecFlowPlugin.Grammars
{
    public class RedefinedGrammar : IGrammar
    {
        static readonly Regex RedefinedRegex = new Regex("the scenario '(?<Scenario>.+?)'(?: (?:of|in) '(?<Feature>.+?)')? is redefined");

        public (string Feature, string Scenario)? Match(string text)
        {
            var match = RedefinedRegex.Match(text);

            if (!match.Success)
                return default;

            return (match.Groups["Feature"].Value, match.Groups["Scenario"].Value);
        }
    }
}