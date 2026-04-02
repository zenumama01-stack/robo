    /// Internal wrapper for third-party adapters (PSPropertyAdapter)
    internal class ThirdPartyAdapter : PropertyOnlyAdapter
        internal ThirdPartyAdapter(Type adaptedType, PSPropertyAdapter externalAdapter)
            AdaptedType = adaptedType;
            _externalAdapter = externalAdapter;
        /// The type this instance is adapting.
        internal Type AdaptedType { get; }
        /// The type of the external adapter.
        internal Type ExternalAdapterType
                return _externalAdapter.GetType();
            Collection<string> typeNameHierarchy = null;
                typeNameHierarchy = _externalAdapter.GetTypeNameHierarchy(obj);
                    "PSPropertyAdapter.GetTypeNameHierarchyError",
                    ExtendedTypeSystem.GetTypeNameHierarchyError, obj.ToString());
            if (typeNameHierarchy == null)
                    "PSPropertyAdapter.NullReturnValueError",
                    ExtendedTypeSystem.NullReturnValueError, "PSPropertyAdapter.GetTypeNameHierarchy");
            return typeNameHierarchy;
            Collection<PSAdaptedProperty> properties = null;
                properties = _externalAdapter.GetProperties(obj);
                    "PSPropertyAdapter.GetProperties",
                    ExtendedTypeSystem.GetProperties, obj.ToString());
                    ExtendedTypeSystem.NullReturnValueError, "PSPropertyAdapter.GetProperties");
            foreach (PSAdaptedProperty property in properties)
                InitializeProperty(property, obj);
                members.Add(property as T);
            PSAdaptedProperty property = null;
                property = _externalAdapter.GetProperty(obj, propertyName);
                    "PSPropertyAdapter.GetProperty",
                    ExtendedTypeSystem.GetProperty, propertyName, obj.ToString());
                property = _externalAdapter.GetFirstPropertyOrDefault(obj, predicate);
                    ExtendedTypeSystem.GetProperty, nameof(predicate), obj.ToString());
        /// Ensures that the adapter and base object are set in the given PSAdaptedProperty.
        private void InitializeProperty(PSAdaptedProperty property, object baseObject)
            if (property.adapter == null)
                property.adapter = this;
                property.baseObject = baseObject;
            PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
            Diagnostics.Assert(adaptedProperty != null, "ThirdPartyAdapter should only receive PSAdaptedProperties");
                return _externalAdapter.IsSettable(adaptedProperty);
                    "PSPropertyAdapter.PropertyIsSettableError",
                    ExtendedTypeSystem.PropertyIsSettableError, property.Name);
                return _externalAdapter.IsGettable(adaptedProperty);
                    "PSPropertyAdapter.PropertyIsGettableError",
                    ExtendedTypeSystem.PropertyIsGettableError, property.Name);
                return _externalAdapter.GetPropertyValue(adaptedProperty);
                    "PSPropertyAdapter.PropertyGetError",
                    ExtendedTypeSystem.PropertyGetError, property.Name);
                _externalAdapter.SetPropertyValue(adaptedProperty, setValue);
                    "PSPropertyAdapter.PropertySetError",
                    ExtendedTypeSystem.PropertySetError, property.Name);
            string propertyTypeName = null;
                propertyTypeName = _externalAdapter.GetPropertyTypeName(adaptedProperty);
                    "PSPropertyAdapter.PropertyTypeError",
                    ExtendedTypeSystem.PropertyTypeError, property.Name);
            return propertyTypeName ?? "System.Object";
        private readonly PSPropertyAdapter _externalAdapter;
    /// User-defined property adapter.
    /// This class is used to expose a simplified version of the type adapter API
    public abstract class PSPropertyAdapter
        /// Returns the type hierarchy for the given object.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        public virtual Collection<string> GetTypeNameHierarchy(object baseObject)
            ArgumentNullException.ThrowIfNull(baseObject);
            Collection<string> types = new Collection<string>();
                types.Add(type.FullName);
            return types;
        /// Returns a list of the adapted properties.
        public abstract Collection<PSAdaptedProperty> GetProperties(object baseObject);
        /// Returns a specific property, or null if the base object does not contain the given property.
        public abstract PSAdaptedProperty GetProperty(object baseObject, string propertyName);
        /// Returns true if the given property is settable.
        public abstract bool IsSettable(PSAdaptedProperty adaptedProperty);
        /// Returns true if the given property is gettable.
        public abstract bool IsGettable(PSAdaptedProperty adaptedProperty);
        /// Returns the value of a given property.
        public abstract object GetPropertyValue(PSAdaptedProperty adaptedProperty);
        /// Sets the value of a given property.
        public abstract void SetPropertyValue(PSAdaptedProperty adaptedProperty, object value);
        /// Returns the type for a given property.
        public abstract string GetPropertyTypeName(PSAdaptedProperty adaptedProperty);
        /// Returns a property if it's name matches the specified <see cref="MemberNamePredicate"/>, otherwise null.
        /// <returns>An adapted property if the predicate matches, or <see langword="null"/>.</returns>
        public virtual PSAdaptedProperty GetFirstPropertyOrDefault(object baseObject, MemberNamePredicate predicate)
            foreach (var property in GetProperties(baseObject))
