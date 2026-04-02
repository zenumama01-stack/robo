namespace System.Management.Automation
    /// This class provides functionality for serializing a PSObject.
    internal sealed class CustomSerialization
        /// Depth of serialization.
        private readonly int _depth;
        /// XmlWriter to be used for writing.
        private readonly XmlWriter _writer;
        /// Whether type information should be included in the xml.
        private readonly bool _notypeinformation;
        /// CustomerSerializer used for formatting the output for _writer.
        private CustomInternalSerializer _serializer;
        /// Initializes a new instance of the <see cref="CustomSerialization"/> class.
        /// <param name="writer">
        /// writer to be used for serialization.
        /// <param name="notypeinformation">
        /// should the type information to be shown.
        /// <param name="depth">
        /// depth to be used for serialization. If this value is specified,
        /// depth from types.xml is not used.
        internal CustomSerialization(XmlWriter writer, bool notypeinformation, int depth)
            if (writer == null)
                throw PSTraceSource.NewArgumentException(nameof(writer));
            if (depth < 1)
                throw PSTraceSource.NewArgumentException(nameof(writer), Serialization.DepthOfOneRequired);
            _depth = depth;
            _writer = writer;
            _notypeinformation = notypeinformation;
            _serializer = null;
        /// Default depth of serialization.
        public static int MshDefaultSerializationDepth { get; } = 1;
        internal CustomSerialization(XmlWriter writer, bool notypeinformation)
            : this(writer, notypeinformation, MshDefaultSerializationDepth)
        private bool _firstCall = true;
        /// Serializes passed in object.
        /// <param name="source">
        /// Object to be serialized.
        internal void Serialize(object source)
            // Write the root element tag before writing first object.
            if (_firstCall)
                _firstCall = false;
            _serializer = new CustomInternalSerializer
                               (
                                   _writer,
                                   _notypeinformation,
                                   true
            _serializer.WriteOneObject(source, null, _depth);
        internal void SerializeAsStream(object source)
        /// Writes the start of root element.
        private void Start()
            CustomInternalSerializer.WriteStartElement(_writer, CustomSerializationStrings.RootElementTag);
        /// Write the end of root element.
        internal void Done()
            _writer.WriteEndElement();
            _writer.Flush();
        /// Flush the writer.
        internal void DoneAsStream()
        internal void Stop()
            CustomInternalSerializer serializer = _serializer;
            serializer?.Stop();
    /// This internal helper class provides methods for serializing mshObject.
    internal sealed class CustomInternalSerializer
        /// Xml writer to be used.
        /// Check first call for every pipeline object to write Object tag else property tag.
        private bool _firstcall;
        /// Should the type information to be shown.
        /// Check object call.
        private bool _firstobjectcall = true;
        /// Initializes a new instance of the <see cref="CustomInternalSerializer"/> class.
        /// <param name="isfirstcallforObject">
        internal CustomInternalSerializer(XmlWriter writer, bool notypeinformation, bool isfirstcallforObject)
            Dbg.Assert(writer != null, "caller should validate the parameter");
            _firstcall = isfirstcallforObject;
        #region Stopping
        private bool _isStopping = false;
        /// Called from a separate thread will stop the serialization process.
            _isStopping = true;
        private void CheckIfStopping()
            if (_isStopping)
                throw PSTraceSource.NewInvalidOperationException(Serialization.Stopping);
        #endregion Stopping
        /// This writes one object.
        /// source to be serialized.
        /// <param name="property">
        /// name of property. If null, name attribute is not written.
        /// depth to which this object should be serialized.
        internal void WriteOneObject(object source, string property, int depth)
            Dbg.Assert(depth >= 0, "depth should always be greater or equal to zero");
            CheckIfStopping();
                WriteNull(property);
            if (HandlePrimitiveKnownType(source, property))
            if (HandlePrimitiveKnownTypePSObject(source, property, depth))
            // Note: We donot use containers in depth calculation. i.e even if the
            // current depth is zero, we serialize the container. All contained items will
            // get serialized with depth zero.
            if (HandleKnownContainerTypes(source, property, depth))
            PSObject mshSource = PSObject.AsPSObject(source);
            // If depth is zero, complex type should be serialized as string.
            if (depth == 0 || SerializeAsString(mshSource))
                HandlePSObjectAsString(mshSource, property, depth);
            HandleComplexTypePSObject(mshSource, property, depth);
        /// Serializes Primitive Known Types.
        /// true if source is handled, else false.
        private bool HandlePrimitiveKnownType(object source, string property)
            Dbg.Assert(source != null, "caller should validate the parameter");
            // Check if source is of primitive known type
            TypeSerializationInfo pktInfo = KnownTypes.GetTypeSerializationInfo(source.GetType());
            if (pktInfo != null)
                WriteOnePrimitiveKnownType(_writer, property, source, pktInfo);
        /// Serializes PSObject whose base objects are of primitive known type.
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="depth"></param>
        private bool HandlePrimitiveKnownTypePSObject(object source, string property, int depth)
            bool sourceHandled = false;
            if (source is PSObject moSource && !moSource.ImmediateBaseObjectIsEmpty)
                // Check if baseObject is primitive known type
                object baseObject = moSource.ImmediateBaseObject;
                TypeSerializationInfo pktInfo = KnownTypes.GetTypeSerializationInfo(baseObject.GetType());
                    WriteOnePrimitiveKnownType(_writer, property, baseObject, pktInfo);
                    sourceHandled = true;
            return sourceHandled;
        private bool HandleKnownContainerTypes(object source, string property, int depth)
            ContainerType ct = ContainerType.None;
            PSObject mshSource = source as PSObject;
            IEnumerable enumerable = null;
            IDictionary dictionary = null;
            // If passed in object is PSObject with no baseobject, return false.
            if (mshSource != null && mshSource.ImmediateBaseObjectIsEmpty)
            // Check if source (or baseobject in mshSource) is known container type
            GetKnownContainerTypeInfo(mshSource != null ? mshSource.ImmediateBaseObject : source, out ct,
                                      out dictionary, out enumerable);
            if (ct == ContainerType.None)
            WriteStartOfPSObject(mshSource ?? PSObject.AsPSObject(source), property, true);
            switch (ct)
                case ContainerType.Dictionary:
                        WriteDictionary(dictionary, depth);
                case ContainerType.Stack:
                case ContainerType.Queue:
                case ContainerType.List:
                case ContainerType.Enumerable:
                        WriteEnumerable(enumerable, depth);
                        Dbg.Assert(false, "All containers should be handled in the switch");
            // An object which is original enumerable becomes an PSObject
            // with arraylist on deserialization. So on roundtrip it will show up
            // as List.
            // We serialize properties of enumerable and on deserialization mark the object
            // as Deserialized. So if object is marked deserialized, we should write properties.
            // Note: we do not serialize the properties of IEnumerable if depth is zero.
            if (depth != 0 && (ct == ContainerType.Enumerable || (mshSource != null && mshSource.IsDeserialized)))
                // Note:Depth is the depth for serialization of baseObject.
                // Depth for serialization of each property is one less.
                WritePSObjectProperties(PSObject.AsPSObject(source), depth);
            // If source is PSObject, serialize notes
            if (mshSource != null)
                // Serialize instanceMembers
                PSMemberInfoCollection<PSMemberInfo> instanceMembers = mshSource.InstanceMembers;
                if (instanceMembers != null)
                    WriteMemberInfoCollection(instanceMembers, depth, true);
        /// Checks if source is known container type and returns appropriate information.
        /// <param name="ct"></param>
        /// <param name="dictionary"></param>
        /// <param name="enumerable"></param>
        private static void GetKnownContainerTypeInfo(
            object source, out ContainerType ct, out IDictionary dictionary, out IEnumerable enumerable)
            ct = ContainerType.None;
            dictionary = null;
            enumerable = null;
            dictionary = source as IDictionary;
            if (dictionary != null)
                ct = ContainerType.Dictionary;
            if (source is Stack)
                ct = ContainerType.Stack;
                enumerable = LanguagePrimitives.GetEnumerable(source);
                Dbg.Assert(enumerable != null, "Stack is enumerable");
            else if (source is Queue)
                ct = ContainerType.Queue;
                Dbg.Assert(enumerable != null, "Queue is enumerable");
            else if (source is IList)
                ct = ContainerType.List;
                Dbg.Assert(enumerable != null, "IList is enumerable");
                Type gt = source.GetType();
                if (gt.GetTypeInfo().IsGenericType)
                    if (DerivesFromGenericType(gt, typeof(Stack<>)))
                    else if (DerivesFromGenericType(gt, typeof(Queue<>)))
                    else if (DerivesFromGenericType(gt, typeof(List<>)))
            // Check if type is IEnumerable
                    ct = ContainerType.Enumerable;
        /// Checks if derived is of type baseType or a type derived from baseType.
        /// <param name="derived"></param>
        /// <param name="baseType"></param>
        private static bool DerivesFromGenericType(Type derived, Type baseType)
            Dbg.Assert(derived != null, "caller should validate the parameter");
            Dbg.Assert(baseType != null, "caller should validate the parameter");
            while (derived != null)
                if (derived.GetTypeInfo().IsGenericType)
                    derived = derived.GetGenericTypeDefinition();
                if (derived == baseType)
                derived = derived.GetTypeInfo().BaseType;
        #region Write PSObject
        /// Serializes an PSObject whose baseobject is of primitive type.
        /// and which has notes.
        /// Source from which notes are written.
        /// <param name="primitive">
        /// primitive object which is written as base object. In most cases it
        /// is same source.ImmediateBaseObject. When PSObject is serialized as string,
        /// it can be different. <see cref="HandlePSObjectAsString"/> for more info.
        /// <param name="pktInfo">
        /// TypeSerializationInfo for the primitive.
        private void WritePrimitiveTypePSObjectWithNotes(
            PSObject source,
            object primitive,
            TypeSerializationInfo pktInfo,
            string property,
            int depth)
            // Write start of PSObject. Since baseobject is primitive known
            // type, we do not need TypeName information.
            WriteStartOfPSObject(source, property, source.ToStringFromDeserialization != null);
                WriteOnePrimitiveKnownType(_writer, null, primitive, pktInfo);
            PSMemberInfoCollection<PSMemberInfo> instanceMembers = source.InstanceMembers;
        private void HandleComplexTypePSObject(PSObject source, string property, int depth)
            WriteStartOfPSObject(source, property, true);
            // Figure out what kind of object we are dealing with
            bool isEnum = false;
            bool isPSObject = false;
            if (!source.ImmediateBaseObjectIsEmpty)
                isEnum = source.ImmediateBaseObject is Enum;
                isPSObject = source.ImmediateBaseObject is PSObject;
            if (isEnum)
                object baseObject = source.ImmediateBaseObject;
                foreach (PSPropertyInfo prop in source.Properties)
                    WriteOneObject(System.Convert.ChangeType(baseObject, Enum.GetUnderlyingType(baseObject.GetType()), System.Globalization.CultureInfo.InvariantCulture), prop.Name, depth);
            else if (isPSObject)
                if (_firstobjectcall)
                    _firstobjectcall = false;
                    WritePSObjectProperties(source, depth);
                    WriteOneObject(source.ImmediateBaseObject, null, depth);
        /// Writes start element, attributes and typeNames for PSObject.
        /// <param name="mshObject"></param>
        /// <param name="writeTNH">
        /// if true, TypeName information is written, else not.
        private void WriteStartOfPSObject(
            PSObject mshObject,
            bool writeTNH)
            Dbg.Assert(mshObject != null, "caller should validate the parameter");
                WriteStartElement(_writer, CustomSerializationStrings.Properties);
                WriteAttribute(_writer, CustomSerializationStrings.NameAttribute, property);
                if (_firstcall)
                    WriteStartElement(_writer, CustomSerializationStrings.PSObjectTag);
                    _firstcall = false;
            object baseObject = mshObject.BaseObject;
            if (!_notypeinformation)
                WriteAttribute(_writer, CustomSerializationStrings.TypeAttribute, baseObject.GetType().ToString());
        #region membersets
        /// Returns true if PSObject has notes.
        private static bool PSObjectHasNotes(PSObject source)
            if (source.InstanceMembers != null && source.InstanceMembers.Count > 0)
        /// Serialize member set. This method serializes without writing.
        /// enclosing tags and attributes.
        /// <param name="me">
        /// Enumerable containing members
        /// <param name="writeEnclosingMemberSetElementTag">
        /// if this is true, write an enclosing "<memberset></memberset>" tag.
        private void WriteMemberInfoCollection(
            PSMemberInfoCollection<PSMemberInfo> me, int depth, bool writeEnclosingMemberSetElementTag)
            Dbg.Assert(me != null, "caller should validate the parameter");
            foreach (PSMemberInfo info in me)
                if (!info.ShouldSerialize)
                if (info is not PSPropertyInfo property)
                WriteAttribute(_writer, CustomSerializationStrings.NameAttribute, info.Name);
                    WriteAttribute(_writer, CustomSerializationStrings.TypeAttribute, info.GetType().ToString());
                _writer.WriteString(property.Value.ToString());
        #endregion membersets
        /// Serializes properties of PSObject.
        private void WritePSObjectProperties(PSObject source, int depth)
            Dbg.Assert(source != null, "caller should validate the information");
            depth = GetDepthOfSerialization(source, depth);
            // Depth available for each property is one less
            --depth;
            Dbg.Assert(depth >= 0, "depth should be greater or equal to zero");
            if (source.GetSerializationMethod(null) == SerializationMethod.SpecificProperties)
                PSMemberInfoInternalCollection<PSPropertyInfo> specificProperties = new();
                foreach (string propertyName in source.GetSpecificPropertiesToSerialize(null))
                    PSPropertyInfo property = source.Properties[propertyName];
                        specificProperties.Add(property);
                SerializeProperties(specificProperties, CustomSerializationStrings.Properties, depth);
                Dbg.Assert(prop != null, "propertyCollection should only have member of type PSProperty");
                object value = AutomationNull.Value;
                // PSObject throws GetValueException if it cannot
                // get value for a property.
                    value = prop.Value;
                catch (GetValueException)
                    WritePropertyWithNullValue(_writer, prop, depth);
                // Write the property
                    WriteOneObject(value, prop.Name, depth);
        /// Serializes properties from collection.
        /// <param name="propertyCollection">
        /// Collection of properties to serialize.
        /// <param name="name">
        /// Name for enclosing element tag.
        /// Depth to which each property should be serialized.
        private void SerializeProperties(
            PSMemberInfoInternalCollection<PSPropertyInfo> propertyCollection, string name, int depth)
            Dbg.Assert(propertyCollection != null, "caller should validate the parameter");
            if (propertyCollection.Count == 0)
            foreach (PSMemberInfo info in propertyCollection)
                PSPropertyInfo prop = info as PSPropertyInfo;
        #endregion base properties
        #endregion WritePSObject
        #region enumerable and dictionary
        /// Serializes IEnumerable.
        /// <param name="enumerable">
        /// Enumerable which is serialized.
        private void WriteEnumerable(IEnumerable enumerable, int depth)
            Dbg.Assert(enumerable != null, "caller should validate the parameter");
            IEnumerator enumerator = null;
                enumerator = enumerable.GetEnumerator();
                enumerator.Reset();
                enumerator = null;
            // AD has incorrect implementation of IEnumerable where they returned null
            // for GetEnumerator instead of empty enumerator
            if (enumerator != null)
                    object item = null;
                        if (!enumerator.MoveNext())
                            item = enumerator.Current;
                    WriteOneObject(item, null, depth);
        /// Serializes IDictionary.
        /// <param name="dictionary">Dictionary which is serialized.</param>
        private void WriteDictionary(IDictionary dictionary, int depth)
            IDictionaryEnumerator dictionaryEnum = null;
                dictionaryEnum = (IDictionaryEnumerator)dictionary.GetEnumerator();
            if (dictionaryEnum != null)
                while (dictionaryEnum.MoveNext())
                    // Write Key
                    WriteOneObject(dictionaryEnum.Key, CustomSerializationStrings.DictionaryKey, depth);
                    // Write Value
                    WriteOneObject(dictionaryEnum.Value, CustomSerializationStrings.DictionaryValue, depth);
        #endregion enumerable and dictionary
        #region serialize as string
        private void HandlePSObjectAsString(PSObject source, string property, int depth)
            bool hasNotes = PSObjectHasNotes(source);
            string value = GetStringFromPSObject(source);
                TypeSerializationInfo pktInfo = KnownTypes.GetTypeSerializationInfo(value.GetType());
                Dbg.Assert(pktInfo != null, "TypeSerializationInfo should be present for string");
                if (hasNotes)
                    WritePrimitiveTypePSObjectWithNotes(source, value, pktInfo, property, depth);
                    WriteOnePrimitiveKnownType(_writer, property, source.BaseObject, pktInfo);
                    WritePrimitiveTypePSObjectWithNotes(source, null, null, property, depth);
        /// Gets the string from PSObject using the information from
        /// types.ps1xml. This string is used for serializing the PSObject.
        /// PSObject to be converted to string.
        /// string value to use for serializing this PSObject.
        private static string GetStringFromPSObject(PSObject source)
            Dbg.Assert(source != null, "caller should have validated the information");
            // check if we have a well known string serialization source
            PSPropertyInfo serializationProperty = source.GetStringSerializationSource(null);
            if (serializationProperty != null)
                object val = serializationProperty.Value;
                        // if we have a string serialization value, return it
                        result = val.ToString();
                    // fall back value
                    result = source.ToString();
        /// Reads the information the PSObject
        /// and returns true if this object should be serialized as string.
        /// <param name="source">PSObject to be serialized.</param>
        /// <returns>True if the object needs to be serialized as a string.</returns>
        private static bool SerializeAsString(PSObject source)
            return source.GetSerializationMethod(null) == SerializationMethod.String;
        #endregion serialize as string
        /// Compute the serialization depth for an PSObject instance subtree.
        /// <param name="source">PSObject whose serialization depth has to be computed.</param>
        /// <param name="depth">Current depth.</param>
        private static int GetDepthOfSerialization(PSObject source, int depth)
                return depth;
            // get the depth from the PSObject
            // NOTE: we assume that the depth out of the PSObject is > 0
            // else we consider it not set in types.ps1xml
            int objectLevelDepth = source.GetSerializationDepth(null);
            if (objectLevelDepth <= 0)
                // no override at the type level
            return objectLevelDepth;
        /// Writes null.
        private void WriteNull(string property)
        #region known type serialization
        private void WritePropertyWithNullValue(
            XmlWriter writer, PSPropertyInfo source, int depth)
            WriteStartElement(writer, CustomSerializationStrings.Properties);
            WriteAttribute(writer, CustomSerializationStrings.NameAttribute, ((PSPropertyInfo)source).Name);
                WriteAttribute(writer, CustomSerializationStrings.TypeAttribute, ((PSPropertyInfo)source).TypeNameOfValue);
            writer.WriteEndElement();
        private void WriteObjectString(
            XmlWriter writer, string property, object source, TypeSerializationInfo entry)
                WriteAttribute(writer, CustomSerializationStrings.NameAttribute, property);
                    WriteStartElement(writer, CustomSerializationStrings.PSObjectTag);
                WriteAttribute(writer, CustomSerializationStrings.TypeAttribute, source.GetType().ToString());
            writer.WriteString(source.ToString());
        /// Writes an item or property in Monad namespace.
        /// <param name="writer">The XmlWriter stream to which the object is serialized.</param>
        /// <param name="property">Name of property. Pass null for item.</param>
        /// <param name="source">Object to be written.</param>
        /// <param name="entry">Serialization information about source.</param>
        private void WriteOnePrimitiveKnownType(
            WriteObjectString(writer, property, source, entry);
        #endregion known type serialization
        #region misc
        /// Writes start element in Monad namespace.
        /// <param name="writer"></param>
        /// <param name="elementTag">Tag of element.</param>
        internal static void WriteStartElement(XmlWriter writer, string elementTag)
            writer.WriteStartElement(elementTag);
        /// Writes attribute in monad namespace.
        /// <param name="name">Name of attribute.</param>
        /// <param name="value">Value of attribute.</param>
        internal static void WriteAttribute(XmlWriter writer, string name, string value)
            writer.WriteAttributeString(name, value);
        #endregion misc
