    /// A command to move a property on an item to another item.
    [Cmdlet(VerbsCommon.Move, "ItemProperty", SupportsShouldProcess = true, DefaultParameterSetName = "Path", SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096817")]
    public class MoveItemPropertyCommand : PassThroughItemPropertyCommandBase
                value ??= Array.Empty<string>();
            string propertyName = string.Empty;
            if (Name != null && Name.Length > 0)
                propertyName = Name[0];
                return InvokeProvider.Property.MovePropertyDynamicParameters(Path[0], propertyName, Destination, propertyName, context);
            return InvokeProvider.Property.MovePropertyDynamicParameters(
        /// The property to be created.
        private string[] _property = Array.Empty<string>();
        /// Creates the property on the item.
                foreach (string propertyName in Name)
                        InvokeProvider.Property.Move(path, propertyName, Destination, propertyName, GetCurrentContext());
