    #region Get-WSManInstance
    [Cmdlet(VerbsCommon.Get, "WSManInstance", DefaultParameterSetName = "GetInstance", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096627")]
    public class GetWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
        #region parameter
        [Parameter(ParameterSetName = "GetInstance")]
        [Parameter(ParameterSetName = "Enumerate")]
                return applicationname;
                { applicationname = value; }
        /// The following is the definition of the input parameter "BasePropertiesOnly".
        /// Enumerate only those properties that are part of the base class
        /// specification in the Resource URI. When
        /// Shallow is specified then this flag has no effect.
        [Alias("UBPO", "Base")]
        public SwitchParameter BasePropertiesOnly
                return basepropertiesonly;
                { basepropertiesonly = value; }
        private SwitchParameter basepropertiesonly;
        /// Specifies the transport, server, port, and Prefix, needed to connect to the
        /// remote machine. The format of this string is:
        /// transport://server:port/Prefix.
                  ParameterSetName = "GetInstance")]
                  ParameterSetName = "Enumerate")]
                return connectionuri;
                { connectionuri = value; }
        /// The following is the definition of the input parameter "Dialect".
        /// Defines the dialect for the filter predicate.
        public Uri Dialect
                return dialect;
                { dialect = value; }
        private Uri dialect;
        /// The following is the definition of the input parameter "Enumerate".
        /// Switch indicates list all instances of a management resource. Equivalent to
        /// WSManagement Enumerate.
        public SwitchParameter Enumerate
                return enumerate;
                { enumerate = value; }
        private SwitchParameter enumerate;
        /// Indicates the filter expression for the enumeration.
                { filter = value; }
        /// The following is the definition of the input parameter "Fragment".
        /// Specifies a section inside the instance that is to be updated or retrieved
        /// for the given operation.
        public string Fragment
                return fragment;
                { fragment = value; }
        private string fragment;
                return optionset;
                { optionset = value; }
                { port = value; }
        /// The following is the definition of the input parameter "Associations".
        /// Associations indicates retrieval of association instances as opposed to
        /// associated instances. This can only be used when specifying the Dialect as
        /// Association.
        public SwitchParameter Associations
                return associations;
                { associations = value; }
        private SwitchParameter associations;
        [Alias("RURI")]
                return resourceuri;
                { resourceuri = value; }
        /// The following is the definition of the input parameter "ReturnType".
        /// Indicates the type of data returned. Possible options are 'Object', 'EPR',
        /// and 'ObjectAndEPR'. Default is Object.
        /// If Object is specified or if this parameter is absent then only the objects
        /// are returned
        /// If EPR is specified then only the EPRs of the objects
        /// are returned. EPRs contain information about the Resource URI and selectors
        /// for the instance
        /// If ObjectAndEPR is specified, then both the object and the associated EPRs
        /// are returned.
        [ValidateSet(new string[] { "object", "epr", "objectandepr" })]
        [Alias("RT")]
        public string ReturnType
                return returntype;
                { returntype = value; }
        private string returntype = "object";
                return selectorset;
                { selectorset = value; }
        /// Defines a set of extended options for the WSMan session.  This can be
        /// created by using the cmdlet New-WSManSessionOption.
                return sessionoption;
                { sessionoption = value; }
        /// Enumerate only instances of the base class specified in the resource URI. If
        /// this flag is not specified, instances of the base class specified in the URI
        /// and all its derived classes are returned.
                { shallow = value; }
        [Alias("SSL")]
                { usessl = value; }
        #endregion parameter
        private string GetFilter()
            string name;
            string value;
            string[] Split = filter.Trim().Split(new char[] { '=', ';' });
            if ((Split.Length) % 2 != 0)
                // mismatched property name/value pair
            filter = "<wsman:SelectorSet xmlns:wsman='http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd'>";
            for (int i = 0; i < Split.Length; i += 2)
                value = Split[i + 1].Substring(1, Split[i + 1].Length - 2);
                name = Split[i];
                filter = filter + "<wsman:Selector Name='" + name + "'>" + value + "</wsman:Selector>";
            filter += "</wsman:SelectorSet>";
            return (filter);
        private void ReturnEnumeration(IWSManEx wsmanObject, IWSManResourceLocator wsmanResourceLocator, IWSManSession wsmanSession)
            string fragment;
                int flags = 0;
                IWSManEnumerator obj;
                if (returntype != null)
                    if (returntype.Equals("object", StringComparison.OrdinalIgnoreCase))
                        flags = wsmanObject.EnumerationFlagReturnObject();
                    else if (returntype.Equals("epr", StringComparison.OrdinalIgnoreCase))
                        flags = wsmanObject.EnumerationFlagReturnEPR();
                        flags = wsmanObject.EnumerationFlagReturnObjectAndEPR();
                if (shallow)
                    flags |= wsmanObject.EnumerationFlagHierarchyShallow();
                else if (basepropertiesonly)
                    flags |= wsmanObject.EnumerationFlagHierarchyDeepBasePropsOnly();
                    flags |= wsmanObject.EnumerationFlagHierarchyDeep();
                if (dialect != null && filter != null)
                    if (dialect.ToString().Equals(helper.ALIAS_WQL, StringComparison.OrdinalIgnoreCase) || dialect.ToString().Equals(helper.URI_WQL_DIALECT, StringComparison.OrdinalIgnoreCase))
                        fragment = helper.URI_WQL_DIALECT;
                        dialect = new Uri(fragment);
                    else if (dialect.ToString().Equals(helper.ALIAS_ASSOCIATION, StringComparison.OrdinalIgnoreCase) || dialect.ToString().Equals(helper.URI_ASSOCIATION_DIALECT, StringComparison.OrdinalIgnoreCase))
                        if (associations)
                            flags |= wsmanObject.EnumerationFlagAssociationInstance();
                            flags |= wsmanObject.EnumerationFlagAssociatedInstance();
                        fragment = helper.URI_ASSOCIATION_DIALECT;
                    else if (dialect.ToString().Equals(helper.ALIAS_SELECTOR, StringComparison.OrdinalIgnoreCase) || dialect.ToString().Equals(helper.URI_SELECTOR_DIALECT, StringComparison.OrdinalIgnoreCase))
                        filter = GetFilter();
                        fragment = helper.URI_SELECTOR_DIALECT;
                    obj = (IWSManEnumerator)wsmanSession.Enumerate(wsmanResourceLocator, filter, dialect.ToString(), flags);
                else if (filter != null)
                    obj = (IWSManEnumerator)wsmanSession.Enumerate(wsmanResourceLocator, filter, null, flags);
                while (!obj.AtEndOfStream)
                    xmldoc.LoadXml(obj.ReadItem());
                ErrorRecord er = new ErrorRecord(ex, "Exception", ErrorCategory.InvalidOperation, null);
            IWSManSession m_session = null;
            IWSManEx m_wsmanObject = (IWSManEx)new WSManClass();
            helper.WSManOp = "Get";
            string connectionStr = null;
                    // in the format http(s)://server[:port/applicationname]
                IWSManResourceLocator m_resource = helper.InitializeResourceLocator(optionset, selectorset, fragment, dialect, m_wsmanObject, resourceuri);
                if (!enumerate)
                        xmldoc.LoadXml(m_session.Get(m_resource, 0));
                        helper.AssertError(ex.Message, false, computername);
                    if (!string.IsNullOrEmpty(fragment))
                        WriteObject(xmldoc.FirstChild.LocalName + "=" + xmldoc.FirstChild.InnerText);
                        ReturnEnumeration(m_wsmanObject, m_resource, m_session);
    #region Set-WsManInstance
    /// Set-WSManInstance -Action StartService -ResourceURI wmicimv2/Win32_Service
    [Cmdlet(VerbsCommon.Set, "WSManInstance", DefaultParameterSetName = "ComputerName", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096937")]
    [OutputType(typeof(XmlElement), typeof(string))]
    public class SetWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
            get { return dialect; }
            set { dialect = value; }
            get { return fragment; }
            set { fragment = value; }
        /// OptionSet is a hahs table which help modify or refine the nature of the
        /// request. These are similar to switches used in command line shells in that
        /// they are service-specific.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resourceuri")]
        /// Defines a set of extended options for the WSMan session. This can be created
        /// by using the cmdlet New-WSManSessionOption.
        [Alias("ssl")]
        /// ValueSet is a hash table which helps to modify resource represented by the
            helper.WSManOp = "set";
            if (dialect != null)
                if (dialect.ToString().Equals(helper.ALIAS_WQL, StringComparison.OrdinalIgnoreCase))
                    dialect = new Uri(helper.URI_WQL_DIALECT);
                if (dialect.ToString().Equals(helper.ALIAS_SELECTOR, StringComparison.OrdinalIgnoreCase))
                    dialect = new Uri(helper.URI_SELECTOR_DIALECT);
                if (dialect.ToString().Equals(helper.ALIAS_ASSOCIATION, StringComparison.OrdinalIgnoreCase))
                    dialect = new Uri(helper.URI_ASSOCIATION_DIALECT);
                string rootNode = helper.GetRootNodeName(helper.WSManOp, m_resource.ResourceUri, null);
                    xmldoc.LoadXml(m_session.Put(m_resource, input, 0));
                    if (xmldoc.DocumentElement.ChildNodes.Count > 0)
                        foreach (XmlNode node in xmldoc.DocumentElement.ChildNodes)
                            if (node.Name.Equals(fragment, StringComparison.OrdinalIgnoreCase))
                                WriteObject(node.Name + " = " + node.InnerText);
    #region Remove-WsManInstance
    [Cmdlet(VerbsCommon.Remove, "WSManInstance", DefaultParameterSetName = "ComputerName", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096721")]
    public class RemoveWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
            helper.WSManOp = "remove";
                string ResourceURI = helper.GetURIWithFilter(resourceuri.ToString(), null, selectorset, helper.WSManOp);
                    ((IWSManSession)m_session).Delete(ResourceURI, 0);
    #region New-WsManInstance
    /// Creates an instance of a management resource identified by the resource URI
    /// using specified ValueSet or input File.
    [Cmdlet(VerbsCommon.New, "WSManInstance", DefaultParameterSetName = "ComputerName", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096933")]
    public class NewWSManInstanceCommand : AuthenticatingWSManCommand, IDisposable
        [Parameter(Mandatory = true, Position = 1,
        /// Defines a set of extended options for the WSMan session.
            helper.WSManOp = "new";
                    string resultXml = m_session.Create(m_resource, input, 0);
