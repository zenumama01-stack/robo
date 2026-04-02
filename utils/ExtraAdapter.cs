    /// Deals with DirectoryEntry objects.
    internal class DirectoryEntryAdapter : DotNetAdapter
        // DirectoryEntry(DE) adapter needs dotnet adapter as DE adapter
        // don't know the underlying native adsi object's method metadata.
        // In the MethodInvoke() call, this adapter first calls
        // native adsi object's method, if there is a failure it calls
        // dotnet method (if one available).
        // This ensures dotnet methods are available on the adapted object.
        private static readonly DotNetAdapter s_dotNetAdapter = new DotNetAdapter();
            PSProperty property;
            DirectoryEntry entry = (DirectoryEntry)obj;
            // This line must precede InvokeGet. See the comment below.
            PropertyValueCollection collection = entry.Properties[memberName];
            object valueToTake = collection;
            // Even for the cases where propertyName does not exist
            // entry.Properties[propertyName] still returns a PropertyValueCollection.
            // The non schema way to check for a non existing property is to call entry.InvokeGet
            // and catch an eventual exception.
            // Specifically for "LDAP://RootDse" there are some cases where calling
            // InvokeGet will throw COMException for existing properties like defaultNamingContext.
            // Having a call to entry.Properties[propertyName] fixes the RootDse problem.
            // Calling entry.RefreshCache() also fixes the RootDse problem.
                object invokeGetValue = entry.InvokeGet(memberName);
                // if entry.Properties[memberName] returns empty value and invokeGet non-empty
                // value..take invokeGet's value. This will fix bug Windows Bug 121188.
                if ((collection == null) || ((collection.Value == null) && (invokeGetValue != null)))
                    valueToTake = invokeGetValue;
                property = new PSProperty(collection.PropertyName, this, obj, valueToTake);
                property = null;
            if (valueToTake == null)
                    #region Readme
                    // we are unable to find a native adsi object property.
                    // The next option is to find method. Unfortunately DirectoryEntry
                    // doesn't provide us a way to access underlying native object's method
                    // metadata.
                    // Adapter engine resolve's members in the following steps:
                    //  1. Extended members -> 2. Adapted members -> 3. Dotnet members
                    // We cannot say from DirectoryEntryAdapter if a method with name "memberName"
                    // is available. So check if a DotNet property with the same name is available
                    // If yes, return null from the adapted view and let adapter engine
                    // take care of DotNet member resolution. If not, assume memberName method
                    // is available on native adsi object.
                    // In case of collisions between Dotnet Property and adsi native object methods,
                    // Dotnet wins. Looking through IADs com interfaces there doesn't appear
                    // to be a collision like this.
                    // Powershell Parser will call only GetMember<PSMemberInfo>, so here
                    // we cannot distinguish if the caller is looking for a property or a
                    // method.
                    if (base.GetDotNetProperty<T>(obj, memberName) == null)
                        return new PSMethod(memberName, this, obj, null) as T;
            PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
            if (entry.Properties == null || entry.Properties.PropertyNames == null)
            int countOfProperties = 0;
                countOfProperties = entry.Properties.PropertyNames.Count;
            catch (Exception) // swallow all non-severe exceptions
            if (countOfProperties > 0)
                foreach (PropertyValueCollection property in entry.Properties)
                    members.Add(new PSProperty(property.PropertyName, this, obj, property) as T);
            return members;
            return property.adapterData;
            PropertyValueCollection values = property.adapterData as PropertyValueCollection;
                // This means GetMember returned PropertyValueCollection
                    values.Clear();
                    if (e.ErrorCode != unchecked((int)0x80004005) || (setValue == null))
                        // When clear is called, DirectoryEntry calls PutEx on AD object with Clear option and Null Value
                        // WinNT provider throws E_FAIL when null value is specified though actually ADS_PROPERTY_CLEAR option is used,
                        // we need to catch this exception here.
                        // But at the same time we don't want to catch the exception if user explicitly sets the value to null.
                IEnumerable enumValues = LanguagePrimitives.GetEnumerable(setValue);
                if (enumValues == null)
                    values.Add(setValue);
                    foreach (object objValue in enumValues)
                        values.Add(objValue);
                // This means GetMember returned the value from InvokeGet..So set the value using InvokeSet.
                DirectoryEntry entry = (DirectoryEntry)property.baseObject;
                Diagnostics.Assert(entry != null, "Object should be of type DirectoryEntry in DirectoryEntry adapter.");
                List<object> setValues = new List<object>();
                    setValues.Add(setValue);
                        setValues.Add(objValue);
                entry.InvokeSet(property.name, setValues.ToArray());
            ParameterInformation[] parameters = new ParameterInformation[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
                parameters[i] = new ParameterInformation(typeof(object), false, null, false);
            MethodInformation[] methodInformation = new MethodInformation[1];
            methodInformation[0] = new MethodInformation(false, false, parameters);
            GetBestMethodAndArguments(method.Name, methodInformation, arguments, out newArguments);
            DirectoryEntry entry = (DirectoryEntry)method.baseObject;
            // First try to invoke method on the native adsi object. If the method
            // call fails, try to invoke dotnet method with same name, if one available.
            // This will ensure dotnet methods are exposed for DE objects.
            // The problem is in GetMember<T>(), DE adapter cannot check if a requested
            // method is available as it doesn't have access to native adsi object's
            // method metadata. So GetMember<T> returns PSMethod assuming a method
            // is available. This behavior will never give a chance to dotnet adapter
            // to resolve method call. So the DE adapter owns calling dotnet method
            // if one available.
                return entry.Invoke(method.Name, newArguments);
            catch (DirectoryServicesCOMException dse)
                exception = dse;
            catch (TargetInvocationException tie)
                exception = tie;
            catch (COMException ce)
                exception = ce;
            // this code is reached only on exception
            // check if there is a dotnet method, invoke the dotnet method if available
            PSMethod dotNetmethod = s_dotNetAdapter.GetDotNetMethod<PSMethod>(method.baseObject, method.name);
            if (dotNetmethod != null)
                return dotNetmethod.Invoke(arguments);
        protected override string MethodToString(PSMethod method)
            foreach (string overload in MethodDefinitions(method))
                returnValue.Append(overload);
