    /// A command to set the property of an item at a specified path.
    [Cmdlet(VerbsCommon.Set, "ItemProperty", DefaultParameterSetName = "propertyValuePathSet", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097147")]
    public class SetItemPropertyCommand : PassThroughItemPropertyCommandBase
        private const string propertyValuePathSet = "propertyValuePathSet";
        private const string propertyValueLiteralPathSet = "propertyValueLiteralPathSet";
        private const string propertyPSObjectPathSet = "propertyPSObjectPathSet";
        private const string propertyPSObjectLiteralPathSet = "propertyPSObjectLiteralPathSet";
        [Parameter(Position = 0, ParameterSetName = propertyPSObjectPathSet,
        [Parameter(Position = 0, ParameterSetName = propertyValuePathSet,
        [Parameter(ParameterSetName = propertyValueLiteralPathSet,
        [Parameter(ParameterSetName = propertyPSObjectLiteralPathSet,
        #region Property Value set
        /// The name of the property to set.
        /// This value type is determined by the InvokeProvider.
        [Parameter(Position = 1, ParameterSetName = propertyValuePathSet,
        [Parameter(Position = 1, ParameterSetName = propertyValueLiteralPathSet,
        public string Name { get; set; } = string.Empty;
        /// The value of the property to set.
        [Parameter(Position = 2, ParameterSetName = propertyValuePathSet,
        [Parameter(Position = 2, ParameterSetName = propertyValueLiteralPathSet,
        #endregion Property Value set
        #region Shell object set
        /// A PSObject that contains the properties and values to be set.
        [Parameter(ParameterSetName = propertyPSObjectPathSet, Mandatory = true,
        [Parameter(ParameterSetName = propertyPSObjectLiteralPathSet, Mandatory = true,
        public PSObject InputObject { get; set; }
        #endregion Shell object set
            PSObject mshObject = null;
                case propertyValuePathSet:
                case propertyValueLiteralPathSet:
                    if (!string.IsNullOrEmpty(Name))
                        mshObject = new PSObject();
                        mshObject.Properties.Add(new PSNoteProperty(Name, Value));
                    mshObject = InputObject;
                return InvokeProvider.Property.SetPropertyDynamicParameters(Path[0], mshObject, context);
            return InvokeProvider.Property.SetPropertyDynamicParameters(".", mshObject, context);
        /// Sets the content of the item at the specified path.
                case propertyPSObjectPathSet:
                    Diagnostics.Assert(
                        "One of the parameter sets should have been resolved or an error should have been thrown by the command processor");
                    InvokeProvider.Property.Set(path, mshObject, currentCommandContext);
