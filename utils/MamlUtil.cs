    /// The MamlUtil class.
    internal static class MamlUtil
        /// Takes Name value from maml2 and overrides it in maml1.
        /// <param name="maml1"></param>
        /// <param name="maml2"></param>
        internal static void OverrideName(PSObject maml1, PSObject maml2)
            PrependPropertyValue(maml1, maml2, new string[] { "Name" }, true);
            PrependPropertyValue(maml1, maml2, new string[] { "Details", "Name" }, true);
        internal static void OverridePSTypeNames(PSObject maml1, PSObject maml2)
            foreach (var typename in maml2.TypeNames)
                if (typename.StartsWith(DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp, StringComparison.OrdinalIgnoreCase))
                    // Win8: 638494 if the original help is auto-generated, let the Provider help decide the format.
            maml1.TypeNames.Clear();
            foreach (string typeName in maml2.TypeNames)
                maml1.TypeNames.Add(typeName);
        /// Adds common properties like PSSnapIn,ModuleName from maml2 to maml1.
        internal static void AddCommonProperties(PSObject maml1, PSObject maml2)
            if (maml1.Properties["PSSnapIn"] == null)
                PSPropertyInfo snapInProperty = maml2.Properties["PSSnapIn"];
                if (snapInProperty != null)
                    maml1.Properties.Add(new PSNoteProperty("PSSnapIn", snapInProperty.Value));
            if (maml1.Properties["ModuleName"] == null)
                PSPropertyInfo moduleNameProperty = maml2.Properties["ModuleName"];
                if (moduleNameProperty != null)
                    maml1.Properties.Add(new PSNoteProperty("ModuleName", moduleNameProperty.Value));
        /// Prepend - Modify Syntax element in maml1 using the Syntax element from maml2.
        internal static void PrependSyntax(PSObject maml1, PSObject maml2)
            PrependPropertyValue(maml1, maml2, new string[] { "Syntax", "SyntaxItem" }, false);
        /// Prepend - Modify DetailedDescription element in maml1 using the DetailedDescription element from maml2.
        internal static void PrependDetailedDescription(PSObject maml1, PSObject maml2)
            PrependPropertyValue(maml1, maml2, new string[] { "Description" }, false);
        /// Override - Modify Parameters element in maml1 using the Parameters element from maml2.
        /// This will copy parameters from maml2 that are not present in maml1.
        internal static void OverrideParameters(PSObject maml1, PSObject maml2)
            string[] parametersPath = new string[] { "Parameters", "Parameter" };
            // Final collection of PSObjects.
            List<object> maml2items = new List<object>();
            // Add maml2 first since we are prepending.
            // For maml2: Add as collection or single item. No-op if
            PSPropertyInfo propertyInfo2 = GetPropertyInfo(maml2, parametersPath);
            var array = propertyInfo2.Value as Array;
                maml2items.AddRange(array as IEnumerable<object>);
                maml2items.Add(PSObject.AsPSObject(propertyInfo2.Value));
            // Extend maml1 to make sure the property-path exists - since we'll be modifying it soon.
            EnsurePropertyInfoPathExists(maml1, parametersPath);
            // For maml1: Add as collection or single item. Do nothing if null or some other type.
            PSPropertyInfo propertyInfo1 = GetPropertyInfo(maml1, parametersPath);
            List<object> maml1items = new List<object>();
            array = propertyInfo1.Value as Array;
                maml1items.AddRange(array as IEnumerable<object>);
                maml1items.Add(PSObject.AsPSObject(propertyInfo1.Value));
            // copy parameters from maml2 that are not present in maml1
            for (int index = 0; index < maml2items.Count; index++)
                PSObject m2paramObj = PSObject.AsPSObject(maml2items[index]);
                string param2Name = string.Empty;
                PSPropertyInfo m2propertyInfo = m2paramObj.Properties["Name"];
                if (m2propertyInfo != null)
                    if (!LanguagePrimitives.TryConvertTo<string>(m2propertyInfo.Value, out param2Name))
                bool isParamFoundInMaml1 = false;
                foreach (PSObject m1ParamObj in maml1items)
                    string param1Name = string.Empty;
                    PSPropertyInfo m1PropertyInfo = m1ParamObj.Properties["Name"];
                    if (m1PropertyInfo != null)
                        if (!LanguagePrimitives.TryConvertTo<string>(m1PropertyInfo.Value, out param1Name))
                    if (param1Name.Equals(param2Name, StringComparison.OrdinalIgnoreCase))
                        isParamFoundInMaml1 = true;
                if (!isParamFoundInMaml1)
                    maml1items.Add(maml2items[index]);
            // Now replace in maml1. If items.Count == 0 do nothing since Value is already null.
            if (maml1items.Count == 1)
                propertyInfo1.Value = maml1items[0];
            else if (maml1items.Count >= 2)
                propertyInfo1.Value = maml1items.ToArray();
        /// Prepend - Modify Notes element in maml1 using the Notes element from maml2.
        internal static void PrependNotes(PSObject maml1, PSObject maml2)
            PrependPropertyValue(maml1, maml2, new string[] { "AlertSet", "Alert" }, false);
        /// Get property info.
        internal static PSPropertyInfo GetPropertyInfo(PSObject psObject, string[] path)
            for (int i = 0; i < path.Length; ++i)
                string propertyName = path[i];
                PSPropertyInfo propertyInfo = psObject.Properties[propertyName];
                if (i == path.Length - 1)
                    return propertyInfo;
                if (propertyInfo == null || propertyInfo.Value is not PSObject)
                psObject = (PSObject)propertyInfo.Value;
            // We will never reach this line but the compiler needs some reassurance.
        /// Prepend property value.
        /// <param name="maml1">
        /// <param name="maml2">
        /// <param name="shouldOverride">
        /// Should Override the maml1 value from maml2 instead of prepend.
        internal static void PrependPropertyValue(PSObject maml1, PSObject maml2, string[] path, bool shouldOverride)
            List<object> items = new List<object>();
            PSPropertyInfo propertyInfo2 = GetPropertyInfo(maml2, path);
            if (propertyInfo2 != null)
                    items.AddRange(propertyInfo2.Value as IEnumerable<object>);
                    items.Add(propertyInfo2.Value);
            EnsurePropertyInfoPathExists(maml1, path);
            PSPropertyInfo propertyInfo1 = GetPropertyInfo(maml1, path);
            if (propertyInfo1 != null)
                if (!shouldOverride)
                    var array = propertyInfo1.Value as Array;
                        items.AddRange(propertyInfo1.Value as IEnumerable<object>);
                        items.Add(propertyInfo1.Value);
                if (items.Count == 1)
                    propertyInfo1.Value = items[0];
                else if (items.Count >= 2)
                    propertyInfo1.Value = items.ToArray();
        /// Ensure property info path exists.
        internal static void EnsurePropertyInfoPathExists(PSObject psObject, string[] path)
            // Walk the path and extend it if necessary.
                // Add a property info here if none was found.
                    // Add null on the last one, since we don't need to extend path further.
                    object propertyValue = (i < path.Length - 1) ? new PSObject() : null;
                    propertyInfo = new PSNoteProperty(propertyName, propertyValue);
                    psObject.Properties.Add(propertyInfo);
                // If we are on the last path element, we are done. Let's not mess with modifying Value.
                // If we are not on the last path element, let's make sure we can extend the path.
                if (propertyInfo.Value == null || propertyInfo.Value is not PSObject)
                    propertyInfo.Value = new PSObject();
                // Now move one step further along the path.
