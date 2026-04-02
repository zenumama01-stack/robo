    /// Provider that provides access to Registry through cmdlets. This provider
    /// implements <see cref="System.Management.Automation.Provider.NavigationCmdletProvider"/>,
    /// <see cref="System.Management.Automation.Provider.IPropertyCmdletProvider"/>,
    /// <see cref="System.Management.Automation.Provider.IDynamicPropertyCmdletProvider"/>,
    /// <see cref="System.Management.Automation.Provider.ISecurityDescriptorCmdletProvider"/>
    /// interfaces.
    /// INSTALLATION:
    /// Type the following at a PowerShell prompt:
    /// new-PSProvider -Path "REG.cmdletprovider" -description "My registry navigation provider"
    /// TO EXERCISE THE PROVIDER:
    /// Get-PSDrive
    /// set-location HKLM:\software
    /// get-childitem
    /// New-PSDrive -PSProvider REG -name HKCR -root HKEY_CLASSES_ROOT\CLSID
    /// set-location HKCR:
    /// get-childitem "{0000*"
    /// The CmdletProvider attribute defines the name and capabilities of the provider.
    /// The first parameter is the default friendly name for the provider. The second parameter
    /// is the provider name which, along with some assembly information like version, company, etc.
    /// is used as a fully-qualified provider name which can be used for disambiguation.
    /// The third parameter states the capabilities of the provider.
