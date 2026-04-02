    /// This provider is the data accessor for shell aliases. It uses
    /// the SessionStateProviderBase as the base class to produce a view on
    /// session state data.
    [CmdletProvider(AliasProvider.ProviderName, ProviderCapabilities.ShouldProcess)]
    [OutputType(typeof(AliasInfo), ProviderCmdlet = ProviderCmdlet.SetItem)]
    [OutputType(typeof(AliasInfo), ProviderCmdlet = ProviderCmdlet.RenameItem)]
    [OutputType(typeof(AliasInfo), ProviderCmdlet = ProviderCmdlet.CopyItem)]
    [OutputType(typeof(AliasInfo), ProviderCmdlet = ProviderCmdlet.GetChildItem)]
    [OutputType(typeof(AliasInfo), ProviderCmdlet = ProviderCmdlet.NewItem)]
    public sealed class AliasProvider : SessionStateProviderBase
        public const string ProviderName = "Alias";
        /// The constructor for the provider that exposes variables to the user
        /// as drives.
        public AliasProvider()
        /// Initializes the alias drive.
        /// An array of a single PSDriveInfo object representing the alias drive.
            string description = SessionStateStrings.AliasDriveDescription;
            PSDriveInfo aliasDrive =
                    DriveNames.AliasDrive,
                    ProviderInfo,
                    description,
            drives.Add(aliasDrive);
        #region Dynamic Parameters
        /// Gets the dynamic parameters for the NewItem cmdlet.
        /// An instance of AliasProviderDynamicParameters which is the dynamic parameters for
        /// NewItem.
        protected override object NewItemDynamicParameters(string path, string type, object newItemValue)
            return new AliasProviderDynamicParameters();
        /// SetItem.
        #endregion Dynamic Parameters
        /// Gets a alias from session state.
        /// A DictionaryEntry that represents the value of the alias.
        internal override object GetSessionStateItem(string name)
                !string.IsNullOrEmpty(name),
                "The caller should verify this parameter");
            AliasInfo value = SessionState.Internal.GetAlias(name, Context.Origin);
        /// Since items are often more than their value, this method should
        /// be overridden to provide the value for an item.
        /// The item to extract the value from.
        /// The value of the specified item.
        /// The default implementation will get
        /// the Value property of a DictionaryEntry
        internal override object GetValueOfItem(object item)
                item != null,
                "Caller should verify the item parameter");
            object value = item;
            AliasInfo aliasInfo = item as AliasInfo;
            if (aliasInfo != null)
                value = aliasInfo.Definition;
        /// Sets the alias of the specified name to the specified value.
        /// The new value for the alias.
        /// <param name="writeItem">
        /// If true, the item that was set should be written to WriteItemObject.
        internal override void SetSessionStateItem(string name, object value, bool writeItem)
            AliasProviderDynamicParameters dynamicParameters =
                DynamicParameters as AliasProviderDynamicParameters;
            AliasInfo item = null;
            bool dynamicParametersSpecified = dynamicParameters != null && dynamicParameters.OptionsSet;
                if (dynamicParametersSpecified)
                    item = (AliasInfo)GetSessionStateItem(name);
                    item?.SetOptions(dynamicParameters.Options, Force);
                    RemoveSessionStateItem(name);
                    string stringValue = value as string;
                            item = SessionState.Internal.SetAliasValue(name, stringValue, dynamicParameters.Options, Force, Context.Origin);
                            item = SessionState.Internal.SetAliasValue(name, stringValue, Force, Context.Origin);
                    AliasInfo alias = value as AliasInfo;
                        AliasInfo newAliasInfo =
                            new AliasInfo(
                                this.Context.ExecutionContext,
                            newAliasInfo.SetOptions(dynamicParameters.Options, Force);
                        item = SessionState.Internal.SetAliasItem(newAliasInfo, Force, Context.Origin);
            if (writeItem && item != null)
                WriteItemObject(item, item.Name, false);
        /// Removes the specified alias from session state.
        /// The name of the alias to remove from session state.
        internal override void RemoveSessionStateItem(string name)
            SessionState.Internal.RemoveAlias(name, Force);
        /// Gets a flattened view of the alias in session state.
        /// An IDictionary representing the flattened view of the aliases in
        internal override IDictionary GetSessionStateTable()
            return (IDictionary)SessionState.Internal.GetAliasTable();
        /// Determines if the item can be renamed. Derived classes that need
        /// to perform a check should override this method.
        /// The item to verify if it can be renamed.
        /// true if the item can be renamed or false otherwise.
        internal override bool CanRenameItem(object item)
                    ((aliasInfo.Options & ScopedItemOptions.ReadOnly) != 0 && !Force))
                            aliasInfo.Name,
                            "CannotRenameAlias",
                            SessionStateStrings.CannotRenameAlias);
    /// The dynamic parameter object for the AliasProvider SetItem and NewItem commands.
    public class AliasProviderDynamicParameters
        /// Gets or sets the option parameter for the alias.
                _optionsSet = true;
        private ScopedItemOptions _options;
        /// Determines if the Options parameter was set.
        internal bool OptionsSet
            get { return _optionsSet; }
        private bool _optionsSet = false;
