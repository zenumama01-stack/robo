    /// Class ScriptCommandHelpProvider implement the help provider for Functions/ExternalScripts.
    /// This class does the same thing as CommandHelpProvider except for decision making: whether
    /// a particular command is Function/Script or not.
    internal class ScriptCommandHelpProvider : CommandHelpProvider
        internal ScriptCommandHelpProvider(HelpSystem helpSystem)
                    HelpCategory.Configuration |
                    HelpCategory.ScriptCommand;
        internal override CommandSearcher GetCommandSearcherForExactMatch(string commandName, ExecutionContext context)
                CommandTypes.Filter | CommandTypes.Function | CommandTypes.ExternalScript | CommandTypes.Configuration,
        internal override CommandSearcher GetCommandSearcherForSearch(string pattern, ExecutionContext context)
                        SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns,