#if CORECLR // System.Transaction namespace is not in CoreClr.
    [CmdletProvider(RegistryProvider.ProviderName, ProviderCapabilities.ShouldProcess)]
    [CmdletProvider(RegistryProvider.ProviderName, ProviderCapabilities.ShouldProcess | ProviderCapabilities.Transactions)]
    [OutputType(typeof(string), ProviderCmdlet = ProviderCmdlet.MoveItemProperty)]
    [OutputType(typeof(RegistryKey), typeof(string), ProviderCmdlet = ProviderCmdlet.GetChildItem)]
    [OutputType(typeof(RegistryKey), ProviderCmdlet = ProviderCmdlet.GetItem)]
    [OutputType(typeof(System.Security.AccessControl.RegistrySecurity), ProviderCmdlet = ProviderCmdlet.GetAcl)]
    [OutputType(typeof(Microsoft.Win32.RegistryKey), ProviderCmdlet = ProviderCmdlet.GetChildItem)]
    [OutputType(typeof(RegistryKey), typeof(string), typeof(Int32), typeof(Int64), ProviderCmdlet = ProviderCmdlet.GetItemProperty)]
    [OutputType(typeof(RegistryKey), ProviderCmdlet = ProviderCmdlet.NewItem)]
    public sealed partial class RegistryProvider :
        NavigationCmdletProvider,
        IDynamicPropertyCmdletProvider,
        ISecurityDescriptorCmdletProvider
        /// using "ProviderProvider" as the category.
        [Dbg.TraceSourceAttribute(
            "RegistryProvider",
            "The namespace navigation provider for the Windows Registry")]
            Dbg.PSTraceSource.GetTracer("RegistryProvider",
            "The namespace navigation provider for the Windows Registry");
        public const string ProviderName = "Registry";
        #region CmdletProvider overrides
        public override char AltItemSeparator => ItemSeparator;
        /// Verifies that the new drive has a valid root.
        /// <returns>A PSDriveInfo object.</returns>
        /// It also givesthe provider an opportunity to return a
        /// derived class of PSDriveInfo which can contain provider specific
        /// information about the drive.This may be done for performance
        /// or reliability reasons or toprovide extra data to all calls
        /// using the drive
            if (!ItemExists(drive.Root))
                Exception e = new ArgumentException(RegistryProviderStrings.NewDriveRootDoesNotExist);
                    e.GetType().FullName,
                    drive.Root));
        /// Creates HKEY_LOCAL_MACHINE and HKEY_CURRENT_USER registry drives during provider initialization.
        /// machine is joined to a domain.  The FileSystem mounts all drives then available.
            drives.Add(
                    "HKLM",
                    "HKEY_LOCAL_MACHINE",
                    RegistryProviderStrings.HKLMDriveDescription,
                    "HKCU",
                    "HKEY_CURRENT_USER",
                    RegistryProviderStrings.HKCUDriveDescription,
        #region ItemCmdletProvider overrides
        /// Determines if the specified <paramref name="path"/> is syntactically and semantically valid.
        /// True if the path is valid, or False otherwise.
                // There really aren't any illegal characters or syntactical patterns
                // to validate, so just ensure that the path starts with one of the hive roots.
                string root = NormalizePath(path);
                root = root.TrimStart(StringLiterals.DefaultPathSeparator);
                root = root.TrimEnd(StringLiterals.DefaultPathSeparator);
                int pathSeparator = root.IndexOf(StringLiterals.DefaultPathSeparator);
                if (pathSeparator != -1)
                    root = root.Substring(0, pathSeparator);
                if (string.IsNullOrEmpty(root))
                    // An empty path means that we are at the root and should
                    // enumerate the hives. So that is a valid path.
                if (GetHiveRoot(root) == null)
        /// Gets the RegistryKey item at the specified <paramref name="path"/>
        /// and writes it to the pipeline using the WriteObject method.
        /// Any non-terminating exceptions are written to the WriteError method.
        /// The path to the key to retrieve.
            // Get the registry item
            IRegistryWrapper result = GetRegkeyForPathWriteIfError(path, false);
            // Write out the result
            WriteRegistryItemObject(result, path);
        /// Sets registry values at <paramref name="path "/> to the <paramref name="value"/> specified.
        /// The path to the item that is to be set. Only registry values can be set using
        /// The new value for the registry value.
            // Confirm the set item with the user
            string action = RegistryProviderStrings.SetItemAction;
            string resourceTemplate = RegistryProviderStrings.SetItemResourceTemplate;
                IRegistryWrapper key = GetRegkeyForPathWriteIfError(path, true);
                // Check to see if the type was specified by the user
                    RegistryProviderSetItemDynamicParameter dynParams =
                        DynamicParameters as RegistryProviderSetItemDynamicParameter;
                            // Convert the parameter to a RegistryValueKind
                            RegistryValueKind kind = dynParams.Type;
                            key.SetValue(null, value, kind);
                            valueSet = true;
                            WriteError(new ErrorRecord(argException, argException.GetType().FullName, ErrorCategory.InvalidArgument, null));
                            key.Close();
                        catch (System.IO.IOException ioException)
                            // An exception occurred while trying to get the key. Write
                            // out the error.
                            WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.WriteError, path));
                            WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        catch (System.UnauthorizedAccessException unauthorizedAccessException)
                            WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, path));
                        // Set the value
                        key.SetValue(null, value);
                // Since SetValue can munge the data to a specified
                // type (RegistryValueKind), retrieve the value again
                // to output it in the correct form to the user.
                result = ReadExistingKeyValue(key, null);
                WriteItemObject(result, path, false);
        /// Gets the dynamic parameters for the SetItem method.
        /// An instance of the <see cref="Microsoft.PowerShell.Commands.RegistryProviderSetItemDynamicParameter"/> class which
        /// contains a parameter for the Type.
            return new RegistryProviderSetItemDynamicParameter();
        /// Clears the item at the specified <paramref name="path"/>.
        /// The path to the item that is to be cleared. Only registry values can be cleared using
        /// The registry provider implements this by removing all the values for the specified key.
        /// The item that is cleared is written to the WriteObject method.
        /// If the path is to a value, then an ArgumentException is written.
            // Confirm the clear item with the user
            string action = RegistryProviderStrings.ClearItemAction;
            string resourceTemplate = RegistryProviderStrings.ClearItemResourceTemplate;
                string[] valueNames;
                    // Remove each value
                    valueNames = key.GetValueNames();
                    WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.ReadError, path));
                for (int index = 0; index < valueNames.Length; ++index)
                        key.DeleteValue(valueNames[index]);
                        // An exception occurred while trying to delete the value. Write
                // Write out the key
                WriteRegistryItemObject(key, path);
        #endregion ItemCmdletProvider overrides
        #region ContainerCmdletProvider overrides
        /// Gets all the child keys and values of the key at the specified <paramref name="path"/>.
        /// The path to the key to get the child keys of.
        /// </param>/
        /// Determines if the call should be recursive. If true, all subkeys of
        /// the key at the specified path will be written. If false, only the
        /// immediate children of the key at the specified path will be written.
            s_tracer.WriteLine("recurse = {0}, depth = {1}", recurse, depth);
            if (IsHiveContainer(path))
                // If the path is empty or it is / or \, return all the hives
                foreach (string hiveName in s_hiveNames)
                    GetItem(hiveName);
                // Get the key at the specified path
                IRegistryWrapper key = GetRegkeyForPathWriteIfError(path, false);
                    // Get all the subkeys of the specified path
                    string[] keyNames = key.GetSubKeyNames();
                    if (keyNames != null)
                        foreach (string subkeyName in keyNames)
                            if (!string.IsNullOrEmpty(subkeyName))
                                string keypath = path;
                                    // Generate the path for each key name
                                    keypath = MakePath(path, subkeyName, childIsLeaf: true);
                                    if (!string.IsNullOrEmpty(keypath))
                                        // Call GetItem to retrieve the RegistryKey object
                                        // and write it to the WriteObject method.
                                        IRegistryWrapper resultKey = GetRegkeyForPath(keypath, false);
                                        if (resultKey != null)
                                            WriteRegistryItemObject(resultKey, keypath);
                                        // Now recurse if necessary
                                                GetChildItems(keypath, recurse, depth - 1);
                                    WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.ReadError, keypath));
                                    WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, keypath));
                                    WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, keypath));
        /// Gets all the child key and value names of the key at the specified <paramref name="path"/>.
        /// The path to the key to get the child names from.
        /// Ignored since the registry provider does not implement filtering.
        /// Normally, if this parameter is ReturnAllContainers then all subkeys should be
        /// returned. If it is false, then only those subkeys that match the
        /// filter should be returned.
                // If the path is empty get the names of the hives
                    WriteItemObject(hiveName, hiveName, true);
                    // Get the child key names
                    string[] results = key.GetSubKeyNames();
                    // Write the child key names to the WriteItemObject method
                    for (int index = 0; index < results.Length; ++index)
                        string childName = EscapeChildName(results[index]);
                        string childPath = MakePath(path, childName, childIsLeaf: true);
                        WriteItemObject(childName, childPath, true);
        private const string charactersThatNeedEscaping = ".*?[]:";
        /// Escapes the characters in the registry key path that are used by globbing and
        /// The path to escape.
        /// The escaped path.
        /// This method handles surrogate pairs. Please see msdn documentation
        private static string EscapeSpecialChars(string path)
            // Get the text enumerator..this will iterate through each character
            // the character can be a surrogate pair
            System.Globalization.TextElementEnumerator textEnumerator =
                System.Globalization.StringInfo.GetTextElementEnumerator(path);
                textEnumerator != null,
                string.Create(CultureInfo.CurrentCulture, $"Cannot get a text enumerator for name {path}"));
            while (textEnumerator.MoveNext())
                // Iterate through each element and findout whether
                // any text needs escaping
                string textElement = textEnumerator.GetTextElement();
                // NTRAID#Windows Out of Band Releases-939036-2006/07/12-LeeHolm
                // A single character can never contain a string of
                // charactersThatNeedEscaping, so this method does nothing.  The fix
                // is to remove all calls to this escaping code, though, as this escaping
                // should not be done.
                if (textElement.Contains(charactersThatNeedEscaping))
                    // This text element needs escaping
                    result.Append('`');
                result.Append(textElement);
        /// Escapes the characters in the registry key name that are used by globbing and
        /// The name to escape.
        /// The escaped name.
        private static string EscapeChildName(string name)
                System.Globalization.StringInfo.GetTextElementEnumerator(name);
                string.Create(CultureInfo.CurrentCulture, $"Cannot get a text enumerator for name {name}"));
        /// Renames the key at the specified <paramref name="path"/> to <paramref name="newName"/>.
        /// The path to the key to rename.
        /// The new name of the key.
            s_tracer.WriteLine("newName = {0}", newName);
            string newPath = MakePath(parentPath, newName);
            // Make sure we aren't going to overwrite an existing item
            bool exists = ItemExists(newPath);
                Exception e = new ArgumentException(RegistryProviderStrings.RenameItemAlreadyExists);
                    newPath));
            // Confirm the rename item with the user
            string action = RegistryProviderStrings.RenameItemAction;
            string resourceTemplate = RegistryProviderStrings.RenameItemResourceTemplate;
                    newPath);
                // Implement rename as a move operation
                MoveRegistryItem(path, newPath);
        /// Creates a new registry key or value at the specified <paramref name="path"/>.
        /// The path to the new key to create.
        /// The type is ignored because this provider only creates
        /// registry keys.
        /// <param name="newItem">
        /// The newItem is ignored because the provider creates the
        /// key based on the path.
            object newItem)
            // Confirm the new item with the user
            string action = RegistryProviderStrings.NewItemAction;
            string resourceTemplate = RegistryProviderStrings.NewItemResourceTemplate;
                // Check to see if the key already exists
                IRegistryWrapper resultKey = GetRegkeyForPath(path, false);
                        Exception e = new System.IO.IOException(RegistryProviderStrings.KeyAlreadyExists);
                            resultKey));
                        resultKey.Close();
                        // Remove the existing key before creating the new one
                        RemoveItem(path, false);
                    if (!CreateIntermediateKeys(path))
                        // We are unable to create Intermediate keys. Just return.
                // Get the parent and child portions of the path
                IRegistryWrapper key = GetRegkeyForPathWriteIfError(parentPath, true);
                    // Create the new subkey
                    IRegistryWrapper newKey = key.CreateSubKey(childName);
                        // Set the default key value if the value and type were specified
                        if (newItem != null)
                            RegistryValueKind kind;
                            if (!ParseKind(type, out kind))
                            SetRegistryValue(newKey, string.Empty, newItem, kind, path, false);
                        // The key has been created, but the default value failed to be set.
                        // If possible, just write an error instead of failing the entire operation.
                        if ((exception is ArgumentException) ||
                            (exception is InvalidCastException) ||
                            (exception is System.IO.IOException) ||
                            (exception is System.UnauthorizedAccessException) ||
                            (exception is NotSupportedException))
                            ErrorRecord rec = new ErrorRecord(
                                newKey);
                            rec.ErrorDetails = new ErrorDetails(StringUtil.Format(RegistryProviderStrings.KeyCreatedValueFailed, childName));
                            WriteError(rec);
                    // Write the new key out.
                    WriteRegistryItemObject(newKey, path);
                    WriteError(new ErrorRecord(argException, argException.GetType().FullName, ErrorCategory.InvalidArgument, path));
                    WriteError(new ErrorRecord(notSupportedException, notSupportedException.GetType().FullName, ErrorCategory.InvalidOperation, path));
        /// Removes the specified registry key and all sub-keys.
        /// The path to the key to remove.
        /// Ignored. All removes are recursive because the
        /// registry provider does not support filters.
            s_tracer.WriteLine("recurse = {0}", recurse);
            // Get the parent key
            // Confirm the remove item with the user
            string action = RegistryProviderStrings.RemoveKeyAction;
            string resourceTemplate = RegistryProviderStrings.RemoveKeyResourceTemplate;
                    key.DeleteSubKeyTree(childName);
                    WriteError(new ErrorRecord(argumentException, argumentException.GetType().FullName, ErrorCategory.WriteError, path));
        /// Determines if the key at the specified path exists.
        /// The path to the key to determine if it exists.
        /// True if the key at the specified path exists, false otherwise.
                    // an empty path, \ or / are valid because
                    // we will enumerate all the hives
                    IRegistryWrapper key = GetRegkeyForPath(path, false);
            // Catch known non-terminating exceptions
            // In these cases, the item does exist
        /// Determines if the specified key has subkeys.
        /// The path to the key to determine if it has sub keys.
        /// True if the specified key has subkeys, false otherwise.
                    // An empty path will enumerate the hives
                    result = s_hiveNames.Length > 0;
                        result = key.SubKeyCount > 0;
        /// Copies the specified registry key to the specified <paramref name="path"/>.
        /// The path of the registry key to copy.
        /// The path to copy the key to.
        /// If true all subkeys should be copied. If false, only the
        /// specified key should be copied.
                CopyRegistryKey(key, path, destination, recurse, true, false);
        private bool CopyRegistryKey(
            IRegistryWrapper key,
            bool streamResult,
            bool streamFirstOnly)
            // Make sure we are not trying to do a recursive copy of a key
            // to itself or a child of itself.
                if (ErrorIfDestinationIsSourceOrChildOfSource(path, destination))
                key != null,
                "The key should have been validated by the caller");
                "The path should have been validated by the caller");
                "The destination should have been validated by the caller");
            // Get the parent key of the destination
            // If the destination already exists and is a key, then it becomes
            // the container of the source. If the key doesn't already exist
            // the parent of the destination path becomes the container of source.
            IRegistryWrapper newParentKey = GetRegkeyForPath(destination, true);
            string destinationName = GetChildName(path);
            string destinationParent = destination;
            if (newParentKey == null)
                destinationParent = GetParentPath(destination, null);
                destinationName = GetChildName(destination);
                newParentKey = GetRegkeyForPathWriteIfError(destinationParent, true);
                // The key was not found.
                // An error should have been written by GetRegkeyForPathWriteIfError
            string destinationPath = MakePath(destinationParent, destinationName);
            // Confirm the copy item with the user
            string action = RegistryProviderStrings.CopyKeyAction;
            string resourceTemplate = RegistryProviderStrings.CopyKeyResourceTemplate;
                // Create new key under the parent
                IRegistryWrapper newKey = null;
                    newKey = newParentKey.CreateSubKey(destinationName);
                    WriteError(new ErrorRecord(e, e.GetType().FullName, ErrorCategory.InvalidOperation, destinationName));
                if (newKey != null)
                    // Now copy all the properties from the source to the destination
                    string[] valueNames = key.GetValueNames();
                            newParentKey.Close();
                            newKey.Close();
                        newKey.SetValue(
                            valueNames[index],
                            key.GetValue(valueNames[index], null, RegistryValueOptions.DoNotExpandEnvironmentNames),
                            key.GetValueKind(valueNames[index]));
                    if (streamResult)
                        // Write out the key that was copied
                        WriteRegistryItemObject(newKey, destinationPath);
                        if (streamFirstOnly)
                            streamResult = false;
                // Copy all the subkeys
                string[] subkeyNames = key.GetSubKeyNames();
                for (int keyIndex = 0; keyIndex < subkeyNames.Length; ++keyIndex)
                    // Make the new path under the copy path.
                    string subKeyPath = MakePath(path, subkeyNames[keyIndex]);
                    string newSubKeyPath = MakePath(destinationPath, subkeyNames[keyIndex]);
                    IRegistryWrapper childKey = GetRegkeyForPath(subKeyPath, false);
                    bool subtreeResult = CopyRegistryKey(childKey, subKeyPath, newSubKeyPath, recurse, streamResult, streamFirstOnly);
                    childKey.Close();
                    if (!subtreeResult)
                        result = subtreeResult;
        private bool ErrorIfDestinationIsSourceOrChildOfSource(
            string destinationPath)
            s_tracer.WriteLine("destinationPath = {0}", destinationPath);
            // Note the paths have already been normalized so case-insensitive
            // comparisons should be sufficient
                // See if the paths are equal
                string newDestinationPath = GetParentPath(destinationPath, null);
                if (string.IsNullOrEmpty(newDestinationPath))
                    // We reached the root so the destination must not be a child
                    // of the source
                        newDestinationPath,
                destinationPath = newDestinationPath;
                        RegistryProviderStrings.DestinationChildOfSource);
        #endregion ContainerCmdletProvider overrides
        #region NavigationCmdletProvider overrides
        /// Determines if the key at the specified <paramref name="path"/> is a container.
        /// The path to a key.
        /// Since all registry keys are containers this method just checks
        /// to see if the key exists and returns true if it is does or
                        // All registry keys can be containers. Values are considered
                        // properties
                // Catch known exceptions that are not terminating
                    WriteError(new ErrorRecord(unauthorizedAccess, unauthorizedAccess.GetType().FullName, ErrorCategory.PermissionDenied, path));
        /// Moves the specified key.
        /// The path of the key to move.
        /// The path to move the key to.
            string action = RegistryProviderStrings.MoveItemAction;
            string resourceTemplate = RegistryProviderStrings.MoveItemResourceTemplate;
                MoveRegistryItem(path, destination);
        private void MoveRegistryItem(string path, string destination)
            // Implement move by copying the item and then removing it.
            // The copy will write the item to the pipeline
            bool continueWithRemove = false;
                continueWithRemove = CopyRegistryKey(key, path, destination, true, true, true);
            string sourceParent = GetParentPath(path, null);
            // If the destination is the same container as the source container don't do remove
            // the source item because the source and destination are the same.
            if (string.Equals(sourceParent, destination, StringComparison.OrdinalIgnoreCase))
                continueWithRemove = false;
            if (continueWithRemove)
                    RemoveItem(path, true);
        #endregion NavigationCmdletProvider overrides
        /// Gets the properties of the item specified by the <paramref name="path"/>.
        /// Nothing. An instance of PSObject representing the properties that were retrieved
        /// should be passed to the WriteObject() method.
        public void GetProperty(
            if (!CheckOperationNotAllowedOnHiveContainer(path))
            // get a set of matching properties on the key itself
            IRegistryWrapper key;
            Collection<string> filteredPropertyCollection;
            GetFilteredRegistryKeyProperties(path,
                                            out key,
                                            out filteredPropertyCollection);
            bool valueAdded = false;
            PSObject propertyResults = new PSObject();
            foreach (string valueName in filteredPropertyCollection)
                string notePropertyName = valueName;
                if (string.IsNullOrEmpty(valueName))
                    // If the value name is empty then using "(default)"
                    // as the property name when adding the note, as
                    // PSObject does not allow an empty propertyName
                    notePropertyName = LocalizedDefaultToken;
                    propertyResults.Properties.Add(new PSNoteProperty(notePropertyName, key.GetValue(valueName)));
                    valueAdded = true;
                catch (InvalidCastException invalidCast)
                        invalidCast.GetType().FullName,
            if (valueAdded)
                WritePropertyObject(propertyResults, path);
        /// Sets the specified properties of the item at the specified <paramref name="path"/>.
        /// Nothing. An instance of PSObject representing the properties that were set
        public void SetProperty(
                throw PSTraceSource.NewArgumentNullException(nameof(propertyValue));
            RegistryValueKind kind = RegistryValueKind.Unknown;
            // Get the kind of the value using the dynamic parameters
                    kind = dynParams.Type;
            string action = RegistryProviderStrings.SetPropertyAction;
            string resourceTemplate = RegistryProviderStrings.SetPropertyResourceTemplate;
            foreach (PSMemberInfo property in propertyValue.Properties)
                object newPropertyValue = property.Value;
                        SetRegistryValue(key, property.Name, newPropertyValue, kind, path);
                        WriteError(new ErrorRecord(invalidCast, invalidCast.GetType().FullName, ErrorCategory.WriteError, path));
                        // An exception occurred while trying to set the value. Write
                        WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.WriteError, property.Name));
                        WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, property.Name));
                        WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, property.Name));
        /// Gives the provider a chance to attach additional parameters to the
        /// Clears a property of the item at the specified <paramref name="path"/>.
                                            propertyToClear,
            string action = RegistryProviderStrings.ClearPropertyAction;
            string resourceTemplate = RegistryProviderStrings.ClearPropertyResourceTemplate;
            bool addedOnce = false;
                    valueName);
                    // reset the value of the property to its default value
                    object defaultValue = ResetRegistryKeyValue(key, valueName);
                    string propertyNameToAdd = valueName;
                        propertyNameToAdd = LocalizedDefaultToken;
                    result.Properties.Add(new PSNoteProperty(propertyNameToAdd, defaultValue));
                    addedOnce = true;
            if (addedOnce)
        #region Unimplemented methods
        #endregion Unimplemented methods
        /// Nothing. A PSObject representing the property that was created should
        /// be passed to the WriteObject() method.
        public void NewProperty(
            string action = RegistryProviderStrings.NewPropertyAction;
            string resourceTemplate = RegistryProviderStrings.NewPropertyResourceTemplate;
                // convert the type to a RegistryValueKind
                    // Check to see if the property already exists
                    // or overwrite if frce is on
                    if (Force || key.GetValue(propertyName) == null)
                        // Create the value
                        SetRegistryValue(key, propertyName, value, kind, path);
                        // The property already exists
                        System.IO.IOException e =
                            new System.IO.IOException(
                                RegistryProviderStrings.PropertyAlreadyExists);
                        WriteError(new ErrorRecord(e, e.GetType().FullName, ErrorCategory.ResourceExists, path));
        public void RemoveProperty(
            string propertyName)
            WildcardPattern propertyNamePattern =
                WildcardPattern.Get(propertyName, WildcardOptions.IgnoreCase);
            bool hadAMatch = false;
            foreach (string valueName in key.GetValueNames())
                    ((!Context.SuppressWildcardExpansion) && (!propertyNamePattern.IsMatch(valueName))) ||
                    (Context.SuppressWildcardExpansion && (!string.Equals(valueName, propertyName, StringComparison.OrdinalIgnoreCase))))
                hadAMatch = true;
                string action = RegistryProviderStrings.RemovePropertyAction;
                string resourceTemplate = RegistryProviderStrings.RemovePropertyResourceTemplate;
                    string propertyNameToRemove = GetPropertyName(valueName);
                        // Remove the value
                        key.DeleteValue(propertyNameToRemove);
                        WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.WriteError, propertyNameToRemove));
                        WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, propertyNameToRemove));
                        WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, propertyNameToRemove));
            WriteErrorIfPerfectMatchNotFound(hadAMatch, path, propertyName);
        /// Renames a property of the item at the specified <paramref name="path"/>.
        /// Nothing. A PSObject that represents the property that was renamed should be
        /// passed to the WriteObject() method.
        public void RenameProperty(
            string action = RegistryProviderStrings.RenamePropertyAction;
            string resourceTemplate = RegistryProviderStrings.RenamePropertyResourceTemplate;
                    destinationProperty);
                    MoveProperty(key, key, sourceProperty, destinationProperty);
        /// Copies a property of the item at the specified <paramref name="path"/> to a new property on the
        /// destination <paramref name="path"/>.
        /// Nothing. A PSObject that represents the property that was copied should be
        public void CopyProperty(
                throw PSTraceSource.NewArgumentNullException(nameof(sourcePath));
            if (!CheckOperationNotAllowedOnHiveContainer(sourcePath, destinationPath))
            IRegistryWrapper key = GetRegkeyForPathWriteIfError(sourcePath, false);
            IRegistryWrapper destinationKey = GetRegkeyForPathWriteIfError(destinationPath, true);
            if (destinationKey == null)
            string action = RegistryProviderStrings.CopyPropertyAction;
            string resourceTemplate = RegistryProviderStrings.CopyPropertyResourceTemplate;
                    CopyProperty(key, destinationKey, sourceProperty, destinationProperty, true);
                    WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.WriteError, sourcePath));
                    WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, sourcePath));
                    WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, sourcePath));
        /// Moves a property on an item specified by <paramref name="sourcePath"/>.
        /// Nothing. A PSObject that represents the property that was moved should be
        public void MoveProperty(
            IRegistryWrapper key = GetRegkeyForPathWriteIfError(sourcePath, true);
            string action = RegistryProviderStrings.MovePropertyAction;
            string resourceTemplate = RegistryProviderStrings.MovePropertyResourceTemplate;
                    MoveProperty(key, destinationKey, sourceProperty, destinationProperty);
            destinationKey.Close();
        /// Gets the parent path of the given <paramref name="path"/>.
        /// The path to get the parent of.
        /// The parent path of the given path.
        /// Since the base class implementation of GetParentPath of HKLM:\foo would return
        /// HKLM: we must add the \ back on.
            // If the main path existed, we must do a semantic analysis
            // to find the parent -- since path elements may contain
            // path delimiters. We only need to do this comparison
            // if the base implementation returns something in our namespace.
            if (!string.Equals(parentPath, root, StringComparison.OrdinalIgnoreCase))
                bool originalPathExists = ItemExists(path);
                bool originalPathExistsWithRoot = false;
                // This is an expensive test, only do it if we need to.
                if (!originalPathExists)
                    originalPathExistsWithRoot = ItemExists(MakePath(root, path));
                if ((!string.IsNullOrEmpty(parentPath)) && (originalPathExists || originalPathExistsWithRoot))
                    string parentPathToTest = parentPath;
                        parentPathToTest = parentPath;
                        if (originalPathExistsWithRoot)
                            parentPathToTest = MakePath(root, parentPath);
                        if (ItemExists(parentPathToTest))
                        parentPath = base.GetParentPath(parentPath, root);
                    } while (!string.IsNullOrEmpty(parentPath));
            return EnsureDriveIsRooted(parentPath);
        /// Gets the child name for the given <paramref name="path"/>.
        /// The path to get the leaf element of.
        /// The leaf element of the given path.
        /// Since the base class implementation of GetChildName will return
        /// normalized paths (with \), we must change them to forward slashes..
            string childName = base.GetChildName(path);
            return childName.Replace('\\', '/');
        public object NewPropertyDynamicParameters(
        public object RemovePropertyDynamicParameters(
        public object RenamePropertyDynamicParameters(
        public object CopyPropertyDynamicParameters(
        public object MovePropertyDynamicParameters(
            IRegistryWrapper sourceKey,
            IRegistryWrapper destinationKey,
            bool writeOnSuccess)
            string realSourceProperty = GetPropertyName(sourceProperty);
            string realDestinationProperty = GetPropertyName(destinationProperty);
            object sourceValue = sourceKey.GetValue(sourceProperty);
            RegistryValueKind sourceKind = sourceKey.GetValueKind(sourceProperty);
            destinationKey.SetValue(destinationProperty, sourceValue, sourceKind);
            if (writeOnSuccess)
                WriteWrappedPropertyObject(sourceValue, realSourceProperty, sourceKey.Name);
                // If sourceProperty and destinationProperty happens to be the same
                // then we shouldn't remove the property
                bool continueWithRemove = true;
                if (string.Equals(sourceKey.Name, destinationKey.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(realSourceProperty, realDestinationProperty, StringComparison.OrdinalIgnoreCase))
                // Move is implemented by copying the value and then deleting the original
                // Copy property will throw an exception if it fails
                CopyProperty(
                    sourceKey,
                    destinationKey,
                    realSourceProperty,
                    realDestinationProperty,
                // Delete sourceproperty only if it is not same as destination property
                    sourceKey.DeleteValue(realSourceProperty);
                object newValue = destinationKey.GetValue(realDestinationProperty);
                WriteWrappedPropertyObject(newValue, destinationProperty, destinationKey.Name);
                WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.WriteError, sourceKey.Name));
                WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, sourceKey.Name));
                WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, sourceKey.Name));
                result = path.Replace(StringLiterals.AlternatePathSeparator, StringLiterals.DefaultPathSeparator);
                // Remove relative path tokens
                if (HasRelativePathTokens(path))
                    result = NormalizeRelativePath(result, null);
                path.StartsWith('\\') ||
                path.Contains("\\.\\") ||
                path.Contains("\\..\\") ||
                path.EndsWith("\\..", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith("\\.", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("..\\", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(".\\", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith('~'));
        private void GetFilteredRegistryKeyProperties(string path,
                                                                    Collection<string> propertyNames,
                                                                    bool getAll,
                                                                    bool writeAccess,
                                                                    out IRegistryWrapper key,
                                                                    out Collection<string> filteredCollection)
            bool expandAll = false;
            filteredCollection = new Collection<string>();
            key = GetRegkeyForPathWriteIfError(path, writeAccess);
            // If properties were not specified, get all the values
            propertyNames ??= new Collection<string>();
            if (propertyNames.Count == 0 && getAll)
                propertyNames.Add("*");
                expandAll = true;
            foreach (string requestedValueName in propertyNames)
                WildcardPattern valueNameMatcher =
                        requestedValueName,
                foreach (string valueName in valueNames)
                    string valueNameToMatch = valueName;
                    // Need to convert the default value name to "(default)"
                        // Only do the conversion if the caller isn't asking for
                        // "" or null.
                        if (!string.IsNullOrEmpty(requestedValueName))
                            valueNameToMatch = LocalizedDefaultToken;
                        expandAll ||
                        ((!Context.SuppressWildcardExpansion) && (valueNameMatcher.IsMatch(valueNameToMatch))) ||
                       ((Context.SuppressWildcardExpansion) && (string.Equals(valueNameToMatch, requestedValueName, StringComparison.OrdinalIgnoreCase))))
                        if (string.IsNullOrEmpty(valueNameToMatch))
                        filteredCollection.Add(valueName);
                WriteErrorIfPerfectMatchNotFound(hadAMatch, path, requestedValueName);
        private void WriteErrorIfPerfectMatchNotFound(bool hadAMatch, string path, string requestedValueName)
            if (!hadAMatch && !WildcardPattern.ContainsWildcardCharacters(requestedValueName))
                // we did not have any match and the requested name did not have
                // any globbing characters (perfect match attempted)
                // we need to write an error
                string formatString = RegistryProviderStrings.PropertyNotAtPath;
                    new PSArgumentException(
                            path),
                        (Exception)null);
                    requestedValueName));
        /// IT resets the a registry key value to its default.
        /// <param name="key">Key whose value has to be reset.</param>
        /// <param name="valueName">Name of the value to reset.</param>
        /// <returns>Default value the key was set to.</returns>
        private object ResetRegistryKeyValue(IRegistryWrapper key, string valueName)
            RegistryValueKind valueKind = key.GetValueKind(valueName);
            switch (valueKind)
                // NOTICE: we assume that an unknown type is treated as
                // the same as a binary blob
                case RegistryValueKind.Binary:
                case RegistryValueKind.Unknown:
                        defaultValue = Array.Empty<byte>();
                case RegistryValueKind.DWord:
                        defaultValue = (int)0;
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.String:
                        defaultValue = string.Empty;
                case RegistryValueKind.MultiString:
                        defaultValue = Array.Empty<string>();
                case RegistryValueKind.QWord:
                        defaultValue = (long)0;
                key.SetValue(valueName, defaultValue, valueKind);
                WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.WriteError, valueName));
                WriteError(new ErrorRecord(securityException, securityException.GetType().FullName, ErrorCategory.PermissionDenied, valueName));
                WriteError(new ErrorRecord(unauthorizedAccessException, unauthorizedAccessException.GetType().FullName, ErrorCategory.PermissionDenied, valueName));
        /// Checks if the given path is the top container path (the one containing the hives)
        /// path to check
        /// true if the path is empty, a \ or a /, else false
        private static bool IsHiveContainer(string path)
            if (string.IsNullOrEmpty(path) ||
                string.Equals(path, "\\", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
        /// Checks the container. if the container is the hive container (Registry::\)
        /// it throws an exception.
        /// <param name="path">Path to check.</param>
        /// <returns>False if the operation is not allowed.</returns>
        private bool CheckOperationNotAllowedOnHiveContainer(string path)
                string message = RegistryProviderStrings.ContainerInvalidOperationTemplate;
                WriteError(new ErrorRecord(ex, "InvalidContainer", ErrorCategory.InvalidArgument, path));
        /// <param name="sourcePath">Source path to check.</param>
        /// <param name="destinationPath">Destination path to check.</param>
        private bool CheckOperationNotAllowedOnHiveContainer(string sourcePath, string destinationPath)
            if (IsHiveContainer(sourcePath))
                string message = RegistryProviderStrings.SourceContainerInvalidOperationTemplate;
                WriteError(new ErrorRecord(ex, "InvalidContainer", ErrorCategory.InvalidArgument, sourcePath));
            if (IsHiveContainer(destinationPath))
                RegistryProviderStrings.DestinationContainerInvalidOperationTemplate;
                WriteError(new ErrorRecord(ex, "InvalidContainer", ErrorCategory.InvalidArgument, destinationPath));
        /// Gets the appropriate hive root name for the specified path.
        /// The path to get the hive root name from.
        /// A registry key for the hive root specified by the path.
        private IRegistryWrapper GetHiveRoot(string path)
            if (TransactionAvailable())
                for (int k = 0; k < s_wellKnownHivesTx.Length; k++)
                    if (string.Equals(path, s_hiveNames[k], StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(path, s_hiveShortNames[k], StringComparison.OrdinalIgnoreCase))
                            return new TransactedRegistryWrapper(s_wellKnownHivesTx[k], this);
                for (int k = 0; k < s_wellKnownHives.Length; k++)
                        return new RegistryWrapper(s_wellKnownHives[k]);
        /// Creates the parent for the keypath specified by <paramref name="path"/>.
        /// <param name="path">RegistryKey path.</param>
        /// True if key is created or already exist,False otherwise.
        /// This method wont call ShouldProcess. Callers should do this before
        /// calling this method.
        private bool CreateIntermediateKeys(string path)
            // Check input.
                // 1. Normalize path ( for "//","." etc )
                // 2. Open the root
                // 3. Create subkey
                int index = path.IndexOf('\\');
                    // The user may precede a path with \
                    index = path.IndexOf('\\');
                    // we are at root..there is no subkey to create
                    // just return
                string keyRoot = path.Substring(0, index);
                // NormalizePath will trim "\" at the end. So there is always something
                // after index. Asserting just in case..
                Dbg.Diagnostics.Assert(index + 1 < path.Length, "Bad path");
                string remainingPath = path.Substring(index + 1);
                IRegistryWrapper rootKey = GetHiveRoot(keyRoot);
                if (remainingPath.Length == 0 || rootKey == null)
                // Create new subkey..and close
                IRegistryWrapper subKey = rootKey.CreateSubKey(remainingPath);
                    subKey.Close();
                    // SubKey is null
                    // Unable to create intermediate keys
                WriteError(new ErrorRecord(argumentException, argumentException.GetType().FullName, ErrorCategory.OpenError, path));
                WriteError(new ErrorRecord(ioException, ioException.GetType().FullName, ErrorCategory.OpenError, path));
        /// A private helper method that retrieves a RegistryKey for the specified
        /// path and if an exception is thrown retrieving the key, an error is written
        /// and null is returned.
        /// The path to the registry key to retrieve.
        /// <param name="writeAccess">
        /// If write access is required the key then this should be true. If false,
        /// the key will be opened with read access only.
        /// The RegistryKey associated with the specified path.
        private IRegistryWrapper GetRegkeyForPathWriteIfError(string path, bool writeAccess)
            IRegistryWrapper result = null;
                result = GetRegkeyForPath(path, writeAccess);
                    // The key was not found, write out an error.
                    ArgumentException exception =
                        RegistryProviderStrings.KeyDoesNotExist);
                    WriteError(new ErrorRecord(exception, exception.GetType().FullName, ErrorCategory.InvalidArgument, path));
        private IRegistryWrapper GetRegkeyForPath(string path, bool writeAccess)
            s_tracer.WriteLine("writeAccess = {0}", writeAccess);
                    // The user may proceed a path with \
                    result = GetHiveRoot(path);
                IRegistryWrapper resultRoot = GetHiveRoot(keyRoot);
                if (remainingPath.Length == 0 || resultRoot == null)
                    result = resultRoot;
                    result = resultRoot.OpenSubKey(remainingPath, writeAccess);
                    WriteError(new ErrorRecord(e, e.GetType().FullName, ErrorCategory.InvalidOperation, path));
                // If we could not open the key, see if we can find the subkey that matches.
                    IRegistryWrapper currentKey = resultRoot;
                    IRegistryWrapper tempKey = null;
                    // While there is still more to process
                    while (!string.IsNullOrEmpty(remainingPath))
                        bool foundSubkey = false;
                        foreach (string subKey in currentKey.GetSubKeyNames())
                            string normalizedSubkey = subKey;
                            // Check if the remaining path starts with the subkey name
                            if (!remainingPath.Equals(subKey, StringComparison.OrdinalIgnoreCase) &&
                                !remainingPath.StartsWith(subKey + StringLiterals.DefaultPathSeparator, StringComparison.OrdinalIgnoreCase))
                                // Actually normalize the subkey and then check again
                                normalizedSubkey = NormalizePath(subKey);
                                if (!remainingPath.Equals(normalizedSubkey, StringComparison.OrdinalIgnoreCase) &&
                                    !remainingPath.StartsWith(normalizedSubkey + StringLiterals.DefaultPathSeparator, StringComparison.OrdinalIgnoreCase))
                            tempKey = currentKey.OpenSubKey(subKey, writeAccess);
                            currentKey.Close();
                            currentKey = tempKey;
                            foundSubkey = true;
                            remainingPath = remainingPath.Equals(normalizedSubkey, StringComparison.OrdinalIgnoreCase)
                                                : remainingPath.Substring((normalizedSubkey + StringLiterals.DefaultPathSeparator).Length);
                        if (!foundSubkey)
                    return currentKey;
        // NB: The HKEY_DYN_DATA hive is left out of the following lists because
        // it is only available on Win98/ME and we do not support that platform.
        private static readonly string[] s_hiveNames = new string[] {
            "HKEY_CLASSES_ROOT",
            "HKEY_CURRENT_CONFIG",
            "HKEY_USERS",
            "HKEY_PERFORMANCE_DATA"
        private static readonly string[] s_hiveShortNames = new string[] {
            "HKCR",
            "HKCC",
            "HKU",
            "HKPD"
        private static readonly RegistryKey[] s_wellKnownHives = new RegistryKey[] {
            Registry.LocalMachine,
            Registry.CurrentUser,
            Registry.ClassesRoot,
            Registry.CurrentConfig,
            Registry.Users,
            Registry.PerformanceData
        private static readonly TransactedRegistryKey[] s_wellKnownHivesTx = new TransactedRegistryKey[] {
            TransactedRegistry.LocalMachine,
            TransactedRegistry.CurrentUser,
            TransactedRegistry.ClassesRoot,
            TransactedRegistry.CurrentConfig,
            TransactedRegistry.Users
        /// Sets or creates a registry value on a key.
        /// <param name="key">
        /// The key to set or create the value on.
        /// The name of the value to set or create.
        /// The new data for the value.
        /// The RegistryValueKind of the value.
        /// The path to the key that the value is being set on.
        private void SetRegistryValue(IRegistryWrapper key, string propertyName, object value, RegistryValueKind kind, string path)
            SetRegistryValue(key, propertyName, value, kind, path, true);
        /// <param name="writeResult">
        /// If true, the value that is set will be written out.
        private void SetRegistryValue(
            RegistryValueKind kind,
            bool writeResult)
                "Caller should have verified key");
            string propertyNameToSet = GetPropertyName(propertyName);
            RegistryValueKind existingKind = RegistryValueKind.Unknown;
            // If user does not specify a kind: get the valuekind if the property
            // already exists
            if (kind == RegistryValueKind.Unknown)
                existingKind = GetValueKindForProperty(key, propertyNameToSet);
            // try to do a conversion based on the existing kind, if we
            // were able to retrieve one
            if (existingKind != RegistryValueKind.Unknown)
                    value = ConvertValueToKind(value, existingKind);
                    kind = existingKind;
                    // failed attempt, we reset to unknown to let the
                    // default conversion process take over
                    existingKind = RegistryValueKind.Unknown;
            // set the kind as defined by the user
            if (existingKind == RegistryValueKind.Unknown)
                // we use to kind passed in, either because we had
                // a valid one or because we failed to retrieve an existing kind to match
                    // set the kind based on value
                        kind = GetValueKindFromObject(value);
                        // if no value and unknown kind, then default to empty string
                        kind = RegistryValueKind.String;
                value = ConvertValueToKind(value, kind);
            key.SetValue(propertyNameToSet, value, kind);
            if (writeResult)
                // Now write out the value
                object newValue = key.GetValue(propertyNameToSet);
                WriteWrappedPropertyObject(newValue, propertyName, path);
        /// Helper to wrap property values when sent to the pipeline into an PSObject;
        /// it adds the name of the property as a note.
        /// <param name="value">The property to be written.</param>
        /// <param name="propertyName">Name of the property being written.</param>
        /// <param name="path">The path of the item being written.</param>
        private void WriteWrappedPropertyObject(object value, string propertyName, string path)
            string propertyNameToAdd = propertyName;
            result.Properties.Add(new PSNoteProperty(propertyNameToAdd, value));
        /// Uses LanguagePrimitives.ConvertTo to convert the value to the type that is appropriate
        /// for the specified RegistryValueKind.
        /// The value to convert.
        /// The RegistryValueKind type to convert the value to.
        /// The converted value.
        private static object ConvertValueToKind(object value, RegistryValueKind kind)
                    value = (value != null)
                        ? (byte[])LanguagePrimitives.ConvertTo(
                            CultureInfo.CurrentCulture)
                        : Array.Empty<byte>();
                                value = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.CurrentCulture);
                                value = (UInt32)LanguagePrimitives.ConvertTo(value, typeof(UInt32), CultureInfo.CurrentCulture);
                        ? (string)LanguagePrimitives.ConvertTo(
                        ? (string[])LanguagePrimitives.ConvertTo(
                            typeof(string[]),
                                value = (long)LanguagePrimitives.ConvertTo(value, typeof(long), CultureInfo.CurrentCulture);
                                value = (UInt64)LanguagePrimitives.ConvertTo(value, typeof(UInt64), CultureInfo.CurrentCulture);
                    // If kind is Unknown then just leave the value as-is.
        /// Helper to infer the RegistryValueKind from an object.
        /// <param name="value">Object whose RegistryValueKind has to be determined.</param>
        /// <returns>Corresponding RegistryValueKind.</returns>
        private static RegistryValueKind GetValueKindFromObject(object value)
            RegistryValueKind result = RegistryValueKind.Unknown;
            if (valueType == typeof(byte[]))
                result = RegistryValueKind.Binary;
            else if (valueType == typeof(int))
                result = RegistryValueKind.DWord;
            if (valueType == typeof(string))
                result = RegistryValueKind.String;
            if (valueType == typeof(string[]))
                result = RegistryValueKind.MultiString;
            if (valueType == typeof(long))
                result = RegistryValueKind.QWord;
        /// Helper to get RegistryValueKind for a Property.
        /// <param name="key">RegistryKey containing property.</param>
        /// <param name="valueName">Property for which RegistryValueKind is requested.</param>
        /// <returns>RegistryValueKind of the property. If the property does not exit,returns RegistryValueKind.Unknown.</returns>
        private static RegistryValueKind GetValueKindForProperty(IRegistryWrapper key, string valueName)
                return key.GetValueKind(valueName);
                // RegistryKey that contains the specified value does not exist
            return RegistryValueKind.Unknown;
        /// Helper to read back an existing registry key value.
        /// <param name="key">Key to read the value from.</param>
        /// <param name="valueName">Name of the value to read.</param>
        /// <returns>Value of the key, null if it could not retrieve
        /// it because known exceptions were thrown, else an exception is percolated up
        private static object ReadExistingKeyValue(IRegistryWrapper key, string valueName)
                return key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        /// Wraps a registry item in a PSObject and sets the TreatAs to
        /// Microsoft.Win32.RegistryKey. This way values will be presented
        /// in the same format as keys.
        /// The registry key to be written out.
        /// The path to the item being written out.
        private void WriteRegistryItemObject(
                    "The RegistryProvider should never attempt to write out a null value");
                // Don't error, but don't write out anything either.
            // Escape any wildcard characters in the path
            path = EscapeSpecialChars(path);
            // Wrap the key in an PSObject
            PSObject outputObject = PSObject.AsPSObject(key.RegistryKey);
            // Add the registry values to the PSObject
                if (string.IsNullOrEmpty(valueNames[index]))
                    // The first unnamed value becomes the default value
                    valueNames[index] = LocalizedDefaultToken;
            outputObject.AddOrSetProperty("Property", valueNames);
            WriteItemObject(outputObject, path, true);
        /// Takes a string and tries to parse it into a RegistryValueKind enum
        /// type.
        /// If the conversion fails, WriteError() is called.
        /// The type as specified by the user that should be parsed into a RegistryValueKind enum.
        /// <param name="kind">Output for the RegistryValueKind for the string.</param>
        /// true if the conversion succeeded
        private bool ParseKind(string type, out RegistryValueKind kind)
            kind = RegistryValueKind.Unknown;
                kind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), type, true);
                innerException = invalidCast;
                innerException = argException;
            if (innerException != null)
                string formatString =
                    RegistryProviderStrings.TypeParameterBindingFailure;
                            typeof(RegistryValueKind).FullName),
                        innerException);
                    type));
        /// Gets the default value name token from the resource.
        /// In English that token is "(default)" without the quotes.
        /// This should not be localized as it will break scripts.
        /// A string containing the default value name.
        private static string LocalizedDefaultToken => "(default)";
        /// Converts an empty or null userEnteredPropertyName to the localized
        /// string for the default property name.
        /// <param name="userEnteredPropertyName">
        /// The property name to convert.
        /// If userEnteredPropertyName is null or empty, the localized default
        /// property name is returned, else the userEnteredPropertyName is returned.
        private string GetPropertyName(string userEnteredPropertyName)
            string result = userEnteredPropertyName;
            if (!string.IsNullOrEmpty(userEnteredPropertyName))
                var stringComparer = Host.CurrentCulture.CompareInfo;
                if (stringComparer.Compare(
                        userEnteredPropertyName,
                        LocalizedDefaultToken,
                        CompareOptions.IgnoreCase) == 0)
    /// Defines dynamic parameters for the registry provider.
    public class RegistryProviderSetItemDynamicParameter
        /// Gets or sets the Type parameter as a dynamic parameter for
        /// the registry provider's SetItem method.
        /// The only acceptable values for this parameter are those found
        /// in the RegistryValueKind enum
        public RegistryValueKind Type { get; set; } = RegistryValueKind.Unknown;
