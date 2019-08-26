using System.Text.RegularExpressions;

namespace SharedBackground.SpecFlowPlugin.Grammars
{
    class BackgroundGrammar : IGrammar
    {
        static readonly Regex BackgroundRegex = new Regex("the background steps (?:of|in) '(?<Feature>.+)' have been executed");
        
        public (string Feature, string Scenario)? Match(string text)
        {
            var match = BackgroundRegex.Match(text);

            if (!match.Success) return default;

            return (match.Groups["Feature"].Value, "");
        }
    }
}