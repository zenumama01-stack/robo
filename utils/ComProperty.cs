    /// Defines a property in the COM object.
    internal class ComProperty
        private bool _hasSetter = false;
        private bool _hasSetterByRef = false;
        private int _dispId;
        private int _setterIndex;
        private int _setterByRefIndex;
        private int _getterIndex;
        /// Initializes a new instance of ComProperty.
        /// <param name="typeinfo">Reference to the ITypeInfo of the COM object.</param>
        /// <param name="name">Name of the property being created.</param>
        internal ComProperty(COM.ITypeInfo typeinfo, string name)
        /// Defines the name of the property.
        private Type _cachedType;
        /// Defines the type of the property.
        internal Type Type
                _cachedType = null;
                if (_cachedType == null)
                    IntPtr pFuncDesc = IntPtr.Zero;
                        _typeInfo.GetFuncDesc(GetFuncDescIndex(), out pFuncDesc);
                        if (IsGettable)
                            // use the return type of the getter
                            _cachedType = ComUtil.GetTypeFromTypeDesc(funcdesc.elemdescFunc.tdesc);
                            // use the type of the first argument to the setter
                            ParameterInformation[] parameterInformation = ComUtil.GetParameterInformation(funcdesc, false);
                            Diagnostics.Assert(parameterInformation.Length == 1, "Invalid number of parameters in a property setter");
                            _cachedType = parameterInformation[0].parameterType;
                        if (pFuncDesc != IntPtr.Zero)
                return _cachedType;
        /// Retrieves the index of the FUNCDESC for the current property.
        private int GetFuncDescIndex()
                return _getterIndex;
            else if (_hasSetter)
                return _setterIndex;
                Diagnostics.Assert(_hasSetterByRef, "Invalid property setter type");
                return _setterByRefIndex;
        /// Defines whether the property has parameters or not.
        internal bool IsParameterized { get; private set; } = false;
        /// Returns the number of parameters in this property.
        /// This is applicable only for parameterized properties.
        internal int ParamCount
        /// Defines whether this property is settable.
        internal bool IsSettable
                return _hasSetter || _hasSetterByRef;
        /// Defines whether this property is gettable.
        internal bool IsGettable { get; private set; } = false;
        /// Get value of this property.
        /// <param name="target">Instance of the object from which to get the property value.</param>
        /// <returns>Value of the property.</returns>
        internal object GetValue(object target)
                return ComInvoker.Invoke(target as IDispatch, _dispId, null, null, COM.INVOKEKIND.INVOKE_PROPERTYGET);
        /// <param name="arguments">Parameters to get the property value.</param>
        /// <returns>Value of the property</returns>
        internal object GetValue(object target, object[] arguments)
                var getterCollection = new Collection<int> { _getterIndex };
                var methods = ComUtil.GetMethodInformationArray(_typeInfo, getterCollection, false);
                object returnValue = ComInvoker.Invoke(target as IDispatch,
                                                       bestMethod.DispId,
                                                       newarguments,
                                                       bestMethod.InvokeKind);
        /// Sets value of this property.
        /// <param name="target">Instance of the object to which to set the property value.</param>
        /// <param name="setValue">Value to set this property.</param>
        internal void SetValue(object target, object setValue)
            object[] propValue = new object[1];
            setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, this.Type, CultureInfo.InvariantCulture);
            propValue[0] = setValue;
                ComInvoker.Invoke(target as IDispatch, _dispId, propValue, null, COM.INVOKEKIND.INVOKE_PROPERTYPUT);
        /// Sets the value of the property.
        /// <param name="arguments">Parameters to set this property.</param>
        internal void SetValue(object target, object setValue, object[] arguments)
            var setterCollection = new Collection<int> { _hasSetterByRef ? _setterByRefIndex : _setterIndex };
            var methods = ComUtil.GetMethodInformationArray(_typeInfo, setterCollection, true);
            var finalArguments = new object[newarguments.Length + 1];
            for (int i = 0; i < newarguments.Length; i++)
                finalArguments[i] = newarguments[i];
            finalArguments[newarguments.Length] = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, Type, CultureInfo.InvariantCulture);
                ComInvoker.Invoke(target as IDispatch,
                                  finalArguments,
                                                           finalArguments.Length,
                                                           isPropertySet: true),
                Adapter.SetReferences(finalArguments, bestMethod, arguments);
        /// Updates the COM property with setter and getter information.
        /// <param name="desc">Functional descriptor for property getter or setter.</param>
        /// <param name="index">Index of function descriptor in type information.</param>
        internal void UpdateFuncDesc(COM.FUNCDESC desc, int index)
            _dispId = desc.memid;
            switch (desc.invkind)
                case COM.INVOKEKIND.INVOKE_PROPERTYGET:
                    IsGettable = true;
                    _getterIndex = index;
                    if (desc.cParams > 0)
                        IsParameterized = true;
                case COM.INVOKEKIND.INVOKE_PROPERTYPUT:
                    _hasSetter = true;
                    _setterIndex = index;
                    if (desc.cParams > 1)
                case COM.INVOKEKIND.INVOKE_PROPERTYPUTREF:
                    _setterByRefIndex = index;
                    _hasSetterByRef = true;
        internal string GetDefinition()
                return ComUtil.GetMethodSignatureFromFuncDesc(_typeInfo, funcdesc, !IsGettable);
        /// Returns the property signature string.
        /// <returns>Property signature.</returns>
            builder.Append(this.GetDefinition());
                builder.Append("{get} ");
            if (_hasSetter)
                builder.Append("{set} ");
            if (_hasSetterByRef)
                builder.Append("{set by ref}");
