    /// An interface that can be implemented on a Cmdlet provider to expose an item's
    /// content.
    /// An IContentCmdletProvider provider implements a set of methods that allows
    /// the use of a set of core commands against the data store that the provider
    /// gives access to. By implementing this interface users can take advantage
    /// the commands that expose the contents of an item.
    ///     get-content
    ///     set-content
    ///     clear-content
    /// This interface should only be implemented on derived classes of
    /// <see cref="CmdletProvider"/>, <see cref="ItemCmdletProvider"/>,
    /// <see cref="ContainerCmdletProvider"/>, or <see cref="NavigationCmdletProvider"/>.
    /// A namespace provider should implemented this interface if items in the
    /// namespace have content the provide wishes to expose.
    public interface IContentCmdletProvider
        /// Overrides of this method should return an <see cref="System.Management.Automation.Provider.IContentReader"/>
        /// for the item specified by the path.
        /// By default overrides of this method should not return a content reader for objects
        /// that are generally hidden from
        /// the user unless the Force property is set to true. An error should be sent to the WriteError method if
        /// the path represents an item that is hidden from the user and Force is set to false.
        IContentReader? GetContentReader(string path);
        /// Gives the provider an opportunity to attach additional parameters to the
        /// get-content cmdlet.
        object? GetContentReaderDynamicParameters(string path);
        /// The path to the item to get the content writer for.
        /// An IContentWriter for the item at the specified path.
        /// Overrides of this method should return an <see cref="System.Management.Automation.Provider.IContentWriter"/>
        /// By default overrides of this method should not return a content writer for objects
        IContentWriter? GetContentWriter(string path);
        /// set-content and add-content cmdlet.
        object? GetContentWriterDynamicParameters(string path);
        /// Clears the content from the specified item.
        /// Overrides of this method should remove any content from the object but
        /// not remove (delete) the object itself.
        /// By default overrides of this method should not clear or write objects that are generally hidden from
        void ClearContent(string path);
        /// clear-content cmdlet.
        object? ClearContentDynamicParameters(string path);
