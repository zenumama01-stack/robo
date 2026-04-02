    /// The base class for Cmdlet providers that expose an item as a PowerShell path.
    /// The ItemCmdletProvider class is a base class that a provider derives from to
    /// inherit a set of methods that allows the PowerShell engine
    /// to provide a core set of commands for getting and setting of data on one or
    /// more items. A provider should derive from this class if they want
    /// to take advantage of the item core commands that are
    /// already implemented by the engine. This allows users to have common
    /// commands and semantics across multiple providers.
    public abstract class ItemCmdletProvider : DriveCmdletProvider
        /// Internal wrapper for the GetItem protected method. It is called instead
        /// The path to the item to retrieve.
        /// Nothing is returned, but all objects should be written to the WriteObject method.
        internal void GetItem(string path, CmdletProviderContext context)
            GetItem(path);
        /// the get-item cmdlet.
            return GetItemDynamicParameters(path);
        /// Internal wrapper for the SetItem protected method. It is called instead
        /// The path to the item to set.
        /// The value of the item specified by the path.
        /// The item that was set at the specified path.
            providerBaseTracer.WriteLine("ItemCmdletProvider.SetItem");
            SetItem(path, value);
        /// the set-item cmdlet.
            return SetItemDynamicParameters(path, value);
        /// Internal wrapper for the ClearItem protected method. It is called instead
        /// The path to the item to clear.
            providerBaseTracer.WriteLine("ItemCmdletProvider.ClearItem");
            ClearItem(path);
        /// the clear-item cmdlet.
        internal object ClearItemDynamicParameters(
            return ClearItemDynamicParameters(path);
        /// Internal wrapper for the InvokeDefaultAction protected method. It is called instead
        /// The path to the item to perform the default action on.
            providerBaseTracer.WriteLine("ItemCmdletProvider.InvokeDefaultAction");
            InvokeDefaultAction(path);
        /// the invoke-item cmdlet.
        internal object InvokeDefaultActionDynamicParameters(
            return InvokeDefaultActionDynamicParameters(path);
        /// Internal wrapper for the Exists protected method. It is called instead
        /// The path to the item to see if it exists.
        /// True if the item exists, false otherwise.
        internal bool ItemExists(string path, CmdletProviderContext context)
            bool itemExists = false;
                // Some providers don't expect non-valid path elements, and instead
                // throw an exception here.
                itemExists = ItemExists(path);
            return itemExists;
        /// the test-path cmdlet.
            return ItemExistsDynamicParameters(path);
        /// Internal wrapper for the IsValidPath protected method. It is called instead
        /// The path to check for validity.
        /// True if the path is syntactically and semantically valid for the provider, or
        /// This test should not verify the existence of the item at the path. It should
        /// only perform syntactic and semantic validation of the path.  For instance, for
        /// the file system provider, that path should be canonicalized, syntactically verified,
        /// and ensure that the path does not refer to a device.
        internal bool IsValidPath(string path, CmdletProviderContext context)
            return IsValidPath(path);
        /// Internal wrapper for the ExpandPath protected method. It is called instead
        /// context of the command can be set. Only called for providers that declare
        /// the ExpandWildcards capability.
        /// The path to expand. Expansion must be consistent with the wildcarding
        /// rules of PowerShell's WildcardPattern class.
        /// A list of provider paths that this path expands to. They must all exist.
        internal string[] ExpandPath(string path, CmdletProviderContext context)
            return ExpandPath(path);
        /// the get-item and get-childitem cmdlets.
        protected virtual void GetItem(string path)
        protected virtual object GetItemDynamicParameters(string path)
        /// Sets the item specified by the path.
        /// Nothing.  The item that was set should be passed to the WriteItemObject method.
        /// Providers override this method to give the user the ability to modify provider objects using
        /// By default overrides of this method should not set or write objects that are generally hidden from
        protected virtual void SetItem(
        protected virtual object SetItemDynamicParameters(string path, object value)
        /// Clears the item specified by the path.
        /// Nothing.  The item that was cleared should be passed to the WriteItemObject method.
        /// Providers override this method to give the user the ability to clear provider objects using
        protected virtual void ClearItem(
        protected virtual object ClearItemDynamicParameters(string path)
        /// Invokes the default action on the specified item.
        /// The default implementation does nothing.
        /// Providers override this method to give the user the ability to invoke provider objects using
        /// the invoke-item cmdlet. Think of the invocation as a double click in the Windows Shell. This
        /// method provides a default action based on the path that was passed.
        /// By default overrides of this method should not invoke objects that are generally hidden from
        protected virtual void InvokeDefaultAction(
        protected virtual object InvokeDefaultActionDynamicParameters(string path)
        /// Determines if an item exists at the specified path.
        /// Providers override this method to give the user the ability to check for the existence of provider objects using
        /// The implementation of this method should take into account any form of access to the object that may
        /// make it visible to the user.  For instance, if a user has write access to a file in the file system
        /// provider bug not read access, the file still exists and the method should return true.  Sometimes this
        /// may require checking the parent to see if the child can be enumerated.
        protected virtual bool ItemExists(string path)
        protected virtual object ItemExistsDynamicParameters(string path)
        /// Providers must override this method to verify the syntax and semantics
        /// of their paths.
        protected abstract bool IsValidPath(string path);
        /// Expand a provider path that contains wildcards to a list of provider
        /// paths that the path represents.Only called for providers that declare
        protected virtual string[] ExpandPath(string path)
                return new string[] { path };
        #endregion Protected methods
    #endregion ItemCmdletProvider
