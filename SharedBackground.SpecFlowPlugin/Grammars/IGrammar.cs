namespace SharedBackground.SpecFlowPlugin.Grammars
{
    interface IGrammar
    {
        (string Feature, string Scenario)? Match(string text);
    }
}