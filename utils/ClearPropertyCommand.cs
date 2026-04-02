    /// A command to clear the value of a property of an item at a specified path.
    [Cmdlet(VerbsCommon.Clear, "ItemProperty", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096903")]
    public class ClearItemPropertyCommand : PassThroughItemPropertyCommandBase
        #region Parameters
        /// Gets or sets the path parameter to the command.
        [Parameter(Position = 0, ParameterSetName = "Path",
                   Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
                return paths;
                paths = value;
        /// Gets or sets the literal path parameter to the command.
        [Parameter(ParameterSetName = "LiteralPath",
                   Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Alias("PSPath", "LP")]
        public string[] LiteralPath
                base.SuppressWildcardExpansion = true;
        /// The properties to clear from the item.
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
                return _property;
                _property = value;
            Collection<string> propertyCollection = new();
            propertyCollection.Add(_property);
                // Go ahead and let any exception terminate the pipeline.
                return InvokeProvider.Property.ClearPropertyDynamicParameters(
                    Path[0],
                    propertyCollection,
                    context);
                ".",
        #endregion Parameters
        #region parameter data
        /// The properties to be cleared.
        private string _property;
        #endregion parameter data
        /// Clears the properties of an item at the specified path.
            CmdletProviderContext currentContext = CmdletProviderContext;
            currentContext.PassThru = PassThru;
                    InvokeProvider.Property.Clear(
                        currentContext);
