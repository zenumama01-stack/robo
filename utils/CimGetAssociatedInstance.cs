    /// Implements operations of get-AssociatedInstance cmdlet.
    internal sealed class CimGetAssociatedInstance : CimAsyncOperation
        /// Initializes a new instance of the <see cref="CimGetAssociatedInstance"/> class.
        public CimGetAssociatedInstance()
            : base()
        /// Base on parametersetName to retrieve associated ciminstances
        /// <param name="cmdlet"><see cref="GetCimInstanceCommand"/> object.</param>
        public void GetCimAssociatedInstance(GetCimAssociatedInstanceCommand cmdlet)
            IEnumerable<string> computerNames = ConstValue.GetComputerNames(cmdlet.ComputerName);
            // use the namespace from parameter
            string nameSpace = cmdlet.Namespace;
            if ((nameSpace == null) && (cmdlet.ResourceUri == null))
                // try to use namespace of ciminstance, then fall back to default namespace
                nameSpace = ConstValue.GetNamespace(cmdlet.CimInstance.CimSystemProperties.Namespace);
            List<CimSessionProxy> proxys = new();
            switch (cmdlet.ParameterSetName)
                case CimBaseCommand.ComputerSetName:
                    foreach (string computerName in computerNames)
                        CimSessionProxy proxy = CreateSessionProxy(computerName, cmdlet.CimInstance, cmdlet);
                        proxys.Add(proxy);
                case CimBaseCommand.SessionSetName:
                    foreach (CimSession session in cmdlet.CimSession)
                        CimSessionProxy proxy = CreateSessionProxy(session, cmdlet);
            foreach (CimSessionProxy proxy in proxys)
                proxy.EnumerateAssociatedInstancesAsync(
                    nameSpace,
                    cmdlet.CimInstance,
                    cmdlet.Association,
                    cmdlet.ResultClassName,
                    null,
                    null);
        #region private methods
        /// Set <see cref="CimSessionProxy"/> properties
        /// <param name="cmdlet"></param>
        private static void SetSessionProxyProperties(
            ref CimSessionProxy proxy,
            GetCimAssociatedInstanceCommand cmdlet)
            proxy.OperationTimeout = cmdlet.OperationTimeoutSec;
            proxy.KeyOnly = cmdlet.KeyOnly;
            if (cmdlet.ResourceUri != null)
                proxy.ResourceUri = cmdlet.ResourceUri;
        /// Create <see cref="CimSessionProxy"/> and set properties
        private CimSessionProxy CreateSessionProxy(
            string computerName,
            CimInstance cimInstance,
            CimSessionProxy proxy = CreateCimSessionProxy(computerName, cimInstance);
            SetSessionProxyProperties(ref proxy, cmdlet);
        /// Create <see cref="CimSessionProxy"/> and set properties.
            CimSession session,
            CimSessionProxy proxy = CreateCimSessionProxy(session);
