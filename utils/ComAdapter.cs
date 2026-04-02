    /// Implements Adapter for the COM objects.
    internal class ComAdapter : Adapter
        ///   Constructor for the ComAdapter.
        /// <param name="typeinfo">Typeinfo for the com object we are adapting.</param>
        internal ComAdapter(ComTypeInfo typeinfo)
            Diagnostics.Assert(typeinfo != null, "Caller to verify typeinfo is not null.");
            _comTypeInfo = typeinfo;
        internal static string GetComTypeName(string clsid)
            StringBuilder firstType = new StringBuilder("System.__ComObject");
            firstType.Append("#{");
            firstType.Append(clsid);
            firstType.Append('}');
            return firstType.ToString();
            yield return GetComTypeName(_comTypeInfo.Clsid);
            ComProperty prop;
            if (_comTypeInfo.Properties.TryGetValue(memberName, out prop))
                if (prop.IsParameterized)
                    if (typeof(T).IsAssignableFrom(typeof(PSParameterizedProperty)))
                        return new PSParameterizedProperty(prop.Name, this, obj, prop) as T;
                else if (typeof(T).IsAssignableFrom(typeof(PSProperty)))
                    return new PSProperty(prop.Name, this, obj, prop) as T;
            ComMethod method;
            if (typeof(T).IsAssignableFrom(typeof(PSMethod)) &&
                (_comTypeInfo != null) && (_comTypeInfo.Methods.TryGetValue(memberName, out method)))
                PSMethod mshMethod = new PSMethod(method.Name, this, obj, method);
                return mshMethod as T;
            bool lookingForParameterizedProperties = typeof(T).IsAssignableFrom(typeof(PSParameterizedProperty));
            if (lookingForProperties || lookingForParameterizedProperties)
                foreach (ComProperty prop in _comTypeInfo.Properties.Values)
                    if (prop.IsParameterized
                        && lookingForParameterizedProperties
                        && predicate(prop.Name))
                    if (lookingForProperties && predicate(prop.Name))
            bool lookingForMethods = typeof(T).IsAssignableFrom(typeof(PSMethod));
            if (lookingForMethods)
                foreach (ComMethod method in _comTypeInfo.Methods.Values)
                    if (predicate(method.Name))
                        var mshMethod = new PSMethod(method.Name, this, obj, method);
            PSMemberInfoInternalCollection<T> collection = new PSMemberInfoInternalCollection<T>();
                        if (lookingForParameterizedProperties)
                            collection.Add(new PSParameterizedProperty(prop.Name, this, obj, prop) as T);
                    else if (lookingForProperties)
                        collection.Add(new PSProperty(prop.Name, this, obj, prop) as T);
                    if (collection[method.Name] == null)
                        PSMethod mshmethod = new PSMethod(method.Name, this, obj, method);
                        collection.Add(mshmethod as T);
            ComProperty prop = (ComProperty)property.adapterData;
            return prop.GetValue(property.baseObject);
        ///  <param name="convertIfPossible">instructs the adapter to convert before setting, if the adapter supports conversion</param>
            prop.SetValue(property.baseObject, setValue);
            return prop.IsSettable;
            return prop.IsGettable;
            return forDisplay ? ToStringCodeMethods.Type(prop.Type) : prop.Type.FullName;
        /// Get the property signature.
        /// <param name="property">Property object whose signature we want.</param>
        /// <returns>String representing the signature of the property.</returns>
            return prop.ToString();
        /// Called after a non null return from GetMethodData to try to call
            ComMethod commethod = (ComMethod)method.adapterData;
            return commethod.InvokeMethod(method, arguments);
        /// Called after a non null return from GetMethodData to return the overloads.
        /// <param name="method">The return of GetMethodData.</param>
            return commethod.MethodDefinitions();
            return prop.Type.FullName;
            return prop.GetValue(property.baseObject, arguments);
            prop.SetValue(property.baseObject, setValue, arguments);
            Collection<string> returnValue = new Collection<string> { prop.GetDefinition() };
