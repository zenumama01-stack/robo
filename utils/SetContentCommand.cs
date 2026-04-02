    /// A command to set the content of an item at a specified path.
    [Cmdlet(VerbsCommon.Set, "Content", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097142")]
    public class SetContentCommand : WriteContentCommandBase
        /// Called by the base class before the streams are open for the path.
        /// This override clears the content from the item.
        /// The path to the items that will be opened for writing content.
        internal override void BeforeOpenStreams(string[] paths)
            if (paths == null || paths.Length == 0)
                throw PSTraceSource.NewArgumentNullException(nameof(paths));
            CmdletProviderContext context = new(GetCurrentContext());
                    InvokeProvider.Content.Clear(path, context);
                    context.ThrowFirstErrorOrDoNothing(true);
                    // If the provider doesn't support clear, that is fine. Continue
                    // on with the setting of the content.
                    // If the item is not found then there is nothing to clear so ignore this exception.
        /// The path to the item on which the content will be set.
            string action = NavigationResources.SetContentAction;
            string target = StringUtil.Format(NavigationResources.SetContentTarget, path);
