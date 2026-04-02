    /// This is used for automatic conversions to be performed in shell variables.
    internal sealed class ArgumentTypeConverterAttribute : ArgumentTransformationAttribute
        /// This ctor form is used to initialize shell variables
        /// whose type is not permitted to change.
        /// <param name="types"></param>
        internal ArgumentTypeConverterAttribute(params Type[] types)
            _convertTypes = types;
        private readonly Type[] _convertTypes;
        internal Type TargetType
                return _convertTypes?.LastOrDefault();
            return Transform(engineIntrinsics, inputData, false, false);
        internal object Transform(EngineIntrinsics engineIntrinsics, object inputData, bool bindingParameters, bool bindingScriptCmdlet)
            if (_convertTypes == null)
                for (int i = 0; i < _convertTypes.Length; i++)
                    if (bindingParameters)
                        // We should not be doing a conversion here if [ref] is the last type.
                        // When [ref] appears in an argument list, it is used for checking only.
                        // No Conversion should be done.
                        if (_convertTypes[i].Equals(typeof(System.Management.Automation.PSReference)))
                            object temp;
                            PSObject mshObject = result as PSObject;
                                temp = mshObject.BaseObject;
                                temp = result;
                            if (temp is not PSReference reference)
                                throw new PSInvalidCastException("InvalidCastExceptionReferenceTypeExpected", null,
                                                                   ExtendedTypeSystem.ReferenceTypeExpected);
                            // If a non-ref type is expected but currently passed in is a ref, do an implicit dereference.
                            PSReference reference = temp as PSReference;
                            if (reference != null)
                                result = reference.Value;
                            if (bindingScriptCmdlet && _convertTypes[i] == typeof(string))
                                // Don't allow conversion from array to string in script w/ cmdlet binding.  Allow
                                // the conversion for ordinary script parameter binding for V1 compatibility.
                                temp = PSObject.Base(result);
                                if (temp != null && temp.GetType().IsArray)
                                    throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", null,
                                        ExtendedTypeSystem.InvalidCastCannotRetrieveString);
                    // BUGBUG
                    // NTRAID#Windows Out of Band Releases - 930116 - 03/14/06
                    // handling special case for boolean, switchparameter and Nullable<bool>
                    // These parameter types will not be converted if the incoming value types are not
                    // one of the accepted categories - $true/$false or numbers (0 or otherwise)
                    if (LanguagePrimitives.IsBoolOrSwitchParameterType(_convertTypes[i]))
                        CheckBoolValue(result, _convertTypes[i]);
                    if (bindingScriptCmdlet)
                        // Check for conversion to something like bool[] or ICollection<bool>, but only for cmdlet binding
                        // to stay compatible with V1.
                        ParameterCollectionTypeInformation collectionTypeInfo = new ParameterCollectionTypeInformation(_convertTypes[i]);
                        if (collectionTypeInfo.ParameterCollectionType != ParameterCollectionType.NotCollection
                            && LanguagePrimitives.IsBoolOrSwitchParameterType(collectionTypeInfo.ElementType))
                            IList currentValueAsIList = ParameterBinderBase.GetIList(result);
                            if (currentValueAsIList != null)
                                foreach (object val in currentValueAsIList)
                                    CheckBoolValue(val, collectionTypeInfo.ElementType);
                                CheckBoolValue(result, collectionTypeInfo.ElementType);
                    result = LanguagePrimitives.ConvertTo(result, _convertTypes[i], CultureInfo.InvariantCulture);
                    // Do validation of invalid direct variable assignments which are allowed to
                    // be used for parameters.
                    // Note - this is duplicated in ExecutionContext.cs as parameter binding for script cmdlets can avoid this code path.
                    if ((!bindingScriptCmdlet) && (!bindingParameters))
                        // ActionPreference.Suspend is reserved for future use and is not supported as a preference variable.
                        if (_convertTypes[i] == typeof(ActionPreference))
                            ActionPreference resultPreference = (ActionPreference)result;
                            if (resultPreference == ActionPreference.Suspend)
                                throw new PSInvalidCastException("InvalidActionPreference", null, ErrorPackage.ActionPreferenceReservedForFutureUseError, resultPreference);
                throw new ArgumentTransformationMetadataException(e.Message, e);
            // Track the flow of untrusted object during the conversion when it's called directly from ParameterBinderBase.
            // When it's called from the override Transform method, the tracking is taken care of in the base type.
            if (bindingParameters || bindingScriptCmdlet)
                ExecutionContext.PropagateInputSource(inputData, result, engineIntrinsics.SessionState.Internal.LanguageMode);
        private static void CheckBoolValue(object value, Type boolType)
                Type resultType = value.GetType();
                if (resultType == typeof(PSObject))
                    resultType = ((PSObject)value).BaseObject.GetType();
                if (!(LanguagePrimitives.IsNumeric(resultType.GetTypeCode()) ||
                      LanguagePrimitives.IsBoolOrSwitchParameterType(resultType)))
                    ThrowPSInvalidBooleanArgumentCastException(resultType, boolType);
                bool isNullable = boolType.IsGenericType &&
                    boolType.GetGenericTypeDefinition() == typeof(Nullable<>);
                if (!isNullable && LanguagePrimitives.IsBooleanType(boolType))
                    ThrowPSInvalidBooleanArgumentCastException(null, boolType);
        internal static void ThrowPSInvalidBooleanArgumentCastException(Type resultType, Type convertType)
            throw new PSInvalidCastException("InvalidCastExceptionUnsupportedParameterType", null,
                                  ExtendedTypeSystem.InvalidCastExceptionForBooleanArgumentValue,
                                  resultType, convertType);
