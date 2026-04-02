    /// Class that queries the server and gets current configurations.
    /// Also provides a generic way to update the configurations.
    internal sealed class CurrentConfigurations
        /// Prefix used to add NameSpace of root element to namespace manager.
        public const string DefaultNameSpacePrefix = "defaultNameSpace";
        /// This holds the current configurations XML.
        private readonly XmlDocument rootDocument;
        /// Holds the reference to the current document element.
        private XmlElement documentElement;
        /// Holds the Namespace Manager to use for XPATH queries.
        private XmlNamespaceManager nameSpaceManger;
        /// Session of the WsMan sserver.
        private readonly IWSManSession serverSession;
        /// Gets the server session associated with the configuration.
        public IWSManSession ServerSession
            get { return serverSession; }
        /// Gets the current configuration XML.
        public XmlDocument RootDocument
            get { return this.rootDocument; }
        /// Gets the current configuration on the given server and for given URI.
        /// This issues a GET request to the server.
        /// <param name="serverSession">Current server session.</param>
        public CurrentConfigurations(IWSManSession serverSession)
            ArgumentNullException.ThrowIfNull(serverSession);
            this.rootDocument = new XmlDocument();
            this.serverSession = serverSession;
        /// Refresh the CurrentConfiguration. This method calls GET operation for the given
        /// URI on the server and update the current configuration. It also initialize some
        /// of required class members.
        /// <param name="responseOfGet">Plugin configuration.</param>
        /// <returns>False, if operation failed.</returns>
        public bool RefreshCurrentConfiguration(string responseOfGet)
            ArgumentException.ThrowIfNullOrEmpty(responseOfGet);
            this.rootDocument.LoadXml(responseOfGet);
            this.documentElement = this.rootDocument.DocumentElement;
            this.nameSpaceManger = new XmlNamespaceManager(this.rootDocument.NameTable);
            this.nameSpaceManger.AddNamespace(CurrentConfigurations.DefaultNameSpacePrefix, this.documentElement.NamespaceURI);
            return string.IsNullOrEmpty(this.serverSession.Error);
        /// Update the server with updated XML.
        /// Issues a PUT request with the ResourceUri provided.
        /// <param name="resourceUri">Resource URI to use.</param>
        /// <returns>False, if operation is not successful.</returns>
        public void PutConfigurationOnServer(string resourceUri)
            ArgumentException.ThrowIfNullOrEmpty(resourceUri);
            this.serverSession.Put(resourceUri, this.rootDocument.InnerXml, 0);
        /// This method will remove the configuration from the XML.
        /// Currently the method will only remove the attributes. But it is extensible enough to support
        /// Node removals in future.
        /// <param name="pathToNodeFromRoot">Path with namespace to the node from Root element. Must not end with '/'.</param>
        public void RemoveOneConfiguration(string pathToNodeFromRoot)
            ArgumentNullException.ThrowIfNull(pathToNodeFromRoot);
            XmlNode nodeToRemove =
                this.documentElement.SelectSingleNode(
                    pathToNodeFromRoot,
                    this.nameSpaceManger);
            if (nodeToRemove != null)
                if (nodeToRemove is XmlAttribute)
                    RemoveAttribute(nodeToRemove as XmlAttribute);
                throw new ArgumentException("Node is not present in the XML, Please give valid XPath", nameof(pathToNodeFromRoot));
        /// Create or Update the value of the configuration on the given Node. Currently this
        /// method is supported for updating attributes, but can be easily updated for nodes.
        /// Caller should call this method to add a new attribute to the Node.
        /// <param name="configurationName">Name of the configuration with name space to update or create.</param>
        /// <param name="configurationValue">Value of the configurations.</param>
        public void UpdateOneConfiguration(string pathToNodeFromRoot, string configurationName, string configurationValue)
            ArgumentException.ThrowIfNullOrEmpty(configurationName);
            ArgumentNullException.ThrowIfNull(configurationValue);
            XmlNode nodeToUpdate =
            if (nodeToUpdate != null)
                foreach (XmlAttribute attribute in nodeToUpdate.Attributes)
                    if (attribute.Name.Equals(configurationName, StringComparison.OrdinalIgnoreCase))
                        attribute.Value = configurationValue;
                XmlNode attr = this.rootDocument.CreateNode(XmlNodeType.Attribute, configurationName, string.Empty);
                attr.Value = configurationValue;
                nodeToUpdate.Attributes.SetNamedItem(attr);
        /// Gets the value of the configuration on the given Node or attribute.
        /// <param name="pathFromRoot">Path with namespace to the node from Root element.</param>
        /// <returns>Value of the Node, or Null if no node present.</returns>
        public string GetOneConfiguration(string pathFromRoot)
            ArgumentNullException.ThrowIfNull(pathFromRoot);
            XmlNode requiredNode =
                    pathFromRoot,
            if (requiredNode != null)
                return requiredNode.Value;
        /// Removes the attribute from OwnerNode.
        /// <param name="attributeToRemove">Attribute to Remove.</param>
        private static void RemoveAttribute(XmlAttribute attributeToRemove)
            XmlElement ownerElement = attributeToRemove.OwnerElement;
            ownerElement.RemoveAttribute(attributeToRemove.Name);
