    #region IDynamicPropertyCmdletProvider
    /// An interface that can be implemented on a Cmdlet provider to expose the dynamic
    /// manipulation of properties.
    /// An IDynamicPropertyCmdletProvider provider implements a set of methods that allows
    /// the commands that expose the creation and deletion of properties on an item.
    ///     rename-itemproperty
    ///     remove-itemproperty
    ///     new-itemproperty
    ///     etc.
    /// A Cmdlet provider should implemented this interface if items in the
    /// namespace have dynamic properties the provide wishes to expose.
    public interface IDynamicPropertyCmdletProvider : IPropertyCmdletProvider
        /// Nothing.  The new property that was created should be passed to the WritePropertyObject method.
        /// Providers override this method to give the user the ability to add properties to provider objects
        /// using the new-itemproperty cmdlet.
        /// By default overrides of this method should not create new properties on objects that are generally hidden from
        void NewProperty(
            object? value);
        /// new-itemproperty cmdlet.
        object? NewPropertyDynamicParameters(
        /// Removes a property on the item specified by the path.
        /// The path to the item on which the property should be removed.
        /// The name of the property to be removed.
        /// Providers override this method to give the user the ability to remove properties from provider objects
        /// using the remove-itemproperty cmdlet.
        /// By default overrides of this method should not remove properties on objects that are generally hidden from
        void RemoveProperty(
        /// remove-itemproperty cmdlet.
        object RemovePropertyDynamicParameters(
        /// Renames a property of the item at the specified path.
        /// The path to the item on which to rename the property.
        /// The property to rename.
        /// Nothing.  The new property that was renamed should be passed to the WritePropertyObject method.
        /// Providers override this method to give the user the ability to rename properties of provider objects
        /// using the rename-itemproperty cmdlet.
        /// By default overrides of this method should not rename properties on objects that are generally hidden from
        void RenameProperty(
            string destinationProperty);
        /// rename-itemproperty cmdlet.
        object? RenamePropertyDynamicParameters(
        /// Copies a property of the item at the specified path to a new property on the
        /// destination item.
        /// The path to the item on which to copy the property.
        /// The path to the item on which to copy the property to.
        /// The destination property to copy to.
        /// Nothing.  The new property that was copied to should be passed to the WritePropertyObject method.
        /// Providers override this method to give the user the ability to copy properties of provider objects
        /// using the copy-itemproperty cmdlet.
        /// By default overrides of this method should not copy properties from or to objects that are generally hidden from
        void CopyProperty(
        /// copy-itemproperty cmdlet.
        object? CopyPropertyDynamicParameters(
        /// Moves a property on an item specified by the path.
        /// The path to the item on which to move the property.
        /// The path to the item on which to move the property to.
        /// The destination property to move to.
        /// Providers override this method to give the user the ability to move properties from one provider object
        /// to another using the move-itemproperty cmdlet.
        /// By default overrides of this method should not move properties on or to objects that are generally hidden from
        void MoveProperty(
        /// move-itemproperty cmdlet.
        object? MovePropertyDynamicParameters(
    #endregion IDynamicPropertyCmdletProvider
