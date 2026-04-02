    /// Wraps an object providing alternate views of the available members
    /// and ways to extend them. Members can be methods, properties,
    /// parameterized properties, etc.
    /// It is permitted to subclass <see cref="PSObject"/>
    [TypeDescriptionProvider(typeof(PSObjectTypeDescriptionProvider))]
    public class PSObject : IFormattable, IComparable, ISerializable, IDynamicMetaObjectProvider
        #region private to the constructors
        internal TypeTable GetTypeTable()
            if (_typeTable != null && _typeTable.TryGetTarget(out TypeTable typeTable))
            ExecutionContext context = LocalPipeline.GetExecutionContextFromTLS();
            return context?.TypeTable;
        internal static T TypeTableGetMemberDelegate<T>(PSObject msjObj, string name) where T : PSMemberInfo
            TypeTable table = msjObj.GetTypeTable();
            return TypeTableGetMemberDelegate<T>(msjObj, table, name);
        private static T TypeTableGetMemberDelegate<T>(PSObject msjObj, TypeTable typeTableToUse, string name) where T : PSMemberInfo
            if (typeTableToUse == null)
            PSMemberInfoInternalCollection<PSMemberInfo> allMembers = typeTableToUse.GetMembers<PSMemberInfo>(msjObj.InternalTypeNames);
            PSMemberInfo member = allMembers[name];
                PSObject.MemberResolution.WriteLine("\"{0}\" NOT present in type table.", name);
                PSObject.MemberResolution.WriteLine("\"{0}\" present in type table.", name);
            PSObject.MemberResolution.WriteLine("\"{0}\" from types table ignored because it has type {1} instead of {2}.",
                name, member.GetType(), typeof(T));
        internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(PSObject msjObj) where T : PSMemberInfo
            return TypeTableGetMembersDelegate<T>(msjObj, table);
        internal static PSMemberInfoInternalCollection<T> TypeTableGetMembersDelegate<T>(PSObject msjObj, TypeTable typeTableToUse) where T : PSMemberInfo
                return new PSMemberInfoInternalCollection<T>();
            PSMemberInfoInternalCollection<T> members = typeTableToUse.GetMembers<T>(msjObj.InternalTypeNames);
            PSObject.MemberResolution.WriteLine("Type table members: {0}.", members.Count);
        internal static T TypeTableGetFirstMemberOrDefaultDelegate<T>(PSObject msjObj, MemberNamePredicate predicate) where T : PSMemberInfo
            return TypeTableGetFirstOrDefaultMemberDelegate<T>(msjObj, table, predicate);
        internal static T TypeTableGetFirstOrDefaultMemberDelegate<T>(PSObject msjObj, TypeTable typeTableToUse, MemberNamePredicate predicate) where T : PSMemberInfo
            return typeTableToUse?.GetFirstMemberOrDefault<T>(msjObj.InternalTypeNames, predicate);
        private static T AdapterGetMemberDelegate<T>(PSObject msjObj, string name) where T : PSMemberInfo
            if (msjObj.IsDeserialized)
                if (msjObj.AdaptedMembers == null)
                T adaptedMember = msjObj.AdaptedMembers[name] as T;
                PSObject.MemberResolution.WriteLine("Serialized adapted member: {0}.", adaptedMember == null ? "not found" : adaptedMember.Name);
                return adaptedMember;
            T retValue = msjObj.InternalAdapter.BaseGetMember<T>(msjObj.ImmediateBaseObject, name);
            PSObject.MemberResolution.WriteLine("Adapted member: {0}.", retValue == null ? "not found" : retValue.Name);
        private static T AdapterGetFirstMemberOrDefaultDelegate<T>(PSObject msjObj, MemberNamePredicate predicate) where T : PSMemberInfo
            if (msjObj.IsDeserialized && typeof(T).IsAssignableFrom(typeof(PSPropertyInfo)))
                foreach (var adaptedMember in msjObj.AdaptedMembers)
                    if (predicate(adaptedMember.Name))
                        return adaptedMember as T;
            T retValue = msjObj.InternalAdapter.BaseGetFirstMemberOrDefault<T>(msjObj._immediateBaseObject, predicate);
        internal static PSMemberInfoInternalCollection<TResult> TransformMemberInfoCollection<TSource, TResult>(PSMemberInfoCollection<TSource> source)
            where TSource : PSMemberInfo where TResult : PSMemberInfo
            if (typeof(TSource) == typeof(TResult))
                // If the types are the same, don't make a copy, return the cached collection.
                return source as PSMemberInfoInternalCollection<TResult>;
            PSMemberInfoInternalCollection<TResult> returnValue = new PSMemberInfoInternalCollection<TResult>();
            foreach (TSource member in source)
                if (member is TResult result)
                    returnValue.Add(result);
        private static PSMemberInfoInternalCollection<T> AdapterGetMembersDelegate<T>(PSObject msjObj) where T : PSMemberInfo
                PSObject.MemberResolution.WriteLine("Serialized adapted members: {0}.", msjObj.AdaptedMembers.Count);
                return TransformMemberInfoCollection<PSPropertyInfo, T>(msjObj.AdaptedMembers);
            PSMemberInfoInternalCollection<T> retValue = msjObj.InternalAdapter.BaseGetMembers<T>(msjObj._immediateBaseObject);
            PSObject.MemberResolution.WriteLine("Adapted members: {0}.", retValue.VisibleCount);
        private static PSMemberInfoInternalCollection<T> DotNetGetMembersDelegate<T>(PSObject msjObj) where T : PSMemberInfo
            // Don't lookup dotnet members if the object doesn't insist.
            if (msjObj.InternalBaseDotNetAdapter != null)
                PSMemberInfoInternalCollection<T> retValue = msjObj.InternalBaseDotNetAdapter.BaseGetMembers<T>(msjObj._immediateBaseObject);
                PSObject.MemberResolution.WriteLine("DotNet members: {0}.", retValue.VisibleCount);
        private static T DotNetGetMemberDelegate<T>(PSObject msjObj, string name) where T : PSMemberInfo
            // Don't lookup dotnet member if the object doesn't insist.
                T retValue = msjObj.InternalBaseDotNetAdapter.BaseGetMember<T>(msjObj._immediateBaseObject, name);
                PSObject.MemberResolution.WriteLine("DotNet member: {0}.", retValue == null ? "not found" : retValue.Name);
        private static T DotNetGetFirstMemberOrDefaultDelegate<T>(PSObject msjObj, MemberNamePredicate predicate) where T : PSMemberInfo
            return msjObj.InternalBaseDotNetAdapter?.BaseGetFirstMemberOrDefault<T>(msjObj._immediateBaseObject, predicate);
        /// A collection of delegates to get Extended/Adapted/Dotnet members based on the
        /// <paramref name="viewType"/>
        /// <param name="viewType">
        /// A filter to select Extended/Adapted/Dotnet view of the object
        internal static Collection<CollectionEntry<PSMemberInfo>> GetMemberCollection(PSMemberViewTypes viewType)
            return GetMemberCollection(viewType, null);
        /// Backup type table to use if there is no execution context associated with the current thread
        internal static Collection<CollectionEntry<PSMemberInfo>> GetMemberCollection(
            PSMemberViewTypes viewType,
            if ((viewType & PSMemberViewTypes.Extended) == PSMemberViewTypes.Extended)
                if (backupTypeTable == null)
                        msjObj => TypeTableGetMembersDelegate<PSMemberInfo>(msjObj, backupTypeTable),
                        (msjObj, name) => TypeTableGetMemberDelegate<PSMemberInfo>(msjObj, backupTypeTable, name),
                        (msjObj, predicate) => TypeTableGetFirstOrDefaultMemberDelegate<PSMemberInfo>(msjObj, backupTypeTable, predicate),
            if ((viewType & PSMemberViewTypes.Adapted) == PSMemberViewTypes.Adapted)
                    PSObject.AdapterGetMembersDelegate<PSMemberInfo>,
                    PSObject.AdapterGetMemberDelegate<PSMemberInfo>,
                    PSObject.AdapterGetFirstMemberOrDefaultDelegate<PSMemberInfo>,
                    shouldReplicateWhenReturning: false,
                    shouldCloneWhenReturning: false,
                    collectionNameForTracing: "adapted members"));
            if ((viewType & PSMemberViewTypes.Base) == PSMemberViewTypes.Base)
                    PSObject.DotNetGetMembersDelegate<PSMemberInfo>,
                    PSObject.DotNetGetMemberDelegate<PSMemberInfo>,
                    PSObject.DotNetGetFirstMemberOrDefaultDelegate<PSMemberInfo>,
                    collectionNameForTracing: "clr members"));
        private static Collection<CollectionEntry<PSMethodInfo>> GetMethodCollection()
            Collection<CollectionEntry<PSMethodInfo>> returnValue = new Collection<CollectionEntry<PSMethodInfo>>
                new CollectionEntry<PSMethodInfo>(
                    shouldReplicateWhenReturning: true,
                    shouldCloneWhenReturning: true,
                    collectionNameForTracing: "type table members"),
                    PSObject.AdapterGetMembersDelegate<PSMethodInfo>,
                    PSObject.AdapterGetMemberDelegate<PSMethodInfo>,
                    PSObject.AdapterGetFirstMemberOrDefaultDelegate<PSMethodInfo>,
                    collectionNameForTracing: "adapted members"),
                    PSObject.DotNetGetMembersDelegate<PSMethodInfo>,
                    PSObject.DotNetGetMemberDelegate<PSMethodInfo>,
                    PSObject.DotNetGetFirstMemberOrDefaultDelegate<PSMethodInfo>,
                    collectionNameForTracing: "clr members")
        /// A collection of delegates to get Extended/Adapted/Dotnet properties based on the
        internal static Collection<CollectionEntry<PSPropertyInfo>> GetPropertyCollection(
            PSMemberViewTypes viewType)
            return GetPropertyCollection(viewType, null);
                        msjObj => TypeTableGetMembersDelegate<PSPropertyInfo>(msjObj, backupTypeTable),
                        (msjObj, name) => TypeTableGetMemberDelegate<PSPropertyInfo>(msjObj, backupTypeTable, name),
                    PSObject.AdapterGetMembersDelegate<PSPropertyInfo>,
                    PSObject.AdapterGetMemberDelegate<PSPropertyInfo>,
                    PSObject.AdapterGetFirstMemberOrDefaultDelegate<PSPropertyInfo>,
                    false, false, "adapted members"));
                    PSObject.DotNetGetMembersDelegate<PSPropertyInfo>,
                    PSObject.DotNetGetMemberDelegate<PSPropertyInfo>,
                    PSObject.DotNetGetFirstMemberOrDefaultDelegate<PSPropertyInfo>,
                    false, false, "clr members"));
        private void CommonInitialization(object obj)
            Diagnostics.Assert(obj != null, "checked by callers");
            if (obj is PSCustomObject)
                this.ImmediateBaseObjectIsEmpty = true;
            _immediateBaseObject = obj;
            _typeTable = context?.TypeTableWeakReference;
        #region Adapter Mappings
        private static readonly ConcurrentDictionary<Type, AdapterSet> s_adapterMapping = new ConcurrentDictionary<Type, AdapterSet>();
        private static readonly List<Func<object, AdapterSet>> s_adapterSetMappers = new List<Func<object, AdapterSet>>
                                                                                        MappedInternalAdapterSet
        private static AdapterSet MappedInternalAdapterSet(object obj)
            if (obj is PSMemberSet) { return PSObject.s_mshMemberSetAdapter; }
            if (obj is PSObject) { return PSObject.s_mshObjectAdapter; }
            if (obj is CimInstance) { return PSObject.s_cimInstanceAdapter; }
            if (obj is ManagementClass) { return PSObject.s_managementClassAdapter; }
            if (obj is ManagementBaseObject) { return PSObject.s_managementObjectAdapter; }
            if (obj is DirectoryEntry) { return PSObject.s_directoryEntryAdapter; }
            if (obj is DataRowView) { return PSObject.s_dataRowViewAdapter; }
            if (obj is DataRow) { return PSObject.s_dataRowAdapter; }
            if (obj is XmlNode) { return PSObject.s_xmlNodeAdapter; }
        /// Returns the adapter corresponding to obj.GetType()
        /// <returns>The adapter set corresponding to obj.GetType().</returns>
        internal static AdapterSet GetMappedAdapter(object obj, TypeTable typeTable)
            Type objectType = obj.GetType();
            PSObject.AdapterSet adapter = typeTable?.GetTypeAdapter(objectType);
            if (adapter != null)
                // We don't cache results found via the type table because type tables may differ b/w runspaces,
                // our cache is app domain wide, and the key is simply the type.
                return adapter;
            if (s_adapterMapping.TryGetValue(objectType, out AdapterSet result))
            lock (s_adapterSetMappers)
                foreach (var mapper in s_adapterSetMappers)
                    result = mapper(obj);
                if (objectType.IsCOMObject)
                    // All WinRT types are COM types.
                    // All WinRT types would contain the TypeAttributes flag being set to WindowsRunTime.
                    if (WinRTHelper.IsWinRTType(objectType))
                        result = PSObject.s_dotNetInstanceAdapterSet;
                    // We are not using IsAssignableFrom because we want to avoid
                    // using the COM adapters for Primary Interop Assembly types
                    // and they derive from System.__ComObject.
                    // We are not using Type.Equals because System.__ComObject is
                    // not public.
                    else if (objectType.FullName.Equals("System.__ComObject"))
                        // We don't cache the adapter set for COM objects because the adapter varies depending on the COM object, but often
                        // (typically), the runtime type is always the same.  That's why this if statement is here and returns.
                        ComTypeInfo info = ComTypeInfo.GetDispatchTypeInfo(obj);
                        return info != null
                                   ? new AdapterSet(new ComAdapter(info), DotNetInstanceAdapter)
                                   : PSObject.s_dotNetInstanceAdapterSet;
                        result = info != null
                                   ? new AdapterSet(new DotNetAdapterWithComTypeName(info), null)
            var existingOrNew = s_adapterMapping.GetOrAdd(objectType, result);
            Diagnostics.Assert(existingOrNew == result, "There is a logic error in caching adapter sets.");
        internal static AdapterSet CreateThirdPartyAdapterSet(Type adaptedType, PSPropertyAdapter adapter)
            return new AdapterSet(new ThirdPartyAdapter(adaptedType, adapter), s_baseAdapterForAdaptedObjects);
        #endregion private to the constructors
        /// Initializes a new instance of PSObject with an PSCustomObject BaseObject.
        public PSObject()
            CommonInitialization(PSCustomObject.SelfInstance);
        /// Initializes a new instance of PSObject with an PSCustomObject BaseObject
        /// with an initial capacity for members.
        /// <param name="instanceMemberCapacity">The initial capacity for the instance member collection.</param>
        public PSObject(int instanceMemberCapacity) : this()
            _instanceMembers = new PSMemberInfoInternalCollection<PSMemberInfo>(instanceMemberCapacity);
        /// Initializes a new instance of PSObject wrapping obj (accessible through BaseObject).
        /// <param name="obj">Object we are wrapping.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="obj"/> is null.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "This is shipped as part of V1. Retaining this for backward compatibility.")]
        public PSObject(object obj)
                throw PSTraceSource.NewArgumentNullException(nameof(obj));
            CommonInitialization(obj);
        /// Creates a PSObject from an ISerializable context.
        /// <param name="info">Serialization information for this instance.</param>
        /// <param name="context">The streaming context for this instance.</param>
        protected PSObject(SerializationInfo info, StreamingContext context)
            if (info.GetValue("CliXml", typeof(string)) is not string serializedData)
            PSObject result = PSObject.AsPSObject(PSSerializer.Deserialize(serializedData));
            CommonInitialization(result.ImmediateBaseObject);
            CopyDeserializerFields(source: result, target: this);
        internal static PSObject ConstructPSObjectFromSerializationInfo(SerializationInfo info, StreamingContext context)
            return new PSObject(info, context);
        #region instance fields
        private readonly object _lockObject = new object();
        private ConsolidatedString _typeNames;
        /// This is the main field in the class representing
        /// the System.Object we are encapsulating.
        private object _immediateBaseObject;
        private WeakReference<TypeTable> _typeTable;
        private AdapterSet _adapterSet;
        private PSMemberInfoInternalCollection<PSMemberInfo> _instanceMembers;
        private PSMemberInfoIntegratingCollection<PSMemberInfo> _members;
        private PSMemberInfoIntegratingCollection<PSPropertyInfo> _properties;
        private PSMemberInfoIntegratingCollection<PSMethodInfo> _methods;
        private PSObjectFlags _flags;
        #endregion instance fields
        private static readonly PSTraceSource s_memberResolution = PSTraceSource.GetTracer("MemberResolution", "Traces the resolution from member name to the member. A member can be a property, method, etc.", false);
        private static readonly ConditionalWeakTable<object, ConsolidatedString> s_typeNamesResurrectionTable = new ConditionalWeakTable<object, ConsolidatedString>();
        private static readonly Collection<CollectionEntry<PSMemberInfo>> s_memberCollection = GetMemberCollection(PSMemberViewTypes.All);
        private static readonly Collection<CollectionEntry<PSMethodInfo>> s_methodCollection = GetMethodCollection();
        private static readonly Collection<CollectionEntry<PSPropertyInfo>> s_propertyCollection = GetPropertyCollection(PSMemberViewTypes.All);
        private static readonly DotNetAdapter s_dotNetInstanceAdapter = new DotNetAdapter();
        private static readonly DotNetAdapter s_baseAdapterForAdaptedObjects = new BaseDotNetAdapterForAdaptedObjects();
        private static readonly DotNetAdapter s_dotNetStaticAdapter = new DotNetAdapter(true);
        private static readonly AdapterSet s_dotNetInstanceAdapterSet = new AdapterSet(DotNetInstanceAdapter, null);
        private static readonly AdapterSet s_mshMemberSetAdapter = new AdapterSet(new PSMemberSetAdapter(), null);
        private static readonly AdapterSet s_mshObjectAdapter = new AdapterSet(new PSObjectAdapter(), null);
        private static readonly PSObject.AdapterSet s_cimInstanceAdapter =
            new PSObject.AdapterSet(new ThirdPartyAdapter(typeof(Microsoft.Management.Infrastructure.CimInstance),
                                                          new Microsoft.PowerShell.Cim.CimInstanceAdapter()),
                                    PSObject.DotNetInstanceAdapter);
        private static readonly AdapterSet s_managementObjectAdapter = new AdapterSet(new ManagementObjectAdapter(), DotNetInstanceAdapter);
        private static readonly AdapterSet s_managementClassAdapter = new AdapterSet(new ManagementClassApdapter(), DotNetInstanceAdapter);
        private static readonly AdapterSet s_directoryEntryAdapter = new AdapterSet(new DirectoryEntryAdapter(), DotNetInstanceAdapter);
        private static readonly AdapterSet s_dataRowViewAdapter = new AdapterSet(new DataRowViewAdapter(), s_baseAdapterForAdaptedObjects);
        private static readonly AdapterSet s_dataRowAdapter = new AdapterSet(new DataRowAdapter(), s_baseAdapterForAdaptedObjects);
        private static readonly AdapterSet s_xmlNodeAdapter = new AdapterSet(new XmlNodeAdapter(), s_baseAdapterForAdaptedObjects);
        internal PSMemberInfoInternalCollection<PSMemberInfo> InstanceMembers
                if (_instanceMembers == null)
                    lock (_lockObject)
                        _instanceMembers ??=
                            s_instanceMembersResurrectionTable.GetValue(
                                GetKeyForResurrectionTables(this),
                                _ => new PSMemberInfoInternalCollection<PSMemberInfo>());
                return _instanceMembers;
            set => _instanceMembers = value;
        /// This is the adapter that will depend on the type of baseObject.
        internal Adapter InternalAdapter => InternalAdapterSet.OriginalAdapter;
        /// This is the adapter that is used to resolve the base dotnet members for an
        /// adapted object. If an object is not adapted, this will be null.
        /// If an object is not adapted, InternalAdapter will use the dotnet adapter.
        /// So there is no point falling back to the same dotnet adapter.
        /// If an object is adapted, this adapter will be used to resolve the dotnet
        internal Adapter InternalBaseDotNetAdapter => InternalAdapterSet.DotNetAdapter;
        /// This is the adapter set that will contain the adapter of the baseObject
        /// and the ultimate .net member lookup adapter.
        /// See <see cref="PSObject.AdapterSet"/> for explanation.
        private AdapterSet InternalAdapterSet
                if (_adapterSet == null)
                        _adapterSet ??= GetMappedAdapter(_immediateBaseObject, GetTypeTable());
                return _adapterSet;
        public PSMemberInfoCollection<PSMemberInfo> Members
                        _members ??= new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, s_memberCollection);
        public PSMemberInfoCollection<PSPropertyInfo> Properties
                if (_properties == null)
                        _properties ??= new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, s_propertyCollection);
                return _properties;
        public PSMemberInfoCollection<PSMethodInfo> Methods
                if (_methods == null)
                        _methods ??= new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, s_methodCollection);
                return _methods;
        /// Gets the object we are directly wrapping.
        /// <remarks>If the ImmediateBaseObject is another PSObject,
        /// that PSObject will be returned.</remarks>
        public object ImmediateBaseObject => _immediateBaseObject;
        /// Gets the object we are wrapping.
        /// <remarks>If the ImmediateBaseObject is another PSObject, this property
        /// will return its BaseObject.</remarks>
        public object BaseObject
                PSObject mshObj = this;
                    returnValue = mshObj._immediateBaseObject;
                    mshObj = returnValue as PSObject;
                } while (mshObj != null);
        /// Gets the type names collection initially containing the object type hierarchy.
        public Collection<string> TypeNames
                var result = InternalTypeNames;
                if (result.IsReadOnly)
                        // Check again after acquiring the lock to ensure some other thread didn't make the copy.
                            _typeNames = s_typeNamesResurrectionTable.GetValue(
                                            _ => new ConsolidatedString(_typeNames));
                            object baseObj = BaseObject;
                            // In most cases, the TypeNames will be modified after it's returned
                            if (baseObj != null) { PSVariableAssignmentBinder.NoteTypeHasInstanceMemberOrTypeName(baseObj.GetType()); }
                            return _typeNames;
        internal ConsolidatedString InternalTypeNames
                if (_typeNames == null)
                            if (!s_typeNamesResurrectionTable.TryGetValue(GetKeyForResurrectionTables(this), out _typeNames))
                                // We don't cache this typename in the resurrection table because it's cached in the psobject,
                                // and the assumption is we'll usually get the value directly from the PSObject, so it's not
                                // needed in the resurrection table.
                                // If we hand out the typename to a client that could change it (through the public property
                                // TypeNames), then we'll need to store the copy in the resurrection table.
                                _typeNames = InternalAdapter.BaseGetTypeNameHierarchy(_immediateBaseObject);
            set => _typeNames = value;
        internal static ConsolidatedString GetTypeNames(object obj)
            if (obj is PSObject psobj)
                return psobj.InternalTypeNames;
            if (HasInstanceTypeName(obj, out ConsolidatedString result))
            return PSObject.GetMappedAdapter(obj, null).OriginalAdapter.BaseGetTypeNameHierarchy(obj);
        internal static bool HasInstanceTypeName(object obj, out ConsolidatedString result)
            return s_typeNamesResurrectionTable.TryGetValue(GetKeyForResurrectionTables(obj), out result);
        internal static bool HasInstanceMembers(object obj, out PSMemberInfoInternalCollection<PSMemberInfo> instanceMembers)
                lock (psobj)
                    if (psobj._instanceMembers == null)
                        s_instanceMembersResurrectionTable.TryGetValue(GetKeyForResurrectionTables(psobj), out psobj._instanceMembers);
                instanceMembers = psobj._instanceMembers;
            else if (obj != null)
                s_instanceMembersResurrectionTable.TryGetValue(GetKeyForResurrectionTables(obj), out instanceMembers);
                instanceMembers = null;
            return instanceMembers != null && instanceMembers.Count > 0;
        private static readonly ConditionalWeakTable<object, PSMemberInfoInternalCollection<PSMemberInfo>> s_instanceMembersResurrectionTable =
            new ConditionalWeakTable<object, PSMemberInfoInternalCollection<PSMemberInfo>>();
        public static implicit operator PSObject(int valueToConvert)
        public static implicit operator PSObject(string valueToConvert)
        public static implicit operator PSObject(Hashtable valueToConvert)
        public static implicit operator PSObject(double valueToConvert)
        public static implicit operator PSObject(bool valueToConvert)
        /// If obj is not an PSObject, it is returned. Otherwise, retrieves
        /// the first non PSObject or PSObject with CustomBaseObject
        /// in the PSObject - BaseObject chain.
        internal static object Base(object obj)
            if (obj is not PSObject mshObj)
            if (mshObj == AutomationNull.Value)
            if (mshObj.ImmediateBaseObjectIsEmpty)
            } while ((mshObj != null) && (!mshObj.ImmediateBaseObjectIsEmpty));
        internal static PSMemberInfo GetStaticCLRMember(object obj, string methodName)
            if (obj == null || methodName == null || methodName.Length == 0)
            var objType = obj as Type ?? obj.GetType();
            return DotNetStaticAdapter.BaseGetMember<PSMemberInfo>(objType, methodName);
        /// If obj is an PSObject it will be returned as is, otherwise
        /// a new PSObject will be created based on obj.
        /// <param name="obj">Object to be wrapped.</param>
        /// obj or a new PSObject whose BaseObject is obj
        public static PSObject AsPSObject(object obj)
            return AsPSObject(obj, false);
        /// If obj is a PSObject, it will be returned as is, otherwise a new
        /// PSObject will be created on obj. Its InstanceMembers and TypeNames
        /// will be initialized if we are not going to use the ResurrectionTables
        /// for this PSObject instance.
        /// <param name="storeTypeNameAndInstanceMembersLocally"></param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "AsPSObject is shipped as part of V1. This is a new overload method.")]
        internal static PSObject AsPSObject(object obj, bool storeTypeNameAndInstanceMembersLocally)
            if (obj is PSObject so)
            return new PSObject(obj) { StoreTypeNameAndInstanceMembersLocally = storeTypeNameAndInstanceMembersLocally };
        /// Returns an object that should be used as a key for getting 1) instance members and 2) type names
        /// - If base object is a PSCustomObject or a string or a type
        ///   then the most nested wrapping PSObject is returned (the PSObject where immediateBaseObject=PSCustomObject/string/type)
        /// - Otherwise the base object is returned
        /// This is a temporary fix for Win8 : 254345 - Job Failed By Throwing ExtendedTypeSystemException.
        internal static object GetKeyForResurrectionTables(object obj)
            if (obj is not PSObject pso)
            PSObject psObjectAboveBase = pso;
            while (psObjectAboveBase.ImmediateBaseObject is PSObject)
                psObjectAboveBase = (PSObject)psObjectAboveBase.ImmediateBaseObject;
            if (psObjectAboveBase.ImmediateBaseObject is PSCustomObject
                || psObjectAboveBase.ImmediateBaseObject is string
                || pso.StoreTypeNameAndInstanceMembersLocally)
                return psObjectAboveBase;
            return psObjectAboveBase.ImmediateBaseObject;
        #region instance methods
        private static string GetSeparator(ExecutionContext context, string separator)
            if (separator != null)
                return separator;
            object obj = context?.GetVariableValue(SpecialVariables.OFSVarPath);
            return " ";
        internal static string ToStringEnumerator(ExecutionContext context, IEnumerator enumerator, string separator, string format, IFormatProvider formatProvider)
            string separatorToUse = GetSeparator(context, separator);
                object obj = enumerator.Current;
                returnValue.Append(PSObject.ToString(context, obj, separator, format, formatProvider, false, false));
                returnValue.Append(separatorToUse);
            if (returnValue.Length == 0)
            int separatorLength = separatorToUse.Length;
            returnValue.Remove(returnValue.Length - separatorLength, separatorLength);
        internal static string ToStringEnumerable(ExecutionContext context, IEnumerable enumerable, string separator, string format, IFormatProvider formatProvider)
                    PSObject mshObj = PSObject.AsPSObject(obj);
                    returnValue.Append(PSObject.ToString(context, mshObj, separator, format, formatProvider, false, false));
        private static string ToStringEmptyBaseObject(ExecutionContext context, PSObject mshObj, string separator, string format, IFormatProvider formatProvider)
            StringBuilder returnValue = new StringBuilder("@{");
            bool isFirst = true;
            foreach (PSPropertyInfo property in mshObj.Properties)
                if (!isFirst)
                    returnValue.Append("; ");
                isFirst = false;
                // Don't evaluate script properties during a ToString() operation.
                var propertyValue = property is PSScriptProperty ? property.GetType().FullName : property.Value;
                returnValue.Append(PSObject.ToString(context, propertyValue, separator, format, formatProvider, false, false));
            if (isFirst)
        /// Returns the string representation of obj.
        /// <param name="context">ExecutionContext used to fetch the separator.</param>
        /// object we are trying to call ToString on. If this is not an PSObject we try
        /// enumerating and if that fails we call obj.ToString.
        /// If this is an PSObject, we look for a brokered ToString.
        /// If it is not present, and the BaseObject is null we try listing the properties.
        /// If the BaseObject is not null we try enumerating. If that fails we try the BaseObject's ToString.
        /// <returns>A string representation of the object.</returns>
        /// When there is a brokered ToString but it failed, or when the ToString on obj throws an exception.
        internal static string ToStringParser(ExecutionContext context, object obj)
            return ToStringParser(context, obj, CultureInfo.InvariantCulture);
        /// <param name="formatProvider">The formatProvider to be passed to ToString.</param>
        internal static string ToStringParser(ExecutionContext context, object obj, IFormatProvider formatProvider)
                return ToString(context, obj, null, null, formatProvider, true, true);
            catch (ExtendedTypeSystemException etse)
                throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", etse.InnerException,
        /// Called from an PSObject instance ToString to provide a string representation for an object.
        /// ExecutionContext used to fetch the separator.
        /// Typically either this or separator will be null.
        /// If both are null, " " is used.
        /// <param name="separator">The separator between elements, if this is an enumeration.</param>
        /// <param name="format">The format to be passed to ToString.</param>
        /// <param name="recurse">true if we should enumerate values or properties which would cause recursive
        /// calls to this method. Such recursive calls will have recurse set to false, limiting the depth.</param>
        /// <param name="unravelEnumeratorOnRecurse">If recurse is false, this parameter is not considered. If it is true
        /// this parameter will determine how enumerators are going to be treated.
        internal static string ToString(ExecutionContext context, object obj, string separator, string format, IFormatProvider formatProvider, bool recurse, bool unravelEnumeratorOnRecurse)
            bool TryFastTrackPrimitiveTypes(object value, out string str)
                switch (Convert.GetTypeCode(value))
                    case TypeCode.String:
                        str = (string)value;
                        var formattable = (IFormattable)value;
                        str = formattable.ToString(format, formatProvider);
                        var dbl = (double)value;
                        str = dbl.ToString(format ?? LanguagePrimitives.DoublePrecision, formatProvider);
                        var sgl = (float)value;
                        str = sgl.ToString(format ?? LanguagePrimitives.SinglePrecision, formatProvider);
                        str = null;
            #region plain object
            if (mshObj == null)
                if (TryFastTrackPrimitiveTypes(obj, out string objString))
                    return objString;
                #region recurse
                    IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj);
                            return ToStringEnumerable(context, enumerable, separator, format, formatProvider);
                            // We do want to ignore exceptions here to try the regular ToString below.
                    if (unravelEnumeratorOnRecurse)
                        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
                                return ToStringEnumerator(context, enumerator, separator, format, formatProvider);
                #endregion recurse
                #region object ToString
                IFormattable objFormattable = obj as IFormattable;
                    if (objFormattable == null)
                        Type type = obj as Type;
                            return Microsoft.PowerShell.ToStringCodeMethods.Type(type);
                    return objFormattable.ToString(format, formatProvider);
                    throw new ExtendedTypeSystemException("ToStringObjectBasicException", e,
                        ExtendedTypeSystem.ToStringException, e.Message);
                #endregion object ToString
            #endregion plain object
            #region PSObject
            #region brokered ToString
            // A brokered ToString has precedence over any other attempts.
            // If it fails we let the exception go because the caller must be notified.
            PSMethodInfo method = null;
            if (PSObject.HasInstanceMembers(mshObj, out PSMemberInfoInternalCollection<PSMemberInfo> instanceMembers))
                method = instanceMembers["ToString"] as PSMethodInfo;
                if (mshObj.InternalTypeNames.Count != 0)
                    TypeTable table = mshObj.GetTypeTable();
                    if (table != null)
                        method = table.GetMembers<PSMethodInfo>(mshObj.InternalTypeNames)["ToString"];
                            method = (PSMethodInfo)method.Copy();
                            method.instance = mshObj;
                    // Even if a format specifier has been provided, if there is only one overload
                    // then it can't take a format specified...
                    object retObj;
                    if (formatProvider != null && method.OverloadDefinitions.Count > 1)
                        retObj = method.Invoke(format, formatProvider);
                        return retObj != null ? retObj.ToString() : string.Empty;
                    retObj = method.Invoke();
                    throw new ExtendedTypeSystemException("MethodExceptionNullFormatProvider", e,
            #endregion brokered ToString
            // Since we don't have a brokered ToString, we check for the need to enumerate the object or its properties
                        return PSObject.ToStringEmptyBaseObject(context, mshObj, separator, format, formatProvider);
                IEnumerable enumerable = LanguagePrimitives.GetEnumerable(mshObj);
                    IEnumerator enumerator = LanguagePrimitives.GetEnumerator(mshObj);
            #region mshObject's BaseObject ToString
            // If we've cached a string representation for this object, use that. This
            // is used to preserve the original string for numeric literals.
            if (mshObj.TokenText != null)
                return mshObj.TokenText;
            // Since we don't have a brokered ToString and the enumerations were not necessary or failed
            // we try the BaseObject's ToString
            object baseObject = mshObj._immediateBaseObject;
            if (TryFastTrackPrimitiveTypes(baseObject, out string baseObjString))
                return baseObjString;
            IFormattable msjObjFormattable = baseObject as IFormattable;
                var result = msjObjFormattable == null ? baseObject.ToString() : msjObjFormattable.ToString(format, formatProvider);
                return result ?? string.Empty;
                throw new ExtendedTypeSystemException("ToStringPSObjectBasicException", e,
            #endregion mshObject's BaseObject ToString
            #endregion PSObject
        /// Returns the string representation for this object. A ToString
        /// CodeMethod or ScriptMethod will be used, if available. Enumerations items are
        /// concatenated using $ofs.
        /// <returns>The string representation for baseObject.</returns>
        /// <exception cref="ExtendedTypeSystemException">If an exception was thrown by the BaseObject's ToString.</exception>
            // If ToString value from deserialization is available,
            // simply return it.
            if (ToStringFromDeserialization != null)
                return ToStringFromDeserialization;
            return PSObject.ToString(null, this, null, null, null, true, false);
        /// <param name="format">Repassed to baseObject's IFormattable if present.</param>
        /// <param name="formatProvider">Repassed to baseObject's IFormattable if present.</param>
        public string ToString(string format, IFormatProvider formatProvider)
            return PSObject.ToString(null, this, null, format, formatProvider, true, false);
        private string PrivateToString()
                result = this.ToString();
                result = this.BaseObject.GetType().FullName;
        #region Clone
        /// Returns a copy of this PSObject. This will copy the BaseObject if
        /// it is a value type, and use BaseObject.Clone() for the new PSObject,
        /// if the BaseObject is ICloneable.
        /// <returns>A copy of this object.</returns>
        public virtual PSObject Copy()
            PSObject returnValue = (PSObject)this.MemberwiseClone();
            if (this.BaseObject is PSCustomObject)
                returnValue._immediateBaseObject = PSCustomObject.SelfInstance;
                returnValue.ImmediateBaseObjectIsEmpty = true;
                returnValue._immediateBaseObject = _immediateBaseObject;
                returnValue.ImmediateBaseObjectIsEmpty = false;
            // Instance members will be recovered as necessary through the resurrection table.
            returnValue._instanceMembers = null;
            // The typename is not resurrected.  A common reason to copy a PSObject is to change the TypeName,
            // especially on a PSCustomObject - e.g. to a help object where we want to force a different view.
            returnValue._typeNames = null;
            returnValue._members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(returnValue, s_memberCollection);
            returnValue._properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(returnValue, s_propertyCollection);
            returnValue._methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(returnValue, s_methodCollection);
            returnValue._adapterSet = GetMappedAdapter(returnValue._immediateBaseObject, returnValue.GetTypeTable());
            if (returnValue._immediateBaseObject is ICloneable cloneableBase)
                returnValue._immediateBaseObject = cloneableBase.Clone();
            if (returnValue._immediateBaseObject is ValueType)
                returnValue._immediateBaseObject = CopyValueType(returnValue._immediateBaseObject);
            // needToReAddInstanceMembersAndTypeNames = returnValue will have a different key (different from "this") returned from GetKeyForResurrectionTables
            bool needToReAddInstanceMembersAndTypeNames = !object.ReferenceEquals(GetKeyForResurrectionTables(this), GetKeyForResurrectionTables(returnValue));
            if (needToReAddInstanceMembersAndTypeNames)
                Diagnostics.Assert(returnValue.InstanceMembers.Count == 0, "needToReAddInstanceMembersAndTypeNames should mean that the new object has a fresh/empty list of instance members");
                foreach (PSMemberInfo member in this.InstanceMembers)
                    // Add will clone the member
                    returnValue.Members.Add(member);
                returnValue.TypeNames.Clear();
                foreach (string typeName in this.InternalTypeNames)
                    returnValue.TypeNames.Add(typeName);
            returnValue.WriteStream = WriteStream;
            returnValue.HasGeneratedReservedMembers = false;
        internal static object CopyValueType(object obj)
            // this will force boxing and unboxing in a new object that will cause
            // the copy of the value type
            var newBaseArray = Array.CreateInstance(obj.GetType(), 1);
            newBaseArray.SetValue(obj, 0);
            return newBaseArray.GetValue(0);
        #endregion Clone
        #region IComparable
        /// Compares the current instance with another object of the same type.
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the comparands.
        /// The return value has these meanings:
        ///     Value             Meaning
        ///     Less than zero    This instance is less than obj.
        ///     Zero              This instance is equal to obj.
        ///     Greater than zero This instance is greater than obj.
        /// <exception cref="ExtendedTypeSystemException"> If <paramref name="obj"/> has a different type
        /// than this instance's BaseObject or if the BaseObject does not implement IComparable.
            // This ReferenceEquals is not just an optimization.
            // It is necessary so that mshObject.Equals(mshObject) returns 0.
            // Please see the comments inside the Equals implementation.
            if (object.ReferenceEquals(this, obj))
                // PSObject.Base instead of BaseObject could cause an infinite
                // loop with LP.Compare calling this Compare.
                return LanguagePrimitives.Compare(this.BaseObject, obj);
                throw new ExtendedTypeSystemException("PSObjectCompareTo", e,
                    ExtendedTypeSystem.NotTheSameTypeOrNotIcomparable, this.PrivateToString(), PSObject.AsPSObject(obj).ToString(), "IComparable");
        #endregion IComparable
        #region Equals and GetHashCode
        /// Determines whether the specified Object is equal to the current Object.
        /// <param name="obj">The Object to compare with the current Object.</param>
        /// <returns>True if the specified Object is equal to the current Object; otherwise, false.</returns>
            // There is a slight difference between BaseObject and PSObject.Base.
            // PSObject.Base returns the containing PSObject that wraps an MshCustomBaseObject.
            // BaseObject returns the MshCustomBaseObject.
            // Because we have to call BaseObject here, and LP.Compare uses PSObject.Base
            // we need the reference equals below so that mshObject.Equals(mshObject) returns true.
            // The above check validates if we are comparing with the same object references
            // This check "shortcuts" the comparison if the first object is a CustomObject
            // since 2 custom objects are not equal.
            if (object.ReferenceEquals(this.BaseObject, PSCustomObject.SelfInstance))
            // loop with LP.Equals calling this Equals.
            return LanguagePrimitives.Equals(this.BaseObject, obj);
        /// Serves as a hash function for a particular type, suitable for use in hashing algorithms and data structures like a hash table.
        /// <returns>A hash code for the current Object.</returns>
            return this.BaseObject.GetHashCode();
        #endregion Equals and GetHashCode
        internal void AddOrSetProperty(string memberName, object value)
            if (PSGetMemberBinder.TryGetInstanceMember(this, memberName, out PSMemberInfo memberInfo) && memberInfo is PSPropertyInfo)
                memberInfo.Value = value;
                Properties.Add(new PSNoteProperty(memberName, value));
        internal void AddOrSetProperty(PSNoteProperty property)
            if (PSGetMemberBinder.TryGetInstanceMember(this, property.Name, out PSMemberInfo memberInfo) && memberInfo is PSPropertyInfo)
                memberInfo.Value = property.Value;
                Properties.Add(property);
        #endregion instance methods
        #region public const strings
        /// The name of the member set for adapted members.
        /// This needs to be Lower cased as it saves some comparison time elsewhere.
        public const string AdaptedMemberSetName = "psadapted";
        /// The name of the member set for extended members.
        public const string ExtendedMemberSetName = "psextended";
        /// The name of the member set for the BaseObject's members.
        public const string BaseObjectMemberSetName = "psbase";
        /// The PSObject's properties.
        internal const string PSObjectMemberSetName = "psobject";
        /// A shortcut to .PSObject.TypeNames.
        internal const string PSTypeNames = "pstypenames";
        #endregion public const strings
        #region serialization
        /// Implements the ISerializable contract for serializing a PSObject.
            // We create a wrapper PSObject, so that we can successfully deserialize it
            string serializedContent;
            if (this.ImmediateBaseObjectIsEmpty)
                PSObject serializeTarget = new PSObject(this);
                serializedContent = PSSerializer.Serialize(serializeTarget);
                serializedContent = PSSerializer.Serialize(this);
            info.AddValue("CliXml", serializedContent);
        /// <param name="settings"></param>
        /// <param name="noteName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="expectedType"></param>
        /// <param name="shouldReplicateInstance">
        /// true to make this PSObject as the owner of the memberset.
        /// <param name="ownerObject">
        /// PSObject to be used while replicating the owner for PSMemberSet
        internal static object GetNoteSettingValue(PSMemberSet settings, string noteName,
            object defaultValue, Type expectedType,
            bool shouldReplicateInstance, PSObject ownerObject)
            if (settings == null)
                return defaultValue;
            if (shouldReplicateInstance)
                settings.ReplicateInstance(ownerObject);
            if (settings.Members[noteName] is not PSNoteProperty note)
            object noteValue = note.Value;
            if (noteValue == null || noteValue.GetType() != expectedType)
            return note.Value;
        internal int GetSerializationDepth(TypeTable backupTypeTable)
            TypeTable typeTable = backupTypeTable ?? this.GetTypeTable();
                PSMemberSet standardMemberSet = TypeTableGetMemberDelegate<PSMemberSet>(this,
                    typeTable, TypeTable.PSStandardMembers);
                result = (int)GetNoteSettingValue(standardMemberSet, TypeTable.SerializationDepth, 0, typeof(int), true, this);
        /// TypeTable to use if this PSObject.GetTypeTable() returns null. This will happen
        /// in the remoting scenario on the client/server side (where a LocalRunspace may not be
        /// present).
        internal PSPropertyInfo GetStringSerializationSource(TypeTable backupTypeTable)
            PSMemberInfo result = this.GetPSStandardMember(backupTypeTable, TypeTable.StringSerializationSource);
            return result as PSPropertyInfo;
        internal SerializationMethod GetSerializationMethod(TypeTable backupTypeTable)
            SerializationMethod result = TypeTable.DefaultSerializationMethod;
                result = (SerializationMethod)GetNoteSettingValue(standardMemberSet,
                        TypeTable.SerializationMethodNode, TypeTable.DefaultSerializationMethod, typeof(SerializationMethod), true, this);
        internal PSMemberSet PSStandardMembers
                var retVal = TypeTableGetMemberDelegate<PSMemberSet>(this, TypeTable.PSStandardMembers);
                if (retVal != null)
                    retVal = (PSMemberSet)retVal.Copy();
                    retVal.ReplicateInstance(this);
                retVal = this.InstanceMembers[TypeTable.PSStandardMembers] as PSMemberSet;
        internal PSMemberInfo GetPSStandardMember(TypeTable backupTypeTable, string memberName)
            PSMemberInfo result = null;
                PSMemberSet standardMemberSet = TypeTableGetMemberDelegate<PSMemberSet>(
                    this, typeTable, TypeTable.PSStandardMembers);
                if (standardMemberSet != null)
                    standardMemberSet.ReplicateInstance(this);
                    PSMemberInfoIntegratingCollection<PSMemberInfo> members =
                        new PSMemberInfoIntegratingCollection<PSMemberInfo>(
                            standardMemberSet,
                            GetMemberCollection(PSMemberViewTypes.All, backupTypeTable));
                    result = members[memberName];
            return result ?? InstanceMembers[TypeTable.PSStandardMembers] as PSMemberSet;
        /// Used by Deserializer to deserialize a serialized object to a given type
        /// (as specified in the a types.ps1xml file)
        internal Type GetTargetTypeForDeserialization(TypeTable backupTypeTable)
            PSMemberInfo targetType = this.GetPSStandardMember(backupTypeTable, TypeTable.TargetTypeForDeserialization);
            return targetType?.Value as Type;
        /// This is only going to be called if SerializationMethod is SpecificProperties.
        /// in the remoting scenario on the client side (where a LocalRunspace may not be
        /// <returns>A collection with only the specific properties to serialize.</returns>
        internal Collection<string> GetSpecificPropertiesToSerialize(TypeTable backupTypeTable)
                Collection<string> tmp = typeTable.GetSpecificProperties(this.InternalTypeNames);
                return tmp;
            return new Collection<string>(new List<string>());
        internal bool ShouldSerializeAdapter()
            if (this.IsDeserialized)
                return this.AdaptedMembers != null;
            return !this.ImmediateBaseObjectIsEmpty;
        internal PSMemberInfoInternalCollection<PSPropertyInfo> GetAdaptedProperties()
            return GetProperties(this.AdaptedMembers, this.InternalAdapter);
        private PSMemberInfoInternalCollection<PSPropertyInfo> GetProperties(PSMemberInfoInternalCollection<PSPropertyInfo> serializedMembers, Adapter particularAdapter)
                return serializedMembers;
            PSMemberInfoInternalCollection<PSPropertyInfo> returnValue = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            foreach (PSPropertyInfo member in particularAdapter.BaseGetMembers<PSPropertyInfo>(_immediateBaseObject))
        internal static void CopyDeserializerFields(PSObject source, PSObject target)
            if (!target.IsDeserialized)
                target.IsDeserialized = source.IsDeserialized;
                target.AdaptedMembers = source.AdaptedMembers;
                target.ClrMembers = source.ClrMembers;
            if (target.ToStringFromDeserialization == null)
                target.ToStringFromDeserialization = source.ToStringFromDeserialization;
                target.TokenText = source.TokenText;
        /// Set base object.
        /// <param name="value">Object which is set as core.</param>
        /// <param name="overrideTypeInfo">If true, overwrite the type information.</param>
        /// <remarks>This method is to be used only by Serialization code</remarks>
        internal void SetCoreOnDeserialization(object value, bool overrideTypeInfo)
            Diagnostics.Assert(this.ImmediateBaseObjectIsEmpty, "BaseObject should be PSCustomObject for deserialized objects");
            Diagnostics.Assert(value != null, "known objects are never null");
            this.ImmediateBaseObjectIsEmpty = false;
            _immediateBaseObject = value;
            _adapterSet = GetMappedAdapter(_immediateBaseObject, GetTypeTable());
            if (overrideTypeInfo)
                this.InternalTypeNames = this.InternalAdapter.BaseGetTypeNameHierarchy(value);
        #endregion serialization
        /// This class is solely used by PSObject to support .net member lookup for all the
        /// adapters except for dotNetInstanceAdapter, mshMemberSetAdapter and mshObjectAdapter.
        /// If the original adapter is not one of those, then .net members are also exposed
        /// on the PSObject. This will have the following effect:
        /// 1. Every adapted object like xml, wmi, adsi will show adapted members as well as
        ///    .net members.
        /// 2. Users will not need to access PSBase to access original .net members.
        /// 3. This will fix v1.0 ADSI adapter where most of the complaints were about
        ///    discovering original .net members.
        /// Use of this class will allow us to customize the ultimate .net member lookup.
        /// For example, XML adapter already exposes .net methods.
        /// Using this class you can choose exact .net adapter to support .net
        /// member lookup and avoid lookup duplication.
        internal class AdapterSet
            // original adapter like Xml, ManagementClass, DirectoryEntry etc.
            // .net adapter
            /// This property can be accessed only internally and hence
            /// no checks are performed on input.
            internal Adapter OriginalAdapter { get; set; }
            internal DotNetAdapter DotNetAdapter { get; }
            internal AdapterSet(Adapter adapter, DotNetAdapter dotnetAdapter)
                OriginalAdapter = adapter;
                DotNetAdapter = dotnetAdapter;
        #region Dynamic metaobject implementation
        internal class PSDynamicMetaObject : DynamicMetaObject
            internal PSDynamicMetaObject(Expression expression, PSObject value)
                : base(expression, BindingRestrictions.Empty, value)
            private new PSObject Value => (PSObject)base.Value;
            private DynamicMetaObject GetUnwrappedObject()
                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.PSObject_Base, this.Expression), this.Restrictions, PSObject.Base(Value));
            public override IEnumerable<string> GetDynamicMemberNames()
                return (from member in Value.Members select member.Name);
            private bool MustDeferIDMOP()
                var baseObject = PSObject.Base(Value);
                return baseObject is IDynamicMetaObjectProvider && baseObject is not PSObject;
            private DynamicMetaObject DeferForIDMOP(DynamicMetaObjectBinder binder, params DynamicMetaObject[] args)
                Diagnostics.Assert(MustDeferIDMOP(), "Defer only works for idmop wrapped PSObjects");
                Expression[] exprs = new Expression[args.Length + 1];
                BindingRestrictions restrictions = this.Restrictions == BindingRestrictions.Empty ? this.PSGetTypeRestriction() : this.Restrictions;
                exprs[0] = Expression.Call(CachedReflectionInfo.PSObject_Base, this.Expression.Cast(typeof(object)));
                    exprs[i + 1] = args[i].Expression;
                    restrictions = restrictions.Merge(args[i].Restrictions == BindingRestrictions.Empty ? args[i].PSGetTypeRestriction() : args[i].Restrictions);
                return new DynamicMetaObject(DynamicExpression.Dynamic(binder, binder.ReturnType, exprs), restrictions);
            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
                if (MustDeferIDMOP())
                    return DeferForIDMOP(binder, arg);
                return binder.FallbackBinaryOperation(GetUnwrappedObject(), arg);
            public override DynamicMetaObject BindConvert(ConvertBinder binder)
                    return DeferForIDMOP(binder);
                // This will invoke the language binder, meaning we might not invoke PowerShell conversions.  This
                // is an interesting design choice, which, if revisited, needs some care because there are multiple
                // binders that PowerShell uses for conversion, the normal one, and the one used when we want to
                // attempt enumeration of some object that might or might not implement IEnumerable.
                return binder.FallbackConvert(this);
            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
                    return DeferForIDMOP(binder, indexes);
                return binder.FallbackDeleteIndex(GetUnwrappedObject(), indexes);
            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
                return binder.FallbackDeleteMember(GetUnwrappedObject());
            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
                return binder.FallbackGetIndex(GetUnwrappedObject(), indexes);
            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
                    return DeferForIDMOP(binder, args);
                return binder.FallbackInvoke(GetUnwrappedObject(), args);
            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
                    return DeferForIDMOP(binder, indexes.Append(value).ToArray());
                return binder.FallbackSetIndex(GetUnwrappedObject(), indexes, value);
            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
                return binder.FallbackUnaryOperation(GetUnwrappedObject());
            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
                return (binder as PSInvokeMemberBinder ??
                        (InvokeMemberBinder)(binder as PSInvokeBaseCtorBinder) ??
                        PSInvokeMemberBinder.Get(binder.Name, binder.CallInfo, false, false, null, null)).FallbackInvokeMember(this, args);
            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
                return (binder as PSGetMemberBinder ?? PSGetMemberBinder.Get(binder.Name, (Type)null, false)).FallbackGetMember(this);
            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
                    return DeferForIDMOP(binder, value);
                return (binder as PSSetMemberBinder ?? PSSetMemberBinder.Get(binder.Name, (Type)null, false)).FallbackSetMember(this, value);
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
            return new PSDynamicMetaObject(parameter, this);
        internal bool IsDeserialized
            get => _flags.HasFlag(PSObjectFlags.IsDeserialized);
                    _flags |= PSObjectFlags.IsDeserialized;
                    _flags &= ~PSObjectFlags.IsDeserialized;
        private bool StoreTypeNameAndInstanceMembersLocally
            get => _flags.HasFlag(PSObjectFlags.StoreTypeNameAndInstanceMembersLocally);
                    _flags |= PSObjectFlags.StoreTypeNameAndInstanceMembersLocally;
                    _flags &= ~PSObjectFlags.StoreTypeNameAndInstanceMembersLocally;
        internal bool IsHelpObject
            get => _flags.HasFlag(PSObjectFlags.IsHelpObject);
                    _flags |= PSObjectFlags.IsHelpObject;
                    _flags &= ~PSObjectFlags.IsHelpObject;
        internal bool HasGeneratedReservedMembers
            get => _flags.HasFlag(PSObjectFlags.HasGeneratedReservedMembers);
                    _flags |= PSObjectFlags.HasGeneratedReservedMembers;
                    _flags &= ~PSObjectFlags.HasGeneratedReservedMembers;
        internal bool ImmediateBaseObjectIsEmpty
            get => _flags.HasFlag(PSObjectFlags.ImmediateBaseObjectIsEmpty);
                    _flags |= PSObjectFlags.ImmediateBaseObjectIsEmpty;
                    _flags &= ~PSObjectFlags.ImmediateBaseObjectIsEmpty;
        /// If 'this' is non-null, return this string as the ToString() for this wrapped object.
        internal string TokenText { get; set; }
        /// Sets the 'ToString' value on deserialization.
        internal string ToStringFromDeserialization { get; set; }
        /// This property contains a stream type used by the formatting system.
        internal WriteStreamType WriteStream { get; set; }
        /// Members from the adapter of the object before it was serialized
        /// Null for live objects but not null for deserialized objects.
        internal PSMemberInfoInternalCollection<PSPropertyInfo> AdaptedMembers { get; set; }
        internal static DotNetAdapter DotNetStaticAdapter => s_dotNetStaticAdapter;
        internal static PSTraceSource MemberResolution => s_memberResolution;
        internal PSMemberInfoInternalCollection<PSPropertyInfo> ClrMembers { get; set; }
        internal static DotNetAdapter DotNetInstanceAdapter => s_dotNetInstanceAdapter;
        /// Gets an instance member if it's name matches the predicate. Otherwise null.
        internal PSPropertyInfo GetFirstPropertyOrDefault(MemberNamePredicate predicate)
            return Properties.FirstOrDefault(predicate);
        private enum PSObjectFlags : byte
            /// This flag is set in deserialized shellobject.
            IsDeserialized = 0b00000001,
            /// Set to true when the BaseObject is PSCustomObject.
            HasGeneratedReservedMembers = 0b00000010,
            ImmediateBaseObjectIsEmpty = 0b00000100,
            IsHelpObject = 0b00001000,
            /// Indicate whether we store the instance members and type names locally
            StoreTypeNameAndInstanceMembersLocally = 0b00010000,
    /// Specifies special stream write processing.
    internal enum WriteStreamType : byte
        Output,
        Warning,
        Verbose,
        Debug,
        Information
    /// Serves as a placeholder BaseObject when PSObject's
    /// constructor with no parameters is used.
    public class PSCustomObject
        /// To prevent other instances than SelfInstance.
        private PSCustomObject() { }
        internal static readonly PSCustomObject SelfInstance = new PSCustomObject();
        /// Returns an empty string.
    /// Please keep in sync with SerializationMethod from
    /// C:\e\win7_powershell\admin\monad\nttargets\assemblies\logging\ETW\Manifests\Microsoft-Windows-PowerShell-Instrumentation.man
    internal enum SerializationMethod
        AllPublicProperties = 0,
        String = 1,
        SpecificProperties = 2
        private static void AddGenericArguments(StringBuilder sb, Type[] genericArguments, bool dropNamespaces)
            for (int i = 0; i < genericArguments.Length; i++)
                if (i > 0) { sb.Append(','); }
                sb.Append(Type(genericArguments[i], dropNamespaces));
        internal static string Type(Type type, bool dropNamespaces = false, string key = null)
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                string genericDefinition = Type(type.GetGenericTypeDefinition(), dropNamespaces);
                // For regular generic types, we find the backtick character, for example:
                //      System.Collections.Generic.List`1[T] ->
                //      System.Collections.Generic.List[string]
                // For nested generic types, we find the left bracket character, for example:
                //      System.Collections.Generic.Dictionary`2+Enumerator[TKey, TValue] ->
                //      System.Collections.Generic.Dictionary`2+Enumerator[string,string]
                int backtickOrLeftBracketIndex = genericDefinition.LastIndexOf(type.IsNested ? '[' : '`');
                var sb = new StringBuilder(genericDefinition, 0, backtickOrLeftBracketIndex, 512);
                AddGenericArguments(sb, type.GetGenericArguments(), dropNamespaces);
                result = sb.ToString();
            else if (type.IsArray)
                string elementDefinition = Type(type.GetElementType(), dropNamespaces);
                var sb = new StringBuilder(elementDefinition, elementDefinition.Length + 10);
                for (int i = 0; i < type.GetArrayRank() - 1; ++i)
                result = TypeAccelerators.FindBuiltinAccelerator(type, key);
                    if (type == typeof(PSCustomObject))
                        return type.Name;
                    if (dropNamespaces)
                        if (type.IsNested)
                            // For nested types, we should return OuterType+InnerType. For example,
                            //  System.Environment+SpecialFolder ->  Environment+SpecialFolder
                            string fullName = type.ToString();
                            result = type.Namespace == null
                                        ? fullName
                                        : fullName.Substring(type.Namespace.Length + 1);
                            result = type.Name;
                        result = type.ToString();
            // We can't round trip anything with a generic parameter.
            // We also can't round trip if we're dropping the namespace.
            if (!type.IsGenericParameter
                && !type.ContainsGenericParameters
                && !dropNamespaces
                && !type.Assembly.GetCustomAttributes(typeof(DynamicClassImplementationAssemblyAttribute)).Any())
                TypeResolver.TryResolveType(result, out Type roundTripType);
                if (roundTripType != type)
                    result = type.AssemblyQualifiedName;
        /// ToString implementation for Type.
        /// <param name="instance">Instance of PSObject wrapping a Type.</param>
        public static string Type(PSObject instance)
            return Type((Type)instance.BaseObject);
        /// ToString implementation for XmlNode.
        /// <param name="instance">Instance of PSObject wrapping an XmlNode.</param>
        public static string XmlNode(PSObject instance)
            XmlNode node = (XmlNode)instance?.BaseObject;
            return node.LocalName;
        /// ToString implementation for XmlNodeList.
        /// <param name="instance">Instance of PSObject wrapping an XmlNodeList.</param>
        public static string XmlNodeList(PSObject instance)
            XmlNodeList nodes = (XmlNodeList)instance?.BaseObject;
            if (nodes.Count == 1)
                if (nodes[0] == null)
                return PSObject.AsPSObject(nodes[0]).ToString();
            return PSObject.ToStringEnumerable(context: null, enumerable: nodes, separator: null, format: null, formatProvider: null);
