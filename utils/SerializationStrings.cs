    /// This class contains strings required for serialization.
    internal static class SerializationStrings
        internal const string RootElementTag = "Objs";
        internal const string PSObjectTag = "Obj";
        internal const string AdapterProperties = "Props";
        /// TypeNames tag.
        internal const string TypeNamesTag = "TN";
        /// Tag for type item in typenames.
        internal const string TypeNamesItemTag = "T";
        /// TypeName reference.
        internal const string TypeNamesReferenceTag = "TNRef";
        /// Memberset.
        internal const string MemberSet = "MS";
        /// Individual notes.
        internal const string NoteProperty = "N";
        /// Tag for ToString value.
        internal const string ToStringElementTag = "ToString";
        /// Element tag used for IEnumerables.
        internal const string CollectionTag = "IE";
        /// Element tag used for Dictionary.
        internal const string DictionaryTag = "DCT";
        /// Element tag used for Dictionary entry.
        internal const string DictionaryEntryTag = "En";
        /// Element tag used for Stack.
        internal const string StackTag = "STK";
        /// Element tag used for Queue.
        internal const string QueueTag = "QUE";
        /// Element tag used for List.
        internal const string ListTag = "LST";
        #endregion known container tags
        #region primitive known type tags
        /// Element tag for char property.
        /// <remarks>This property is used for System.Char type</remarks>
        internal const string CharTag = "C";
        /// Element tag for guid property.
        /// <remarks>This property is used for System.Guid type</remarks>
        internal const string GuidTag = "G";
        /// Element tag for boolean property.
        /// <remarks>This property is used for System.Boolean type</remarks>
        internal const string BooleanTag = "B";
        /// Element tag for unsignedByte property.
        /// <remarks>This property is used for System.Byte type</remarks>
        internal const string UnsignedByteTag = "By";
        /// Element tag for dateTime property.
        /// <remarks>This property is used for System.DateTime type</remarks>
        internal const string DateTimeTag = "DT";
        /// Element tag for decimal property.
        /// <remarks>This property is used for System.Decimal type</remarks>
        internal const string DecimalTag = "D";
        /// Element tag for double property.
        /// <remarks>This property is used for System.Double type</remarks>
        internal const string DoubleTag = "Db";
        /// Element tag for duration property.
        /// <remarks>This property is used for System.TimeSpan type</remarks>
        internal const string DurationTag = "TS";
        /// Element tag for float property.
        /// <remarks>This property is used for System.Single type</remarks>
        internal const string FloatTag = "Sg";
        /// Element tag for int property.
        /// <remarks>This property is used for System.Int32 type</remarks>
        internal const string IntTag = "I32";
        /// Element tag for long property.
        /// <remarks>This property is used for System.Int64 type</remarks>
        internal const string LongTag = "I64";
        /// Element tag for byte property.
        /// <remarks>This property is used for System.SByte type</remarks>
        internal const string ByteTag = "SB";
        /// Element tag for short property.
        /// <remarks>This property is used for System.Int16 type</remarks>
        internal const string ShortTag = "I16";
        /// Element tag for base64Binary property.
        /// <remarks>This property is used for System.IO.Stream type</remarks>
        internal const string Base64BinaryTag = "BA";
        /// Element tag for scriptblock property.
        /// <remarks>This property is used for System.Management.Automation.ScriptBlock type</remarks>
        internal const string ScriptBlockTag = "SBK";
        /// Element tag for string property.
        /// <remarks>This property is used for System.String type</remarks>
        internal const string StringTag = "S";
        /// Element tag for secure string property.
        /// <remarks>This property is used for System.Security.SecureString type</remarks>
        internal const string SecureStringTag = "SS";
        /// Element tag for unsignedShort property.
        /// <remarks>This property is used for System.UInt16 Stream type</remarks>
        internal const string UnsignedShortTag = "U16";
        /// Element tag for unsignedInt property.
        /// <remarks>This property is used for System.UInt32 type</remarks>
        internal const string UnsignedIntTag = "U32";
        /// Element tag for unsignedLong property.
        /// <remarks>This property is used for System.Long type</remarks>
        internal const string UnsignedLongTag = "U64";
        /// Element tag for anyUri property.
        /// <remarks>This property is used for System.Uri type</remarks>
        internal const string AnyUriTag = "URI";
        /// Element tag for Version property.
        internal const string VersionTag = "Version";
        /// Element tag for SemanticVersion property.
        internal const string SemanticVersionTag = "SemanticVersion";
        /// Element tag for XmlDocument.
        internal const string XmlDocumentTag = "XD";
        /// Element tag for property whose value is null.
        internal const string NilTag = "Nil";
        /// Element tag for PSObjectReference property.
        /// <remarks>This property is used for a reference to a property bag</remarks>
        internal const string ReferenceTag = "Ref";
        #region progress record
        internal const string ProgressRecord = "PR";
        internal const string ProgressRecordActivityId = "AI";
        internal const string ProgressRecordParentActivityId = "PI";
        internal const string ProgressRecordActivity = "AV";
        internal const string ProgressRecordStatusDescription = "SD";
        internal const string ProgressRecordCurrentOperation = "CO";
        internal const string ProgressRecordPercentComplete = "PC";
        internal const string ProgressRecordSecondsRemaining = "SR";
        internal const string ProgressRecordType = "T";
        #endregion progress record
        #endregion primitive known type tags
        #endregion element tags
        /// String for reference id attribute.
        internal const string ReferenceIdAttribute = "RefId";
        internal const string NameAttribute = "N";
        /// String for version attribute.
        internal const string VersionAttribute = "Version";
        /// String for stream attribute.
        internal const string StreamNameAttribute = "S";
        #endregion attribute tags
        #region namespace values
        /// Monad namespace.
        internal const string MonadNamespace = "http://schemas.microsoft.com/powershell/2004/04";
        /// Prefix string for monad namespace.
        internal const string MonadNamespacePrefix = "ps";
        #endregion namespace values
