    /// Adapter that deals with CimInstance objects.
    /// Implementing the PropertyOnlyAdapter for the time being as CimInstanceTypeAdapter currently
    /// supports only properties. If method support is needed in future, this should derive from
    /// Adapter class.
    public sealed class CimInstanceAdapter : PSPropertyAdapter
        private static PSAdaptedProperty GetCimPropertyAdapter(CimProperty property, object baseObject, string propertyName)
            PSAdaptedProperty propertyToAdd = new(propertyName, property);
            propertyToAdd.baseObject = baseObject;
            // propertyToAdd.adapter = this;
            return propertyToAdd;
        private static PSAdaptedProperty GetCimPropertyAdapter(CimProperty property, object baseObject)
                string propertyName = property.Name;
                return GetCimPropertyAdapter(property, baseObject, propertyName);
                // ignore "Name" property access failures and move on.
        private static PSAdaptedProperty GetPSComputerNameAdapter(CimInstance cimInstance)
            PSAdaptedProperty psComputerNameProperty = new(RemotingConstants.ComputerNameNoteProperty, cimInstance);
            psComputerNameProperty.baseObject = cimInstance;
            // psComputerNameProperty.adapter = this;
            return psComputerNameProperty;
        /// <param name="baseObject"></param>
        public override System.Collections.ObjectModel.Collection<PSAdaptedProperty> GetProperties(object baseObject)
            // baseObject should never be null
                    CimInstanceTypeAdapterResources.BaseObjectNotCimInstance,
                    "baseObject",
                    typeof(CimInstance).ToString());
                throw new PSInvalidOperationException(msg);
            Collection<PSAdaptedProperty> result = new();
            if (cimInstance.CimInstanceProperties != null)
                foreach (CimProperty property in cimInstance.CimInstanceProperties)
                    PSAdaptedProperty propertyToAdd = GetCimPropertyAdapter(property, baseObject);
                    if (propertyToAdd != null)
                        result.Add(propertyToAdd);
            PSAdaptedProperty psComputerNameProperty = GetPSComputerNameAdapter(cimInstance);
            if (psComputerNameProperty != null)
                result.Add(psComputerNameProperty);
        public override PSAdaptedProperty GetProperty(object baseObject, string propertyName)
            if (propertyName == null)
                throw new PSArgumentNullException(nameof(propertyName));
            CimProperty cimProperty = cimInstance.CimInstanceProperties[propertyName];
            if (cimProperty != null)
                PSAdaptedProperty prop = GetCimPropertyAdapter(cimProperty, baseObject, propertyName);
                return prop;
            if (propertyName.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
                PSAdaptedProperty prop = GetPSComputerNameAdapter(cimInstance);
        public override PSAdaptedProperty GetFirstPropertyOrDefault(object baseObject, MemberNamePredicate predicate)
            if (predicate == null)
                throw new PSArgumentNullException(nameof(predicate));
                string msg = string.Format(
            if (predicate(RemotingConstants.ComputerNameNoteProperty))
            foreach (CimProperty cimProperty in cimInstance.CimInstanceProperties)
                if (cimProperty != null && predicate(cimProperty.Name))
                    PSAdaptedProperty prop = GetCimPropertyAdapter(cimProperty, baseObject, cimProperty.Name);
        internal static string CimTypeToTypeNameDisplayString(CimType cimType)
            switch (cimType)
                case CimType.DateTime:
                case CimType.Reference:
                case CimType.DateTimeArray:
                case CimType.ReferenceArray:
                    return "CimInstance#" + cimType.ToString();
                    return ToStringCodeMethods.Type(
                        CimConverter.GetDotNetType(cimType));
        /// <param name="adaptedProperty"></param>
        public override string GetPropertyTypeName(PSAdaptedProperty adaptedProperty)
            ArgumentNullException.ThrowIfNull(adaptedProperty);
            if (adaptedProperty.Tag is CimProperty cimProperty)
                return CimTypeToTypeNameDisplayString(cimProperty.CimType);
            if (adaptedProperty.Name.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
                return ToStringCodeMethods.Type(typeof(string));
            throw new ArgumentNullException(nameof(adaptedProperty));
        public override object GetPropertyValue(PSAdaptedProperty adaptedProperty)
                return cimProperty.Value;
                CimInstance cimInstance = (CimInstance)adaptedProperty.Tag;
                return cimInstance.GetCimSessionComputerName();
        private static void AddTypeNameHierarchy(IList<string> typeNamesWithNamespace, IList<string> typeNamesWithoutNamespace, string namespaceName, string className)
            if (!string.IsNullOrEmpty(namespaceName))
                string fullTypeName = string.Create(CultureInfo.InvariantCulture, $"Microsoft.Management.Infrastructure.CimInstance#{namespaceName}/{className}");
                typeNamesWithNamespace.Add(fullTypeName);
            typeNamesWithoutNamespace.Add(string.Create(CultureInfo.InvariantCulture, $"Microsoft.Management.Infrastructure.CimInstance#{className}"));
        private static List<CimClass> GetInheritanceChain(CimInstance cimInstance)
            List<CimClass> inheritanceChain = new();
            CimClass cimClass = cimInstance.CimClass;
            Dbg.Assert(cimClass != null, "CimInstance should always have ClassDecl");
            while (cimClass != null)
                inheritanceChain.Add(cimClass);
                    cimClass = cimClass.CimSuperClass;
            return inheritanceChain;
        public override Collection<string> GetTypeNameHierarchy(object baseObject)
                throw new ArgumentNullException(nameof(baseObject));
            var typeNamesWithNamespace = new List<string>();
            var typeNamesWithoutNamespace = new List<string>();
            IList<CimClass> inheritanceChain = GetInheritanceChain(cimInstance);
            if ((inheritanceChain == null) || (inheritanceChain.Count == 0))
                AddTypeNameHierarchy(
                    typeNamesWithNamespace,
                    typeNamesWithoutNamespace,
                    cimInstance.CimSystemProperties.Namespace,
                    cimInstance.CimSystemProperties.ClassName);
                foreach (CimClass cimClass in inheritanceChain)
                        cimClass.CimSystemProperties.Namespace,
                        cimClass.CimSystemProperties.ClassName);
                    cimClass.Dispose();
            result.AddRange(typeNamesWithNamespace);
            result.AddRange(typeNamesWithoutNamespace);
            if (baseObject != null)
                for (Type type = baseObject.GetType(); type != null; type = type.BaseType)
                    result.Add(type.FullName);
            return new Collection<string>(result);
        public override bool IsGettable(PSAdaptedProperty adaptedProperty)
            /* I was explicitly asked to only use MI_FLAG_READONLY for now
            // based on DSP0004, version 2.6.0, section "5.5.3.41 Read" (page 85, lines 2881-2884)
            bool readQualifierValue = this.GetPropertyQualifierValue(adaptedProperty, "Read", defaultValue: true);
            return readQualifierValue;
        public override bool IsSettable(PSAdaptedProperty adaptedProperty)
            // based on DSP0004, version 2.6.0, section "5.5.3.55 Write" (pages 89-90, lines 3056-3061)
            bool writeQualifierValue = this.GetPropertyQualifierValue(adaptedProperty, "Write", defaultValue: false);
            return writeQualifierValue;
            if (adaptedProperty == null)
            if (adaptedProperty.Tag is not CimProperty cimProperty)
            bool isReadOnly = ((cimProperty.Flags & CimFlags.ReadOnly) == CimFlags.ReadOnly);
            bool isSettable = !isReadOnly;
            return isSettable;
        public override void SetPropertyValue(PSAdaptedProperty adaptedProperty, object value)
            if (!IsSettable(adaptedProperty))
                throw new SetValueException("ReadOnlyCIMProperty",
                        CimInstanceTypeAdapterResources.ReadOnlyCIMProperty,
                        adaptedProperty.Name);
            CimProperty cimProperty = adaptedProperty.Tag as CimProperty;
            object valueToSet = value;
            if (valueToSet != null)
                // Convert only if value is not null
                Type paramType;
                switch (cimProperty.CimType)
                        paramType = typeof(object);
                        paramType = typeof(object[]);
                        paramType = CimConverter.GetDotNetType(cimProperty.CimType);
                        Dbg.Assert(paramType != null, "'default' case should only be used for well-defined CimType->DotNetType conversions");
                valueToSet = Adapter.PropertySetAndMethodArgumentConvertTo(
                    value, paramType, CultureInfo.InvariantCulture);
            cimProperty.Value = valueToSet;
