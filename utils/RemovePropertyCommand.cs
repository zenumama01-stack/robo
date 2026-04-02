    /// A command to remove a property from an item.
    [Cmdlet(VerbsCommon.Remove, "ItemProperty", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097013")]
    public class RemoveItemPropertyCommand : ItemPropertyCommandBase
            get { return _property; }
            set { _property = value ?? Array.Empty<string>(); }
            string propertyName = null;
                return InvokeProvider.Property.RemovePropertyDynamicParameters(Path[0], propertyName, context);
            return InvokeProvider.Property.RemovePropertyDynamicParameters(".", propertyName, context);
        /// Removes the property from the item.
                foreach (string prop in Name)
                        InvokeProvider.Property.Remove(path, prop, CmdletProviderContext);
