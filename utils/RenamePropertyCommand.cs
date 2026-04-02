    /// A command to rename a property of an item at a specified path.
    [Cmdlet(VerbsCommon.Rename, "ItemProperty", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097152")]
    public class RenameItemPropertyCommand : PassThroughItemPropertyCommandBase
                return _path;
        /// The properties to be renamed on the item.
        /// The new name of the property on the item.
        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
                return InvokeProvider.Property.RenamePropertyDynamicParameters(Path, Name, NewName, context);
            return InvokeProvider.Property.RenamePropertyDynamicParameters(".", Name, NewName, context);
        /// The path to rename the property on.
        /// Renames a property on an item.
                InvokeProvider.Property.Rename(_path, Name, NewName, currentContext);
