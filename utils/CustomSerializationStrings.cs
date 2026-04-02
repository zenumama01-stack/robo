    /// This class contains strings required for serialization for ConvertTo-XML.
    internal static class CustomSerializationStrings
        #region element tags
        /// Element tag for root node.
        internal const string RootElementTag = "Objects";
        /// Element tag for PSObject.
        internal const string PSObjectTag = "Object";
        /// Element tag for properties.
        internal const string Properties = "Property";
        #region attribute tags
        /// String for name attribute.
        internal const string NameAttribute = "Name";
        /// String for type attribute.
        internal const string TypeAttribute = "Type";
        #region known container tags
        /// Value of name attribute for dictionary key part in dictionary entry.
        internal const string DictionaryKey = "Key";
        /// Value of name attribute for dictionary value part in dictionary entry.
        internal const string DictionaryValue = "Value";
