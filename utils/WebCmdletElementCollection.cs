    /// WebCmdletElementCollection for elements in html web responses.
    public class WebCmdletElementCollection : ReadOnlyCollection<PSObject>
        internal WebCmdletElementCollection(IList<PSObject> list) : base(list)
        /// Finds the element with name or id.
        /// <param name="nameOrId"></param>
        /// <returns>Found element as PSObject.</returns>
        public PSObject? Find(string nameOrId) => FindById(nameOrId) ?? FindByName(nameOrId);
        /// Finds the element by id.
        public PSObject? FindById(string id) => Find(id, findById: true);
        /// Finds the element by name.
        public PSObject? FindByName(string name) => Find(name, findById: false);
        private PSObject? Find(string nameOrId, bool findById)
            foreach (PSObject candidate in this)
                var namePropInfo = candidate.Properties[(findById ? "id" : "name")];
                if (namePropInfo != null && (string)namePropInfo.Value == nameOrId)
                    return candidate;
