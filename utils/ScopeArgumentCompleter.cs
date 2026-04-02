    /// Provides argument completion for Scope parameter.
    public class ScopeArgumentCompleter : IArgumentCompleter
        private static readonly string[] s_Scopes = new string[] { "Global", "Local", "Script" };
        /// Returns completion results for scope parameter.
                    possibleCompletionValues: s_Scopes);
