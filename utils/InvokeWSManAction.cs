    /// Executes action on a target object specified by RESOURCE_URI, where
    /// parameters are specified by key value pairs.
    /// eg., Call StartService method on the spooler service
    /// Invoke-WSManAction -Action StartService -ResourceURI wmicimv2/Win32_Service
    /// -SelectorSet {Name=Spooler}
    [Cmdlet(VerbsLifecycle.Invoke, "WSManAction", DefaultParameterSetName = "URI", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096843")]
    public class InvokeWSManActionCommand : AuthenticatingWSManCommand, IDisposable
        /// The following is the definition of the input parameter "Action".
        /// Indicates the method which needs to be executed on the management object
        /// specified by the ResourceURI and selectors.
        public string Action
            get { return action; }
            set { action = value; }
        private string action;
        [Parameter(ParameterSetName = "ComputerName")]
        private string applicationname = null;
        /// Executes the management operation on the specified computer(s). The default
        /// is the local computer. Type the fully qualified domain name, NETBIOS name or
        /// IP address to indicate the remote host(s)
        [Alias("cn")]
                if ((string.IsNullOrEmpty(computername)) || (computername.Equals(".", StringComparison.OrdinalIgnoreCase)))
                    computername = "localhost";
        private string computername = null;
        [Parameter(ParameterSetName = "URI")]
        [Alias("CURI", "CU")]
        /// The following is the definition of the input parameter "FilePath".
        /// Updates the management resource specified by the ResourceURI and SelectorSet
        /// via this input file.
            get { return filepath; }
            set { filepath = value; }
        private string filepath;
        /// OptionSet is a hashtable and is used to pass a set of switches to the
        [Parameter(ValueFromPipeline = true,
        /// The following is the definition of the input parameter "SelectorSet".
        /// SelectorSet is a hash table which helps in identify an instance of the
        /// management resource if there are more than 1 instance of the resource
        [Parameter(Position = 2,
        public Hashtable SelectorSet
            get { return selectorset; }
            set { selectorset = value; }
        private Hashtable selectorset;
        /// Defines a set of extended options for the WSMan session. This hashtable can
        [Alias("so")]
        /// The following is the definition of the input parameter "ValueSet".
        /// ValueSet is a hahs table which helps to modify resource represented by the
        /// ResourceURI and SelectorSet.
        public Hashtable ValueSet
            get { return valueset; }
            set { valueset = value; }
        private Hashtable valueset;
        /// The following is the definition of the input parameter "ResourceURI".
        /// URI of the resource class/instance representation.
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
        [Alias("ruri")]
        public Uri ResourceURI
            get { return resourceuri; }
            set { resourceuri = value; }
        private Uri resourceuri;
        private readonly IWSManEx m_wsmanObject = (IWSManEx)new WSManClass();
        private IWSManSession m_session = null;
        private string connectionStr = string.Empty;
            helper.WSManOp = "invoke";
            // create the connection string
            connectionStr = helper.CreateConnectionString(connectionuri, port, computername, applicationname);
                // create the resourcelocator object
                IWSManResourceLocator m_resource = helper.InitializeResourceLocator(optionset, selectorset, null, null, m_wsmanObject, resourceuri);
                // create the session object
                m_session = helper.CreateSessionObject(m_wsmanObject, Authentication, sessionoption, Credential, connectionStr, CertificateThumbprint, usessl.IsPresent);
                string rootNode = helper.GetRootNodeName(helper.WSManOp, m_resource.ResourceUri, action);
                string input = helper.ProcessInput(m_wsmanObject, filepath, helper.WSManOp, rootNode, valueset, m_resource, m_session);
                string resultXml = m_session.Invoke(action, m_resource, input, 0);
                xmldoc.LoadXml(resultXml);
                WriteObject(xmldoc.DocumentElement);
                if (!string.IsNullOrEmpty(m_wsmanObject.Error))
                    helper.AssertError(m_wsmanObject.Error, true, resourceuri);
                if (!string.IsNullOrEmpty(m_session.Error))
                    helper.AssertError(m_session.Error, true, resourceuri);
                if (m_session != null)
                    Dispose(m_session);
            //  WSManHelper helper = new WSManHelper();
            helper.CleanUp();
