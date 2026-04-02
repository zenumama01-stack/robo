    /// This interface needs to be implemented by providers that want users to see
    /// provider-specific help.
    public interface ICmdletProviderSupportsHelp
        /// Called by the help system to get provider-specific help from the provider.
        /// Full path to the current location of the user or the full path to
        /// the location of the property that the user needs help about.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Maml", Justification = "Maml is an acronym.")]
        string GetHelpMaml(string helpItemName, string path);
    #region CmdletProvider
    /// The base class for Cmdlet provider.
    public abstract partial class CmdletProvider : IResourceSupplier
        /// The context under which the provider is running. This will change between each
        /// invocation of a method in this class or derived classes.
        private CmdletProviderContext _contextBase = null;
        /// The information that the Monad engine stores on behalf of the provider.
        private ProviderInfo _providerInformation = null;
        /// using "CmdletProviderClasses" as the category.
        [TraceSource(
             "CmdletProviderClasses",
             "The namespace provider base classes tracer")]
        internal static readonly PSTraceSource providerBaseTracer = PSTraceSource.GetTracer(
                                                               "The namespace provider base classes tracer");
        /// Sets the provider information that is stored in the Monad engine into the
        /// provider base class.
        /// <param name="providerInfoToSet">
        /// The provider information that is stored by the Monad engine.
        /// If <paramref name="providerInformation"/> is null.
        internal void SetProviderInformation(ProviderInfo providerInfoToSet)
            if (providerInfoToSet == null)
                throw PSTraceSource.NewArgumentNullException(nameof(providerInfoToSet));
            _providerInformation = providerInfoToSet;
        /// Checks whether the filter of the provider is set.
        /// Can be overridden by derived class when additional filters are defined.
        /// Whether the filter of the provider is set.
        internal virtual bool IsFilterSet()
            bool filterSet = !string.IsNullOrEmpty(Filter);
            return filterSet;
        #region CmdletProvider method wrappers
        /// Gets or sets the context for the running command.
        /// On set, if the context contains credentials and the provider
        /// doesn't support credentials, or if the context contains a filter
        /// parameter and the provider does not support filters.
        internal CmdletProviderContext Context
                return _contextBase;
                // Check that the provider supports the use of credentials
                if (value.Credential != null &&
                    value.Credential != PSCredential.Empty &&
                    !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Credentials, _providerInformation))
                        SessionStateStrings.Credentials_NotSupported);
                // Supplying Credentials for the FileSystemProvider is supported only for New-PSDrive Command.
                if (_providerInformation != null && !string.IsNullOrEmpty(_providerInformation.Name) && _providerInformation.Name.Equals("FileSystem") &&
                    value.Credential != null &&
                    !value.ExecutionContext.CurrentCommandProcessor.Command.GetType().Name.Equals("NewPSDriveCommand"))
                        SessionStateStrings.FileSystemProviderCredentials_NotSupported);
                // Check that the provider supports the use of filters
                if ((!string.IsNullOrEmpty(value.Filter)) &&
                    (!CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Filter, _providerInformation)))
                        SessionStateStrings.Filter_NotSupported);
                // Check that the provider supports the use of transactions if the command
                // requested it
                if ((value.UseTransaction) &&
                   (!CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Transactions, _providerInformation)))
                        SessionStateStrings.Transactions_NotSupported);
                _contextBase = value;
                _contextBase.ProviderInstance = this;
        /// Called when the provider is first initialized. It sets the context
        /// of the call and then calls the derived providers Start method.
        /// The information about the provider.
        /// <param name="cmdletProviderContext">
        internal ProviderInfo Start(ProviderInfo providerInfo, CmdletProviderContext cmdletProviderContext)
            Context = cmdletProviderContext;
            return Start(providerInfo);
        /// Gets an object that defines the additional parameters for the Start implementation
        internal object StartDynamicParameters(CmdletProviderContext cmdletProviderContext)
            return StartDynamicParameters();
        /// Called when the provider is being removed. It sets the context
        /// of the call and then calls the derived providers Stop method.
        internal void Stop(CmdletProviderContext cmdletProviderContext)
        /// <Content contentref="System.Management.Automation.Cmdlet.StopProcessing" />
        protected internal virtual void StopProcessing()
        #endregion CmdletProvider method wrappers
        #region IPropertyCmdletProvider method wrappers
        /// Internal wrapper for the GetProperty protected method. This method will
        /// only be called if the provider implements the IPropertyCmdletProvider interface.
            CmdletProviderContext cmdletProviderContext)
            if (this is not IPropertyCmdletProvider propertyProvider)
                        SessionStateStrings.IPropertyCmdletProvider_NotSupported);
            // Call interface method
            propertyProvider.GetProperty(path, providerSpecificPickList);
        /// Gives the provider a chance to attach additional parameters to
        /// the get-itemproperty cmdlet.
            return propertyProvider.GetPropertyDynamicParameters(path, providerSpecificPickList);
        /// Internal wrapper for the SetProperty protected method. This method will
            propertyProvider.SetProperty(path, propertyValue);
        /// the set-itemproperty cmdlet.
            return propertyProvider.SetPropertyDynamicParameters(path, propertyValue);
        /// Internal wrapper for the ClearProperty protected method. This method will
        /// The path to the item from which the property should be cleared.
        /// The name of the property that should be cleared.
        /// Implement this method when you are providing access to a data store
        /// that allows dynamic clearing of properties.
            Collection<string> propertyName,
            propertyProvider.ClearProperty(path, propertyName);
        /// the clear-itemproperty cmdlet.
        /// A list of properties that should be cleared. If this parameter is null
        /// or empty, all properties should be cleared.
            return propertyProvider.ClearPropertyDynamicParameters(path, providerSpecificPickList);
        /// Internal wrapper for the NewProperty protected method. This method will
        /// only be called if the provider implements the IDynamicPropertyCmdletProvider interface.
        /// that allows dynamic creation of properties.
            if (this is not IDynamicPropertyCmdletProvider propertyProvider)
                        SessionStateStrings.IDynamicPropertyCmdletProvider_NotSupported);
            propertyProvider.NewProperty(path, propertyName, propertyTypeName, value);
        /// the new-itemproperty cmdlet.
            return propertyProvider.NewPropertyDynamicParameters(path, propertyName, propertyTypeName, value);
        /// Internal wrapper for the RemoveProperty protected method. This method will
        /// The name of the property to be removed
        /// that allows dynamic removal of properties.
            propertyProvider.RemoveProperty(path, propertyName);
        /// the remove-itemproperty cmdlet.
            return propertyProvider.RemovePropertyDynamicParameters(path, propertyName);
        /// Internal wrapper for the RenameProperty protected method. This method will
        /// The path to the item on which the property should be renamed.
        /// The name of the property that should be renamed.
        /// <param name="newPropertyName">
        /// that allows dynamic renaming of properties.
            string newPropertyName,
            propertyProvider.RenameProperty(path, propertyName, newPropertyName);
        /// the rename-itemproperty cmdlet.
        /// The name of the property to rename it to.
            return propertyProvider.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty);
        /// Internal wrapper for the CopyProperty protected method. This method will
        /// The path to the item from which the property should be copied.
        /// The name of the property that should be copied.
        /// The path to the item to which the property should be copied.
        /// The name of the property that should be copied to.
        /// that allows dynamic copying of properties.
            propertyProvider.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty);
        /// the copy-itemproperty cmdlet.
            return propertyProvider.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty);
        /// Internal wrapper for the MoveProperty protected method. This method will
        /// The path to the item from which the property should be moved.
        /// The name of the property that should be moved.
        /// The path to the item to which the property should be moved.
        /// The name of the property that should be moved to.
        /// that allows dynamic moving of properties.
            propertyProvider.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty);
        /// the move-itemproperty cmdlet.
            return propertyProvider.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty);
        #endregion IDynamicPropertyCmdletProvider method wrappers
        #region IContentCmdletProvider method wrappers
        /// Internal wrapper for the GetContentReader protected method. This method will
        /// only be called if the provider implements the IContentCmdletProvider interface.
        /// The path to the item to retrieve content from.
        /// An instance of the IContentReader for the specified path.
        internal IContentReader GetContentReader(
            if (this is not IContentCmdletProvider contentProvider)
                        SessionStateStrings.IContentCmdletProvider_NotSupported);
            return contentProvider.GetContentReader(path);
        /// the get-content cmdlet.
            return contentProvider.GetContentReaderDynamicParameters(path);
        /// Internal wrapper for the GetContentWriter protected method. This method will
        /// The path to the item to set content on.
        /// An instance of the IContentWriter for the specified path.
        internal IContentWriter GetContentWriter(
            return contentProvider.GetContentWriter(path);
        /// the add-content and set-content cmdlet.
            return contentProvider.GetContentWriterDynamicParameters(path);
        /// Internal wrapper for the ClearContent protected method. This method will
            contentProvider.ClearContent(path);
        /// the clear-content cmdlet.
            return contentProvider.ClearContentDynamicParameters(path);
        #endregion IContentCmdletProvider method wrappers
        /// Gives the provider the opportunity to initialize itself.
        /// The information about the provider that is being started.
        /// The default implementation returns the ProviderInfo instance that
        /// was passed.
        /// To have session state maintain persisted data on behalf of the provider,
        /// the provider should derive from <see cref="System.Management.Automation.ProviderInfo"/>
        /// and add any properties or
        /// methods for the data it wishes to persist.  When Start gets called the
        /// provider should construct an instance of its derived ProviderInfo using the
        /// providerInfo that is passed in and return that new instance.
        protected virtual ProviderInfo Start(ProviderInfo providerInfo)
        protected virtual object StartDynamicParameters()
        /// Called by session state when the provider is being removed.
        /// A provider should override this method to free up any resources that the provider
        /// was using.
        protected virtual void Stop()
        /// Indicates whether stop has been requested on this provider.
                        Context != null,
                        "The context should always be set");
                    return Context.Stopping;
                    return new SessionState(Context.ExecutionContext.EngineSessionState);
                    return new ProviderIntrinsics(Context.ExecutionContext.EngineSessionState);
        /// Gets the instance of the command invocation APIs for the current runspace.
                    return new CommandInvocationIntrinsics(Context.ExecutionContext);
        /// Gets the credentials under which the operation should run.
                    return Context.Credential;
        /// The information about the provider that is stored in the runspace
        /// on behalf of the provider.
        /// If a derived type of ProviderInfo was returned from the Start method, it
        /// will be set here in all subsequent calls to the provider.
        protected internal ProviderInfo ProviderInfo
                    return _providerInformation;
        /// The drive information associated with the context of the current operation.
        protected PSDriveInfo PSDriveInfo
                    return Context.Drive;
        /// The dynamic parameters object populated with the values as specified
        /// by the user.
        protected object DynamicParameters
                    return Context.DynamicParameters;
        /// Gets the force property.
                    return Context.Force;
        /// Gets the provider specific filter that was supplied by the caller.
                    return Context.Filter;
        /// Gets the include wildcard patterns which is used to determine which items
        /// will be included when taking an action.
        public Collection<string> Include
                    return Context.Include;
        /// Gets the exclude wildcard patterns which is used to determine which items
        /// will be excluded when taking an action.
        public Collection<string> Exclude
                    return Context.Exclude;
                    return Context.ExecutionContext.EngineHostInterface;
        public virtual char ItemSeparator => Path.DirectorySeparatorChar;
        public virtual char AltItemSeparator =>
            '\\';
            Path.AltDirectorySeparatorChar;
        #region IResourceSupplier
        /// Gets the resource string corresponding to baseName and
        /// resourceId from the current assembly. You should override
        /// this if you require a different behavior.
        /// <param name="baseName">
        /// the base resource name
        /// the resource id
        /// the resource string corresponding to baseName and resourceId
        /// When overriding this method, the resource string for the specified
        /// resource should be retrieved from a localized resource assembly.
                    throw PSTraceSource.NewArgumentException(nameof(baseName));
                    throw PSTraceSource.NewArgumentException(nameof(resourceId));
                        this.GetType().Assembly,
                        baseName);
                    retValue = manager.GetString(resourceId,
                                                  System.Globalization.CultureInfo.CurrentUICulture);
        #endregion IResourceSupplier
        /// <Content contentref="System.Management.Automation.Cmdlet.ThrowTerminatingError" />
                        ProviderInfo.Name,
                // We can't play the same game as Cmdlet.ThrowTerminatingError
                //  and save the exception in the "pipeline".  We need to pass
                //  the actual exception as a thrown exception.  So, we wrap
                //  it in ProviderInvocationException.
                ProviderInvocationException providerInvocationException =
                    new ProviderInvocationException(ProviderInfo, errorRecord);
                    providerInvocationException,
                throw providerInvocationException;
        /// <Content contentref="System.Management.Automation.Cmdlet.ShouldProcess" />
                return Context.ShouldProcess(target);
                return Context.ShouldProcess(target, action);
                return Context.ShouldProcess(
        /// <Content contentref="System.Management.Automation.Cmdlet.ShouldContinue" />
                return Context.ShouldContinue(query, caption);
                return Context.ShouldContinue(
                    return Context.TransactionAvailable();
                    return Context.CurrentPSTransaction;
        /// <Content contentref="System.Management.Automation.Cmdlet.WriteVerbose" />
                Context.WriteVerbose(text);
        /// <Content contentref="System.Management.Automation.Cmdlet.WriteWarning" />
                Context.WriteWarning(text);
        /// <Content contentref="System.Management.Automation.Cmdlet.WriteProgress" />
                Context.WriteProgress(progressRecord);
        /// <Content contentref="System.Management.Automation.Cmdlet.WriteDebug" />
                Context.WriteDebug(text);
        /// <Content contentref="System.Management.Automation.Cmdlet.WriteInformation" />
        public void WriteInformation(InformationRecord record)
                Context.WriteInformation(record);
                Context.WriteInformation(messageData, tags);
        /// Converts the incoming object to a PSObject and then adds extra
        /// data as notes. Then it writes the shell object to the context.
        /// The item being written out.
        /// The path of the item being written out.
        /// True if the item is a container, false otherwise.
        private void WriteObject(
            object item,
            PSObject result = WrapOutputInPSObject(item, path);
            // Now add the IsContainer
            result.AddOrSetProperty("PSIsContainer", isContainer ? Boxed.True : Boxed.False);
            providerBaseTracer.WriteLine("Attaching {0} = {1}", "PSIsContainer", isContainer);
            Context.WriteObject(result);
        /// Wraps the item in a PSObject and attaches some notes to the
        /// object that deal with path information.
        /// The item to be wrapped.
        /// A PSObject that wraps the item and has path information attached
        /// as notes.
        /// if <paramref name="item"/> is null.
        private PSObject WrapOutputInPSObject(
            PSObject result = new PSObject(item);
                ProviderInfo != null,
                "The ProviderInfo should always be set");
            // Move the TypeNames to the wrapping object if the wrapped object
            // was an PSObject
            PSObject mshObj = item as PSObject;
                result.InternalTypeNames = new ConsolidatedString(mshObj.InternalTypeNames);
            string providerQualifiedPath =
                LocationGlobber.GetProviderQualifiedPath(path, ProviderInfo);
            result.AddOrSetProperty("PSPath", providerQualifiedPath);
            providerBaseTracer.WriteLine("Attaching {0} = {1}", "PSPath", providerQualifiedPath);
            NavigationCmdletProvider navProvider = this as NavigationCmdletProvider;
            if (navProvider != null && path != null)
                // Get the parent path
                    parentPath = navProvider.GetParentPath(path, PSDriveInfo.Root, Context);
                    parentPath = navProvider.GetParentPath(path, string.Empty, Context);
                string providerQualifiedParentPath = string.Empty;
                if (!string.IsNullOrEmpty(parentPath))
                    providerQualifiedParentPath =
                        LocationGlobber.GetProviderQualifiedPath(parentPath, ProviderInfo);
                result.AddOrSetProperty("PSParentPath", providerQualifiedParentPath);
                providerBaseTracer.WriteLine("Attaching {0} = {1}", "PSParentPath", providerQualifiedParentPath);
                string childName = navProvider.GetChildName(path, Context);
                result.AddOrSetProperty("PSChildName", childName);
                providerBaseTracer.WriteLine("Attaching {0} = {1}", "PSChildName", childName);
                // Add a commonstat structure to file system objects
                if (ProviderInfo.ImplementingType == typeof(Microsoft.PowerShell.Commands.FileSystemProvider))
                        // Use LStat because if you get a link, you want the information about the
                        // link, not the file.
                        var commonStat = Platform.Unix.GetLStat(path);
                        result.AddOrSetProperty("UnixStat", commonStat);
                        // If there is *any* problem in retrieving the stat information
                        // set the property to null. There is no specific exception which
                        // would result in different behavior.
                        result.AddOrSetProperty("UnixStat", value: null);
                result.AddOrSetProperty(this.PSDriveInfo.GetNotePropertyForProviderCmdlets("PSDrive"));
                providerBaseTracer.WriteLine("Attaching {0} = {1}", "PSDrive", this.PSDriveInfo);
            result.AddOrSetProperty(this.ProviderInfo.GetNotePropertyForProviderCmdlets("PSProvider"));
            providerBaseTracer.WriteLine("Attaching {0} = {1}", "PSProvider", this.ProviderInfo);
        /// Writes an item to the output as a PSObject with extra data attached
        /// The item to be written.
        /// The path of the item being written.
        public void WriteItemObject(
                WriteObject(item, path, isContainer);
        /// Writes a property object to the output as a PSObject with extra data attached
        /// The properties to be written.
        public void WritePropertyObject(
            object propertyValue,
                WriteObject(propertyValue, path);
        /// Writes a Security Descriptor object to the output as a PSObject with extra data attached
        /// The Security Descriptor to be written.
        /// The path of the item from which the Security Descriptor was retrieved.
        public void WriteSecurityDescriptorObject(
                WriteObject(securityDescriptor, path);
        /// <Content contentref="System.Management.Automation.Cmdlet.WriteError" />
                        errorRecord.ErrorDetails.TextLookupError,
                Context.WriteError(errorRecord);
    #endregion CmdletProvider
