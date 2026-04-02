    /// A command that appends the specified content to the item at the specified path.
    [Cmdlet(VerbsCommon.Add, "Content", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?linkid=2096489")]
    public class AddContentCommand : WriteContentCommandBase
        /// Seeks to the end of the writer stream in each of the writers in the
        /// content holders.
        /// <param name="contentHolders">
        /// The content holders that contain the writers to be moved.
        /// <exception cref="ProviderInvocationException">
        /// If calling Seek on the content writer throws an exception.
        internal override void SeekContentPosition(List<ContentHolder> contentHolders)
            foreach (ContentHolder holder in contentHolders)
                if (holder.Writer != null)
                        holder.Writer.Seek(0, System.IO.SeekOrigin.End);
                    catch (Exception e) // Catch-all OK, 3rd party callout
                        ProviderInvocationException providerException =
                            new(
                                "ProviderSeekError",
                                SessionStateStrings.ProviderSeekError,
                                holder.PathInfo.Provider,
                                holder.PathInfo.Path,
                                e);
                        // Log a provider health event
                        MshLog.LogProviderHealthEvent(
                            this.Context,
                            holder.PathInfo.Provider.Name,
                            providerException,
                            Severity.Warning);
                        throw providerException;
        /// Makes the call to ShouldProcess with appropriate action and target strings.
        /// <param name="path">
        /// The path to the item on which the content will be added.
        /// True if the action should continue or false otherwise.
        internal override bool CallShouldProcess(string path)
            string action = NavigationResources.AddContentAction;
            string target = StringUtil.Format(NavigationResources.AddContentTarget, path);
            return ShouldProcess(target, action);
        #endregion protected members
