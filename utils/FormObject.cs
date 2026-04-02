    /// FormObject used in HtmlWebResponseObject.
    public class FormObject
        /// Gets the Id property.
        public string Id { get; }
        /// Gets the Method property.
        public string Method { get; }
        /// Gets the Action property.
        public string Action { get; }
        /// Gets the Fields property.
        public Dictionary<string, string> Fields { get; }
        /// Initializes a new instance of the <see cref="FormObject"/> class.
        /// <param name="id"></param>
        /// <param name="method"></param>
        public FormObject(string id, string method, string action)
            Method = method;
            Action = action;
            Fields = new Dictionary<string, string>();
        internal void AddField(string key, string value)
            if (key is not null && !Fields.TryGetValue(key, out string? _))
                Fields[key] = value;
