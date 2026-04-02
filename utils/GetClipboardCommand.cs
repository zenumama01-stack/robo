using Microsoft.PowerShell.Commands.Internal;
    /// Defines the implementation of the 'Get-Clipboard' cmdlet.
    /// This cmdlet get the content from system clipboard.
    [Cmdlet(VerbsCommon.Get, "Clipboard", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2109905")]
    [Alias("gcb")]
    public class GetClipboardCommand : PSCmdlet
        /// Property that sets raw parameter. This will allow clipboard return text or file list as one string.
        public SwitchParameter Raw
                return _raw;
                _raw = value;
        /// Gets or sets the delimiters to use when splitting the clipboard content.
        [ArgumentCompleter(typeof(DelimiterCompleter))]
        public string[] Delimiter { get; set; } = [Environment.NewLine];
        private bool _raw;
        /// This method implements the ProcessRecord method for Get-Clipboard command.
            this.WriteObject(GetClipboardContentAsText(), true);
        /// Returns the clipboard content as text format.
        /// <returns>Array of strings representing content from clipboard.</returns>
        private List<string> GetClipboardContentAsText()
            var result = new List<string>();
            string textContent = null;
                textContent = Clipboard.GetText();
            catch (PlatformNotSupportedException)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(ClipboardResources.UnsupportedPlatform), "FailedToGetClipboardUnsupportedPlatform", ErrorCategory.InvalidOperation, "Clipboard"));
            if (_raw)
                result.Add(textContent);
                result.AddRange(textContent.Split(Delimiter, StringSplitOptions.None));
    /// Provides argument completion for the Delimiter parameter.
    public sealed class DelimiterCompleter : IArgumentCompleter
        /// <param name="commandName">The name of the command that is being completed.</param>
        /// <param name="parameterName">The name of the parameter that is being completed.</param>
        /// <param name="wordToComplete">The input text to filter the results by.</param>
        /// <param name="commandAst">The ast of the command that triggered the completion.</param>
        /// <param name="fakeBoundParameters">The parameters bound to the command.</param>
        /// <returns>Completion results.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
            wordToComplete ??= string.Empty;
            var pattern = new WildcardPattern(wordToComplete + '*', WildcardOptions.IgnoreCase);
            if (pattern.IsMatch("CRLF") || pattern.IsMatch("Windows"))
                yield return new CompletionResult("\"`r`n\"", "CRLF", CompletionResultType.ParameterValue, "Windows (CRLF)");
            if (pattern.IsMatch("LF") || pattern.IsMatch("Unix") || pattern.IsMatch("Linux"))
                yield return new CompletionResult("\"`n\"", "LF", CompletionResultType.ParameterValue, "UNIX (LF)");
