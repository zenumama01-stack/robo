    /// A command to get the property of an item at a specified path.
    [Cmdlet(VerbsCommon.Get, "ItemProperty", DefaultParameterSetName = "Path", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096493")]
    public class GetItemPropertyCommand : ItemPropertyCommandBase
        /// The properties to retrieve from the item.
                return InvokeProvider.Property.GetPropertyDynamicParameters(
                    SessionStateUtilities.ConvertArrayToCollection<string>(_property), context);
        /// The properties to be retrieved.
        private string[] _property;
        /// Gets the properties of an item at the specified path.
                    InvokeProvider.Property.Get(
                        SessionStateUtilities.ConvertArrayToCollection<string>(_property),
                        CmdletProviderContext);
    /// A command to get the property value of an item at a specified path.
    [Cmdlet(VerbsCommon.Get, "ItemPropertyValue", DefaultParameterSetName = "Path", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096906")]
    public sealed class GetItemPropertyValueCommand : ItemPropertyCommandBase
        [Parameter(Position = 0, ParameterSetName = "Path", Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = "LiteralPath", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        /// Gets the values of the properties of an item at the specified path.
            if (Path == null || Path.Length == 0)
                paths = new string[] { "." };
                    Collection<PSObject> itemProperties = InvokeProvider.Property.Get(
                        new string[] { path },
                        base.SuppressWildcardExpansion);
                    if (itemProperties != null)
                        foreach (PSObject currentItem in itemProperties)
                            if (this.Name != null)
                                foreach (string currentPropertyName in this.Name)
                                    if (currentItem.Properties != null &&
                                        currentItem.Properties[currentPropertyName] != null &&
                                        currentItem.Properties[currentPropertyName].Value != null)
                                        CmdletProviderContext.WriteObject(currentItem.Properties[currentPropertyName].Value);
