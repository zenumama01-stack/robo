    /// An interface that can be implemented by a Cmdlet provider to expose properties of an item.
    /// An IPropertyCmdletProvider provider implements a set of methods that allows
    ///     get-itemproperty
    ///     set-itemproperty
    /// namespace have properties the provide wishes to expose.
    public interface IPropertyCmdletProvider
        /// Gets the properties of the item specified by the path.
        /// The path to the item to retrieve properties from.
        /// Nothing.  The property that was retrieved should be passed to the WritePropertyObject method.
        /// using the get-itemproperty cmdlet.
        /// By default overrides of this method should not retrieve properties from objects that are generally hidden from
        /// An <see cref="System.Management.Automation.PSObject"/> can be used as a property bag for the
        /// properties that need to be returned if the <paramref name="providerSpecificPickList"/> contains
        /// multiple properties to write.
        void GetProperty(
            Collection<string>? providerSpecificPickList);
        /// get-itemproperty cmdlet.
        object? GetPropertyDynamicParameters(
        /// Sets the specified properties of the item at the specified path.
        /// Nothing.  The property that was set should be passed to the WritePropertyObject method.
        /// Providers override this method to give the user the ability to set the value of provider object properties
        /// using the set-itemproperty cmdlet.
        /// <paramref name="propertyValue"/> is a property bag containing the properties that should be set.
        /// See <see cref="System.Management.Automation.PSObject"/> for more information.
        void SetProperty(
            PSObject propertyValue);
        object? SetPropertyDynamicParameters(
        /// Clears a property of the item at the specified path.
        /// The path to the item on which to clear the property.
        /// Nothing.  The property that was cleared should be passed to the WritePropertyObject method.
        /// Providers override this method to give the user the ability to clear the value of provider object properties
        /// using the clear-itemproperty cmdlet.
        /// By default overrides of this method should not clear properties from objects that are generally hidden from
        void ClearProperty(
            Collection<string> propertyToClear);
        /// clear-itemproperty cmdlet.
        object? ClearPropertyDynamicParameters(
